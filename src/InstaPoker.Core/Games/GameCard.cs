namespace InstaPoker.Core.Games;

/// <summary>
/// Represents a card of a traditional poker playing card set.
/// </summary>
public struct GameCard : IBinarySerializable {
    
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

    public void Write(BinaryWriter bw) {
        byte serialized = (byte)((((byte)Value << 2) & 0b111100) | ((byte)Suit & 0b11));
        bw.Write(serialized);
    }

    public void Read(BinaryReader br) {
        byte serialized = br.ReadByte();
        Value = (serialized >> 2) & 0b1111;
        Suit = (Suit)(serialized & 0b11);
    }
}