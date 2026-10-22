using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using InstaPoker.Core;
using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// Static class that handles connecting to the remote game server and has methods that
/// send and received messages from the server. 
/// </summary>
public class NetworkManager {

    private TcpClient? _client;
    private SslStream? _sslStream;
    private const int Port = 12512;

    private readonly ILogger<NetworkManager> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly MainThreadDispatcher dispatcher;

    private readonly object subscribersLock = new();
    private readonly Dictionary<Type, List<Action<Message>>> subscribers = [];

    /// <summary>
    /// The handler responsible for reading messages from and sending messages to the remote server.
    /// </summary>
    internal MessageHandler? Handler { get; private set; }

    /// <summary>
    /// If the game is currently connected to the server. Ideally this is false when the user is authenticating
    /// and for the rest of the game, true.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Response types the client is currently waiting for.
    /// </summary>
    public IReadOnlyCollection<Type> PendingRequests => Handler?.PendingRequests ?? [];

    internal ServerInfo ServerInfo { get; } = new();

    /// <summary>
    /// Raised on the main thread when the connection to the server is lost.
    /// </summary>
    public event Action<Exception?>? Disconnected;

    public NetworkManager(ILogger<NetworkManager> logger, IServiceProvider serviceProvider,
        MainThreadDispatcher dispatcher) {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.dispatcher = dispatcher;
    }

    /// <summary>
    /// Registers a handler for messages of type <typeparamref name="TMessage"/>.
    /// The handler is always invoked from the main thread, inside <see cref="Pump"/>.
    /// </summary>
    public IDisposable Subscribe<TMessage>(Action<TMessage> handler) where TMessage : Message {
        Action<Message> wrapper = message => handler((TMessage)message);
        lock (subscribersLock) {
            if (!subscribers.TryGetValue(typeof(TMessage), out List<Action<Message>>? handlers)) {
                subscribers[typeof(TMessage)] = handlers = [];
            }
            handlers.Add(wrapper);
        }

        return new Subscription(() => {
            lock (subscribersLock) {
                if (subscribers.TryGetValue(typeof(TMessage), out List<Action<Message>>? handlers)) {
                    handlers.Remove(wrapper);
                }
            }
        });
    }

    /// <summary>
    /// Dispatches everything the server sent since the last frame and runs queued main thread continuations.
    /// Must be called once per frame from the main thread.
    /// </summary>
    public void Pump() {
        Handler?.Pump();
        dispatcher.Pump();
    }

    public async Task<ConnectionResult> ConnectToServer(string username) {
        Disconnect();

        _client = new TcpClient();
        logger.LogInformation("Connecting to the remote server");
        using CancellationTokenSource connectionCts = new(TimeSpan.FromSeconds(20));
        ServerInfo.HostName = "localhost";
        ServerInfo.Port = Port;
        try {
            await _client.ConnectAsync(ServerInfo.HostName, Port, connectionCts.Token);
        }
        catch (OperationCanceledException e) {
            logger.LogError("timeout. could not connect to remote server: {msg} {err}", e.Message, e.GetType().Name);
            return ConnectionResult.Timeout;
        }
        catch (SocketException e) {
            logger.LogError("Host computer rejected connection: {msg} {err}", e.Message, e.GetType().Name);
            return ConnectionResult.ConnectionRefused;
        }

        if (!_client.Connected) {
            logger.LogError("unknown error. could not connect to remote server");
            return ConnectionResult.Failed;
        }

        // the handshake uses blocking reads, so keep it off the main thread
        try {
            _sslStream = await Task.Run(() => ServerHandshake.IdentifyAndSecureAsync(
                _client, ServerInfo.HostName, username,
                Assembly.GetExecutingAssembly().GetName().Version!, logger));
        }
        catch (ServerCertificateMismatchException e) {
            logger.LogError(e, "The server certificate does not match this client. The client must be updated.");
            Disconnect();
            return ConnectionResult.CertificateMismatch;
        }
        catch (Exception e) {
            logger.LogError(e, "Failed to identify with the server");
            Disconnect();
            return ConnectionResult.Failed;
        }

        Handler = new MessageHandler(
            new MessageReader(_sslStream!, true, serviceProvider.GetRequiredService<ILogger<MessageReader>>()),
            new MessageWriter(_sslStream!, true),
            dispatcher,
            serviceProvider.GetRequiredService<ILogger<MessageHandler>>());
        Handler.MessageReceived += OnMessageReceived;
        Handler.Disconnected += OnDisconnected;

        IsConnected = true;
        return ConnectionResult.Success;
    }

    /// <summary>
    /// Closes the connection and stops the network loops. Safe to call when already disconnected.
    /// </summary>
    public void Disconnect() {
        if (Handler is not null) {
            Handler.MessageReceived -= OnMessageReceived;
            Handler.Disconnected -= OnDisconnected;
            Handler.Dispose();
            Handler = null;
        }

        _sslStream?.Dispose();
        _sslStream = null;
        _client?.Close();
        _client = null;
        IsConnected = false;
    }

    public async Task<(string, RoomSettings)> CreateRoom() {
        CreateRoomResponse response = await HandlerOrThrow
            .SendRequest<CreateRoomRequest, CreateRoomResponse>(new CreateRoomRequest());
        logger.LogInformation("Created room with code {code}", response.RoomCode);
        return (response.RoomCode, response.Settings);
    }

    public Task<JoinRoomResponse> JoinRoom(string code) {
        return HandlerOrThrow.SendRequest<JoinRoomRequest, JoinRoomResponse>(new JoinRoomRequest {
            RoomCode = code
        });
    }

    public Task KickUser(string name) {
        return HandlerOrThrow.SendNotification(new KickUserNotification() {
            Username = name
        });
    }

    public Task SendSettings(RoomSettings settings) {
        return HandlerOrThrow.SendNotification(new RoomSettingsChangeNotification() {
            NewSettings = settings
        });
    }

    public Task LeaveRoom() {
        return HandlerOrThrow.SendNotification(new LeaveRoomNotification());
    }

    public Task NotifyGameStart() {
        return HandlerOrThrow.SendNotification(new OwnerStartGameNotification());
    }

    /// <summary>
    /// Queues a message that does not expect a response.
    /// </summary>
    public Task SendNotification<TNotification>(TNotification notification) where TNotification : Message {
        return HandlerOrThrow.SendNotification(notification);
    }

    private MessageHandler HandlerOrThrow =>
        Handler ?? throw new InvalidOperationException("Not connected to a server.");

    private void OnMessageReceived(Message message) {
        Action<Message>[] handlers;
        lock (subscribersLock) {
            if (!subscribers.TryGetValue(message.GetType(), out List<Action<Message>>? registered)) {
                return;
            }
            handlers = registered.ToArray();
        }

        foreach (Action<Message> handler in handlers) {
            try {
                handler(message);
            }
            catch (Exception e) {
                logger.LogError(e, "Unhandled exception in a {message} subscriber", message.GetType().Name);
            }
        }
    }

    private void OnDisconnected(Exception? error) {
        IsConnected = false;
        logger.LogWarning("Disconnected from the server: {reason}", error?.Message ?? "connection closed");
        Disconnected?.Invoke(error);
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable {
        private int disposed;

        public void Dispose() {
            if (Interlocked.Exchange(ref disposed, 1) == 0) {
                unsubscribe();
            }
        }
    }
}

internal class ServerInfo {
    public Version Version { get; set; } = new (0, 0, 0, 0);
    public string HostName { get; set; } = string.Empty;
    public int Port {get;set;}
}
