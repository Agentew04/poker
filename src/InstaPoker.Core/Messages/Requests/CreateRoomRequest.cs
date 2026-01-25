using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Core.Messages.Requests;

/// <summary>
/// Request to create a new room. Pairs with <see cref="CreateRoomResponse"/>.
/// </summary>
/// <remarks>Client to Server</remarks>
public class CreateRoomRequest : Message {
    public override Guid UniqueId => new("22FF01FE-3989-474C-95F9-30192DC547C4");
    public override void Write(BinaryWriter bw) {
        // empty
    }

    public override void Read(BinaryReader br) {
        // empty
    }
}