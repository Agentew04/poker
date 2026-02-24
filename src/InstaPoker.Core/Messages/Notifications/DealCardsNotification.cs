using InstaPoker.Core.Games;

namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Sent privately to each player at the start of a hand with their 2 hole cards.
/// </summary>

public class DealCardsNotification : Message {
    public static readonly Guid Id = new("6A5A5158-7F52-49BA-A574-2C9A0CED2F10");
    public override Guid UniqueId => Id;

    public GameCard Card1 { get; set; }
    public GameCard Card2 { get; set; }

    public override void Write(BinaryWriter bw)
    {
        Card1.Write(bw);
        Card2.Write(bw);
    }

    public override void Read(BinaryReader br)
    {
        Card1 = new GameCard(); Card1.Read(br);
        Card2 = new GameCard(); Card2.Read(br);
    }
}

