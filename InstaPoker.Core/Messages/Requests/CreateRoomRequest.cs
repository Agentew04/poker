
namespace InstaPoker.Core.Messages.Requests;

public class CreateRoomRequest : Message {
    public override Guid UniqueId => new("22FF01FE-3989-474C-95F9-30192DC547C4");
    public override void Write(BinaryWriter bw) {
        // empty
    }

    public override void Read(BinaryReader br) {
        // empty
    }
}