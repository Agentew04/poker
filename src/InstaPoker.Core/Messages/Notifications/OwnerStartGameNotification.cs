namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Message sent from the owner to tell the server that he wants to
/// start a new game. 
/// </summary>
/// <remarks>Client to Server</remarks>
public class OwnerStartGameNotification : Message {
    
    public override Guid UniqueId => new("5FC07C99-CC8A-44B0-8E5A-C25898C82EE6");
    
    public override void Write(BinaryWriter bw) {
        // empty
    }

    public override void Read(BinaryReader br) {
        // empty
    }
}