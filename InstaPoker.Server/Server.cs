using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using InstaPoker.Core;
using InstaPoker.Core.Messages;

namespace InstaPoker.Server;

public class Server {

    private const int Port = 12512;
    
    private readonly UserManager userManager = new();
    private readonly Router messageRouter = new();
    private readonly RoomManager roomManager = new();

    public Server() {
        messageRouter.UserManager = userManager;
        messageRouter.RoomManager = roomManager;
    }
    
    public async Task Run() {
        Console.WriteLine("Starting Server");
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
        // esse codigo roda numa 'thread' do cliente
        NetworkStream ns = client.GetStream();
        await using BinaryWriter bw = new(ns, Encoding.UTF8, true);
        using BinaryReader br = new(ns, Encoding.UTF8, true);
        bw.Write("InstaPoker.Server");
        bw.Write(Assembly.GetExecutingAssembly().GetName().Version!);

        string clientId = br.ReadString();
        if (clientId != "InstaPoker.Client") {
            bw.Write($"Client provided invalid client id: \"{clientId}\". Expected: \"InstaPoker.Client\"");
            client.Close();
            return;
        }

        Version clientVersion = br.ReadVersion();
        // add validation for outdated protocols

        string username = br.ReadString();
        Console.WriteLine($"User identified as {username} using client version {clientVersion}");
        ClientConnection conn = new() {
            Client = client,
            Username = username,
            NetworkStream = client.GetStream(),
            MessageWriter = new MessageWriter(client.GetStream(), true)
        };
        userManager.AddNewConnection(conn);
        await conn.StartReceivingAsync();
    }
}