using InstaPoker.Core;
using InstaPoker.Server.Games;

namespace InstaPoker.Server;

public class Room {

    public string Code { get; set; }
    public List<ClientConnection> ConnectedUsers { get; set; } = [];
    public ClientConnection Owner { get; set; }
    public RoomSettings Settings { get; set; } = new();
    
    public string Game { get; set; } // unused for now. Will say if its poker, blackjack, truco etc.
    
    public GameTable Table { get; set; }
}