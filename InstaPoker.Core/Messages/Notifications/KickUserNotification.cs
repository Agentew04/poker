namespace InstaPoker.Core.Messages.Notifications;

public class KickUserNotification : Message {

    public string Username { get; set; } = string.Empty;
    
    public override Guid UniqueId => new("2E86F234-ABAE-4FB8-8AC3-9A373F73F258");
    
    public override void Write(BinaryWriter bw) {
        bw.Write(Username);
    }

    public override void Read(BinaryReader br) {
        Username = br.ReadString();
    }
}