namespace InstaPoker.Core;

public class RoomSettings : IBinarySerializable {
    public int MaxBet { get; set; }
    public int MaxPlayers { get; set; }
    public int SmallBlind { get; set; }
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