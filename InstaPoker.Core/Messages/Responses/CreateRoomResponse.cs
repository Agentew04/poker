namespace InstaPoker.Core.Messages.Responses;

public class CreateRoomResponse : Message {

    public string RoomCode { get; set; } = string.Empty;

    public RoomSettings Settings { get; set; } = new();
    
    public override Guid UniqueId => new("B8431911-CF5F-43E1-846D-78B5053E4D48");
    public override void Write(BinaryWriter bw) {
        bw.Write(RoomCode);
        Settings.Write(bw);
    }

    public override void Read(BinaryReader br) {
        RoomCode = br.ReadString();
        Settings.Read(br);
    }
}