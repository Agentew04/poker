using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using InstaPoker.Core;
using InstaPoker.Core.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InstaPoker.Server;

/// <summary>
/// Accepts TCP connections and upgrades each one to TLS. Runs as a hosted service so the host owns its
/// lifecycle: the accept loop stops when the host shuts down, and active connections are closed gracefully.
/// </summary>
public sealed class Server(
    IOptions<ServerOptions> options,
    UserManager userManager,
    MessageBus messageBus,
    X509Certificate2 certificate,
    ILogger<Server> logger,
    ILoggerFactory loggerFactory) : BackgroundService {

    private readonly ConcurrentDictionary<long, Task> connectionTasks = new();
    private long nextConnectionId;
    private TcpListener? listener;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        int port = options.Value.Port;
        logger.LogInformation("Loaded certificate {subject} (valid until {valid:yyyy-MM-dd})",
            certificate.Subject, certificate.NotAfter);

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        logger.LogInformation("Listening for incoming TCP connections @ {port}", port);

        try {
            while (!stoppingToken.IsCancellationRequested) {
                TcpClient client;
                try {
                    client = await listener.AcceptTcpClientAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                    break;
                }
                catch (SocketException e) when (stoppingToken.IsCancellationRequested) {
                    logger.LogDebug(e, "Listener stopped while accepting a connection");
                    break;
                }

                logger.LogInformation("Received connection from {endpoint}", client.Client.RemoteEndPoint);

                long id = Interlocked.Increment(ref nextConnectionId);
                Task task = HandleClientAsync(client, stoppingToken);
                connectionTasks[id] = task;
                _ = task.ContinueWith(_ => {
                    connectionTasks.TryRemove(id, out Task? _);
                }, TaskScheduler.Default);
            }
        }
        finally {
            listener.Stop();
            logger.LogInformation("Stopped listening; closing active connections");
            userManager.DisconnectAll();
            await WaitForConnectionsAsync();
            messageBus.Complete();
            logger.LogInformation("Server stopped");
        }
    }

    private async Task WaitForConnectionsAsync() {
        Task[] pending = connectionTasks.Values.ToArray();
        if (pending.Length == 0) {
            return;
        }

        try {
            await Task.WhenAll(pending).WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException) {
            logger.LogWarning("{count} connection(s) did not finish within the shutdown timeout",
                pending.Count(task => !task.IsCompleted));
        }
        catch (Exception e) {
            logger.LogWarning(e, "One or more connections failed during shutdown");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken) {
        string endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        try {
            await HandleClientCoreAsync(client, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            // expected during shutdown
        }
        catch (Exception e) {
            logger.LogError(e, "Connection with {endpoint} failed", endpoint);
        }
        finally {
            client.Close();
        }
    }

    private async Task HandleClientCoreAsync(TcpClient client, CancellationToken stoppingToken) {
        // esse codigo roda numa 'thread' do cliente
        NetworkStream ns = client.GetStream();
        using BinaryWriter bw = new(ns, Encoding.UTF8, true);
        using BinaryReader br = new(ns, Encoding.UTF8, true);
        bw.Write("InstaPoker.Server");
        bw.Write(Assembly.GetExecutingAssembly().GetName().Version!);
        bw.Flush();

        string clientId = br.ReadString();
        if (clientId != "InstaPoker.Client") {
            bw.Write($"Client provided invalid client id: \"{clientId}\". Expected: \"InstaPoker.Client\"");
            bw.Flush();
            return;
        }

        Version clientVersion = br.ReadVersion();
        // add validation for outdated protocols

        string username = br.ReadString();
        logger.LogInformation("User identified as {username} using client version {version}", username, clientVersion);

        await using SslStream sslStream = new(ns, false);
        await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions {
            ServerCertificate = certificate,
            ClientCertificateRequired = false
        }, stoppingToken);
        logger.LogInformation("TLS established with {username}: {protocol}, {cipher}",
            username, sslStream.SslProtocol, sslStream.NegotiatedCipherSuite);

        ClientConnection conn = new() {
            Client = client,
            Username = username,
            Stream = sslStream,
            MessageWriter = new MessageWriter(sslStream, true),
            UserManager = userManager,
            MessageBus = messageBus,
            Logger = loggerFactory.CreateLogger<ClientConnection>(),
            ShutdownToken = stoppingToken
        };
        userManager.AddNewConnection(conn);
        await conn.StartReceivingAsync();
    }
}
