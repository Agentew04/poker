namespace InstaPoker.Core;

/// <summary>
/// Class that holds game settings that belongs to a specific room.
/// </summary>
public class RoomSettings : IBinarySerializable {
    
    /// <summary>
    /// The maximum amount that a bet is allowed to have.
    /// </summary>
    public int MaxBet { get; set; }
    
    /// <summary>
    /// The maximum amount of players that can be in the room.
    /// </summary>
    public int MaxPlayers { get; set; }
    
    /// <summary>
    /// The amount that the first player is required to bet.
    /// </summary>
    public int SmallBlind { get; set; }
    
    /// <summary>
    /// If players can bet their entire balance in one go.
    /// </summary>
    public bool IsAllInEnabled { get; set; }

    public void Write(BinaryWriter bw) {
        bw.Write(MaxPlayers);
        bw.Write(MaxBet);
        bw.Write(SmallBlind);
        bw.Write(IsAllInEnabled);
    }

    public void Read(BinaryReader br) {
        MaxPlayers = br.ReadInt32();
        MaxBet = br.ReadInt32();
        SmallBlind = br.ReadInt32();
        IsAllInEnabled = br.ReadBoolean();
    }
}