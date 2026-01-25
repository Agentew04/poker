using System.Net;
using System.Net.Sockets;

namespace InstaPoker.Server;

public class Server
{
    private TcpListener _listener;

    public void Start()
    {
        _listener = new TcpListener(IPAddress.Any, 21915);
        _listener.Start();

        while (true)
        {
            var client = _listener.AcceptTcpClient();
            Task.Run(() => HandleClient(client));
        }
    }

    private void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        using var reader = new BinaryReader(stream);
        using var writer = new BinaryWriter(stream);

        try
        {
            while (client.Connected)
            {
                int packetId = reader.ReadInt32();
                var type = (PacketType)packetId;

                switch (type)
                {
                    // requests
                    case PacketType.CreateRoomRequest:
                        break;

                    case PacketType.JoinRoomRequest:
                        break;

                    // notifications
                    case PacketType.PlayerLeftNotification:
                        break;
                        
                    case PacketType.PlayerJoinedNotification:
                        break;
                }
            }
        }
        finally
        {
            client.Close();
        }
    }
}