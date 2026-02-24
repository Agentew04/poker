namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Broadcast at the start of a hand. Tells all players who the dealer is,
/// who posted the blinds, and the starting balances.
/// </summary>

public class RoundStartNotification : Message
{
    public static readonly Guid Id = new("4859ADA3-F4B4-4E1D-962B-72C3046D5ECC");
    public override Guid UniqueId => Id;
    
    public string DealerUsername { get; set; } = string.Empty;
    public string SmallBlindUsername  { get; set; } = string.Empty;
    public string BigBlindUsername  { get; set; } = string.Empty;
    public int SmallBlindAmount  { get; set; }
    public int BigBlindAmount   { get; set; }
    
    public Dictionary<string, int> Balances  { get; set; } = [];

    public override void Write(BinaryWriter bw)
    {
        bw.Write(DealerUsername);
        bw.Write(SmallBlindUsername);
        bw.Write(BigBlindUsername);
        bw.Write(SmallBlindAmount);
        bw.Write(BigBlindAmount);
        bw.Write(Balances.Count);
        foreach ((string user, int balance) in Balances)
        {
            bw.Write(user);
            bw.Write(balance);
        }
    }

    public override void Read(BinaryReader br)
    {
        DealerUsername = br.ReadString();
        SmallBlindUsername = br.ReadString();
        BigBlindUsername = br.ReadString();
        SmallBlindAmount = br.ReadInt32();
        BigBlindAmount = br.ReadInt32();
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
            Balances[br.ReadString()] = br.ReadInt32();
        
    }
}