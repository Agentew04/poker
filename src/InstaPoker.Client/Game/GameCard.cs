namespace InstaPoker.Client.Game;

/// <summary>
/// Represents a card of a traditional poker playing card set.
/// </summary>
public struct GameCard {
    
    /// <summary>
    /// The value of the card.
    /// </summary>
    /// <remarks>
    /// Special cards have the following numeric values:
    /// <ul>
    ///     <li><b>Joker:</b> 0</li>
    ///     <li><b>Ace:</b> 1</li>
    ///     <li><b>Jack:</b> 11</li>
    ///     <li><b>Queen:</b> 12</li>
    ///     <li><b>King:</b> 13</li>
    /// </ul>
    /// </remarks>
    public int Value { get; set; }
    
    /// <summary>
    /// The suit of the card.
    /// </summary>
    public Suit Suit { get; set; }

    /// <summary>
    /// Creates a new card with the provided values.
    /// </summary>
    /// <param name="value">The numbered value of the card</param>
    /// <param name="suit">The suit of the card</param>
    public GameCard(int value, Suit suit) {
        Value = value;
        Suit = suit;
    }
}