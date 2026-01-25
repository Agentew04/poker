namespace InstaPoker.Core.Messages.Responses;

public class JoinRoomResponse : Message {
    
    public JoinRoomResult Result { get; set; }
    public List<string> ConnectedUsers { get; set; } = [];
    public RoomSettings Settings { get; set; } = new();
    public string OwnerName { get; set; } = string.Empty;
    
    public override Guid UniqueId => new("02E08670-47CE-4E20-97FC-8F10D101CFC5");
    
    public override void Write(BinaryWriter bw) {
        bw.Write((int)Result);
        bw.Write(ConnectedUsers.Count);
        foreach (string user in ConnectedUsers) {
            bw.Write(user);
        }
        Settings.Write(bw);
        bw.Write(OwnerName);
    }

    public override void Read(BinaryReader br) {
        Result = (JoinRoomResult)br.ReadInt32();
        int userCount = br.ReadInt32();
        ConnectedUsers.Clear();
        ConnectedUsers.EnsureCapacity(userCount);
        for (int i = 0; i < userCount; i++) {
            ConnectedUsers.Add(br.ReadString());
        }
        Settings.Read(br);
        OwnerName = br.ReadString();
    }
}

public enum JoinRoomResult {
    Success,
    RoomDoesNotExist,
    RoomFull,
    UsernameAlreadyExist,
    AlreadyInOtherRoom
}