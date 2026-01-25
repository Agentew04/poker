namespace InstaPoker.Core.Messages.Requests;

public class JoinRoomRequest : Message {

    public string RoomCode { get; set; } = string.Empty;
    
    public override Guid UniqueId => new("1FF25BDF-E863-4708-9F43-87EC3948C4BE");
    
    public override void Write(BinaryWriter bw) {
        bw.Write(RoomCode);
    }

    public override void Read(BinaryReader br) {
        RoomCode = br.ReadString();
    }
}