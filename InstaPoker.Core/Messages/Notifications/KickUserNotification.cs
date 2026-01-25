namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Notificacao que avisa ao servidor que o dono expulsou um jogador do lobby. 
/// </summary>
/// <remarks>Client to Server</remarks>
public class KickUserNotification : Message {

    /// <summary>
    /// The name of the user that has been kicked by the owner.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    public override Guid UniqueId => new("2E86F234-ABAE-4FB8-8AC3-9A373F73F258");
    
    public override void Write(BinaryWriter bw) {
        bw.Write(Username);
    }

    public override void Read(BinaryReader br) {
        Username = br.ReadString();
    }
}