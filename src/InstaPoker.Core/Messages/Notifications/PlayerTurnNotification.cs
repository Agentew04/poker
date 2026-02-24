using InstaPoker.Core.Games;

namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Sent to ALL players to indicate whose turn it is.
/// The player whose turn it is uses this to know they must act.
/// Others use it to update the UI.
/// </summary>

public class PlayerTurnNotification : Message {
    
    public static readonly Guid Id = new("591454CB-1D0B-44BF-8E6C-56241D48168A");
    public override Guid UniqueId => Id;
    
    public string Username { get; set; } = string.Empty;

    /// <summary>Which actions are valid this turn.</summary>
    public List<PokerAction> ValidActions { get; set; } = [];
    
    /// <summary>Minimum raise amount.</summary>
    public int MinRaise { get; set; }
    
    /// <summary>Amount needed to call.</summary>
    public int CallAmount { get; set; }
    
    /// <summary>Remaining balance of the acting player.</summary>
    public int PlayerBalance { get; set; }

    public override void Write(BinaryWriter bw)
    {
        bw.Write(Username);
        bw.Write((byte)ValidActions.Count);
        foreach (PokerAction action in ValidActions)
            bw.Write((byte)action);
        bw.Write(MinRaise);
        bw.Write(CallAmount);
        bw.Write(PlayerBalance);
    }

    public override void Read(BinaryReader br)
    {
        Username = br.ReadString();
        int count  = br.ReadByte();
        for (int i =  0; i < count; i++)
            ValidActions.Add((PokerAction)br.ReadByte());
        MinRaise = br.ReadInt32();
        CallAmount = br.ReadInt32();
        PlayerBalance = br.ReadInt32();
    }
}