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

    public static void Shuffle(this Random rng, List<GameCard> deck) {
        int length = deck.Count;
        for (int index1 = 0; index1 < length - 1; ++index1)
        {
            int index2 = rng.Next(index1, length);
            if (index2 != index1)
            {
                (deck[index1], deck[index2]) = (deck[index2], deck[index1]);
            }
        }
    }
}