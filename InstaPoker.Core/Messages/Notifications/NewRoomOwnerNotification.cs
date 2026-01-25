using System.Buffers.Binary;

namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Notificacao avisando que existe um novo dono da sala
/// </summary>
/// <remarks>Server to Client</remarks>
public class NewRoomOwnerNotification : Message {
    public string Owner { get; set; } = string.Empty;

    public override Guid UniqueId => new("9ACF2F3B-4AA8-4D09-993B-84E3A2B0331B");
    
    public override void Write(BinaryWriter bw) {
        bw.Write(Owner);
    }

    public override void Read(BinaryReader br) {
        Owner = br.ReadString();
    }
}