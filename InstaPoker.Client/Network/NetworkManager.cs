using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using InstaPoker.Client.Game;
using InstaPoker.Core;
using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Client.Network;

public static class NetworkManager {

    private static TcpClient client;
    private const int Port = 12512;
    public static MessageHandler? Handler { get; private set; }
    
    public static async Task ConnectToServer(string username) {
        client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, Port);
        NetworkStream ns = client.GetStream();
        await using BinaryWriter bw = new (ns, Encoding.UTF8, true);
        using BinaryReader br = new(ns, Encoding.UTF8, true);
        
        bw.Write("InstaPoker.Client"); // send client id
        bw.Write(Assembly.GetExecutingAssembly().GetName().Version!);
        bw.Write(username);
        
        string serverId = br.ReadString(); 
        if (serverId != "InstaPoker.Server") {
            Console.WriteLine($"Could not recognize server id. Received \"{serverId}\". Expected: \"InstaPoker.Server\"");
            client.Close();
            return;
        }

        Version serverVersion = br.ReadVersion();
        Console.WriteLine($"Server is at version {serverVersion.ToString()}");

        Handler = new MessageHandler(ns, new MessageWriter(ns, true), new MessageReader(ns, true));
    }

    public static Task<(string, RoomSettings)> CreateRoom() {
        return Task.Run(async () => {
            CreateRoomResponse response = await Handler!.SendRequest<CreateRoomRequest, CreateRoomResponse>(new CreateRoomRequest());
            Console.WriteLine("Created room with code " + response.RoomCode);
            return (response.RoomCode,response.Settings);
        });
    }
    
    public static Task JoinRoom(string code) {
        return Task.Delay(Random.Shared.Next(50,500));
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