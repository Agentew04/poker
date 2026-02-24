using InstaPoker.Core.Games;

namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Broadcast when table cards are revealed (Flop = 3 cards, Turn = 1, River = 1).
/// </summary>

public class TableCardsNotification : Message
{
    public static readonly Guid Id = new("8B26A1EA-C9F5-4F63-B0E7-955A8EE13062");
    public override Guid UniqueId => Id;
    
    public TableCardStage Stage { get; set; }
    public List<GameCard> Cards { get; set; } = [];

    public override void Write(BinaryWriter bw)
    {
        bw.Write((byte)Stage);
        bw.Write((byte)Cards.Count);
        foreach (GameCard card in Cards)
            card.Write(bw);
    }

    public override void Read(BinaryReader br)
    {
        Stage = (TableCardStage)br.ReadByte();
        int count = br.ReadByte();
        for (int i = 0; i < count; i++)
        {
            GameCard card = new();
            card.Read(br);
            Cards.Add(card);
        }
    }
}

public enum TableCardStage : byte
{
    Flop,
    Turn,
    River,
}