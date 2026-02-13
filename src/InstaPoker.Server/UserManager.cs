namespace InstaPoker.Server;

public static class UserManager {

    public static List<ClientConnection> Connections { get; set; } = [];
    
    public static void AddNewConnection(ClientConnection conn) {
        lock (Connections) {
            Connections.Add(conn);
        }
    }

    public static void RemoveConnection(ClientConnection conn) {
        Connections.Remove(conn);
        RoomManager.UnexpectedDisconnect(conn);
    }
}