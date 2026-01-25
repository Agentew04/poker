namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Notificacao quando as configuracoes de uma sala sao mudadas.
/// </summary>
/// <remarks>Client[Owner] to Server || Server to Client</remarks>
public class RoomSettingsChangeNotification : Message {

    public RoomSettings NewSettings { get; set; } = new();
    
    public override Guid UniqueId => new("55E044F9-7AC4-425F-8A02-11B2BFE54169");
    
    public override void Write(BinaryWriter bw) {
        NewSettings.Write(bw);
    }

    public override void Read(BinaryReader br) {
        NewSettings.Read(br);
    }
}