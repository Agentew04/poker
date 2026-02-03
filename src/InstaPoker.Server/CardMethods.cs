using InstaPoker.Core.Games;

namespace InstaPoker.Server;

public static class CardMethods {

    private static readonly Suit[] suits = [
        Suit.Diamonds,
        Suit.Clubs,
        Suit.Hearts,
        Suit.Spades
    ]; 
    
    public static IEnumerable<GameCard> CreateDeck() {
        foreach (Suit suit in suits) {
            for (int i = 1; i <= 13; i++) {
                yield return new GameCard(i, suit);
            }
        }
    }
}