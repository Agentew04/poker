namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Notificacao avisando que o usuario vai sair da sala.
/// </summary>
/// <remarks>Client to Server</remarks>
public class LeaveRoomNotification : Message {
    public override Guid UniqueId => new("9191A282-0F42-4E52-90F5-25E219DCD81F");
    public override void Write(BinaryWriter bw) {
        // empty
    }

    public override void Read(BinaryReader br) {
        // empty
    }
}