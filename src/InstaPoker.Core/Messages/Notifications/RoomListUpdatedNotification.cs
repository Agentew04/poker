namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Notificacao enviada pelo servidor para clientes avisando que ocorreu alguma
/// mudanca na lista de usuarios
/// </summary>
/// <remarks>Server to Client</remarks>
public class RoomListUpdatedNotification : Message {
    
    /// <summary>
    /// The name of the user that has been added/removed.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of update that has occurred.
    /// </summary>
    public LobbyListUpdateType UpdateType { get; set; }
    
    public override Guid UniqueId => new("32A9C853-EE23-4C0A-83C4-030C3F452F1F");
    
    public override void Write(BinaryWriter bw) {
        bw.Write(Username);
        bw.Write((int)UpdateType);
    }

    public override void Read(BinaryReader br) {
        Username = br.ReadString();
        UpdateType = (LobbyListUpdateType)br.ReadInt32();
    }
}

public enum LobbyListUpdateType {
    UserLeft,
    UserKicked,
    UserJoined,
}