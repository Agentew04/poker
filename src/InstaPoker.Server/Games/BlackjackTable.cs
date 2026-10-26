using InstaPoker.Core.Games;

namespace InstaPoker.Server.Games;

public class BlackjackTable : GameTable {

    private readonly Room room;
    private List<GameCard> deck = [];
    private List<ClientConnection> players = [];
    
    public BlackjackTable(Room room) {
        this.room = room;
        deck.AddRange(CardMethods.CreateDeck());
        players.AddRange(room.ConnectedUsers);
    }
    
    public override Task StartGame() {
        return Task.CompletedTask;
    }
}