using InstaPoker.Core.Games;

namespace InstaPoker.Server.Games;

public class PokerTable : GameTable {

    private GameCard[] deck;
    private Random rng = new();
    
    public PokerTable(Room room) {
        deck = CardMethods.CreateDeck().ToArray();
        rng.Shuffle(deck);
    }

    public override async Task StartGame() {
        
    }
}