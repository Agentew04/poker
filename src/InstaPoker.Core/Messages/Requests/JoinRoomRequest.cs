using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Core.Messages.Requests;

/// <summary>
/// Request to join an existing room. Pairs with <see cref="JoinRoomResponse"/>.
/// </summary>
/// <remarks>Client to Server</remarks>
public class JoinRoomRequest : Message {

    /// <summary>
    /// The code of the room that the user wants to join.
    /// </summary>
    public string RoomCode { get; set; } = string.Empty;
    
    public override Guid UniqueId => new("1FF25BDF-E863-4708-9F43-87EC3948C4BE");
    
    public override void Write(BinaryWriter bw) {
        bw.Write(RoomCode);
    }

    public override void Read(BinaryReader br) {
        RoomCode = br.ReadString();
    }
}