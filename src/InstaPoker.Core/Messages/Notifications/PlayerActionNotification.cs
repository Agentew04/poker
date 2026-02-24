using InstaPoker.Core.Games;

namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Broadcast to all players after a player acts.
/// Used to update the UI: pot, balances, who folded, etc.
/// </summary>

public class PlayerActionNotification : Message
{
    public static readonly Guid Id = new("CC0B1A70-7302-48C0-B97B-3C05FE04A7C1");
    public override Guid UniqueId => Id;
    
    public string Username { get; set; } = string.Empty;
    public PokerAction Action { get; set; }
    
    /// <summary>Amount added to the pot by this action (0 for Fold/Check).</summary>
    public int Amount  { get; set; }
    
    /// <summary>Total pot after this action.</summary>
    public int TotalPot { get; set; }
    
    /// <summary>Remaining balance of the player who acted.</summary>
    public int RemainingBalance { get; set; }

    public override void Write(BinaryWriter bw)
    {
        bw.Write(Username);
        bw.Write((byte)Action);
        bw.Write(Amount);
        bw.Write(TotalPot);
        bw.Write(RemainingBalance);
    }

    public override void Read(BinaryReader br)
    {
        Username = br.ReadString();
        Action = (PokerAction)br.ReadByte();
        Amount = br.ReadInt32();
        TotalPot  = br.ReadInt32();
        RemainingBalance = br.ReadInt32();
    }
}

