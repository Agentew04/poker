using System.Net.Sockets;
using System.Reflection;
using System.Text;
using InstaPoker.Core;
using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Client.Network;

/// <summary>
/// Static class that handles connecting to the remote game server and has methods that
/// send and received messages from the server. 
/// </summary>
public static class NetworkManager {

    private static TcpClient? _client;
    private const int Port = 12512;
    
    /// <summary>
    /// The handler responsible for reading messages from and sending messages to the remote server.
    /// </summary>
    public static MessageHandler? Handler { get; private set; }
    
    /// <summary>
    /// If the game is currently connected to the server. Ideally this is false when the user is authenticating
    /// and for the rest of the game, true.
    /// </summary>
    public static bool IsConnected { get; private set; }

    internal static ServerInfo ServerInfo { get; private set; } = new();
    
    public static async Task<bool> ConnectToServer(string username) {
        _client = new TcpClient();
        Console.WriteLine("Connecting to remote server");
        using CancellationTokenSource connectionCts = new(TimeSpan.FromSeconds(20));
        ServerInfo.HostName = "localhost";
        ServerInfo.Port = Port;
        try {
            await _client.ConnectAsync(ServerInfo.HostName, Port, connectionCts.Token);
        }
        catch (OperationCanceledException e) {
            Console.WriteLine("timeout. could not connect to remote server: " + e.Message + " " + e.GetType().Name);
            return false;
        }
        catch (SocketException e) {
            Console.WriteLine("Host computer rejected connection: " + e.Message + " " + e.GetType().Name);
            return false;
        }

        if (!_client.Connected) {
            Console.WriteLine("unknown error. could not connect to remote server");
            return false;
        }

        IsConnected = true;

        NetworkStream ns = _client.GetStream();
        await using BinaryWriter bw = new (ns, Encoding.UTF8, true);
        using BinaryReader br = new(ns, Encoding.UTF8, true);
        
        bw.Write("InstaPoker.Client"); // send client id
        bw.Write(Assembly.GetExecutingAssembly().GetName().Version!);
        bw.Write(username);
        
        string serverId = br.ReadString(); 
        if (serverId != "InstaPoker.Server") {
            Console.WriteLine($"Could not recognize server id. Received \"{serverId}\". Expected: \"InstaPoker.Server\"");
            _client.Close();
            return false;
        }

        Version serverVersion = br.ReadVersion();
        Console.WriteLine($"Server is at version {serverVersion.ToString()}");

        Handler = new MessageHandler(ns, new MessageWriter(ns, true), new MessageReader(ns, true));
        return true;
    }

    public static Task<(string, RoomSettings)> CreateRoom() {
        return Task.Run(async () => {
            CreateRoomResponse response = await Handler!.SendRequest<CreateRoomRequest, CreateRoomResponse>(new CreateRoomRequest());
            Console.WriteLine("Created room with code " + response.RoomCode);
            return (response.RoomCode,response.Settings);
        });
    }
    
    public static Task<JoinRoomResponse> JoinRoom(string code) {
        return Handler!.SendRequest<JoinRoomRequest, JoinRoomResponse>(new JoinRoomRequest {
            RoomCode = code
        });
    }

    public static Task KickUser(string name) {
        return Handler!.SendNotification(new KickUserNotification() {
            Username = name
        });
    }

    public static Task SendSettings(RoomSettings settings) {
        return Handler!.SendNotification(new RoomSettingsChangeNotification() {
            NewSettings = settings
        });
    }

    public static Task LeaveRoom() {
        return Handler!.SendNotification(new LeaveRoomNotification());
    }
}

internal class ServerInfo {
    public Version Version { get; set; } = new (0, 0, 0, 0);
    public string HostName { get; set; } = string.Empty;
    public int Port {get;set;}
}