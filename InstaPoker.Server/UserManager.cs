using System.Net.Sockets;

namespace InstaPoker.Server;

public class UserManager {

    public List<ClientConnection> Connections { get; init; } = [];
    
    public void AddNewConnection(ClientConnection conn) {
        lock (Connections) {
            Connections.Add(conn);
        }
    }
}