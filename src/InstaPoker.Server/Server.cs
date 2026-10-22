using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using InstaPoker.Core;
using InstaPoker.Core.Messages;

namespace InstaPoker.Server;

public class Server {

    private const int Port = 12512;
    
    private readonly Router messageRouter = new();

    public async Task Run() {
        Console.WriteLine("Starting Server");
        Console.WriteLine("Loaded certificate: {0} (valid until {1:yyyy-MM-dd})",
            ServerTls.Certificate.Subject, ServerTls.Certificate.NotAfter);
        Console.WriteLine($"Listening for incoming TCP connections @ {Port}");
        TcpListener listener = new(new IPEndPoint(IPAddress.Any, Port));

        // listen for new connections
        CancellationTokenSource cts = new();
        
        Task acceptConnLoop = Task.Run(() => AcceptConnectionsLoop(listener, cts.Token), cts.Token);
        Task routingLoop = messageRouter.RouteMessagesLoop(cts.Token);

        // keep server open
        await Task.WhenAll(acceptConnLoop, routingLoop);
    }

    private async Task AcceptConnectionsLoop(TcpListener listener, CancellationToken token) {
        listener.Start();
        while (!token.IsCancellationRequested) {
            TcpClient client = await listener.AcceptTcpClientAsync(token);
            Console.WriteLine($"Received connection from: {client.Client.RemoteEndPoint}");
            _ = Task.Run(() => HandleClient(client), token);
        }
    }

    private async Task HandleClient(TcpClient client) {
        try {
            await HandleClientCore(client);
        }
        catch (Exception e) {
            Console.WriteLine($"Connection with {client.Client.RemoteEndPoint} failed: {e.GetType().Name} {e.Message}");
        }
        finally {
            client.Close();
        }
    }

    private async Task HandleClientCore(TcpClient client) {
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
        Console.WriteLine($"User identified as {username} using client version {clientVersion}");

        await using SslStream sslStream = new(ns, false);
        Console.WriteLine($"Starting TLS handshake with {username}");
        await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions {
            ServerCertificate = ServerTls.Certificate,
            ClientCertificateRequired = false
        });
        Console.WriteLine($"TLS established with {username}: {sslStream.SslProtocol}, {sslStream.NegotiatedCipherSuite}");

        ClientConnection conn = new() {
            Client = client,
            Username = username,
            Stream = sslStream,
            MessageWriter = new MessageWriter(sslStream, true)
        };
        UserManager.AddNewConnection(conn);
        await conn.StartReceivingAsync();
    }
}