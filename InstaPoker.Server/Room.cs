using InstaPoker.Core;

namespace InstaPoker.Server;

public class Room {

    public string Code { get; set; }
    public List<ClientConnection> ConnectedUsers { get; set; } = [];
    public ClientConnection Owner { get; set; }

    public RoomSettings Settings { get; set; } = new();
}