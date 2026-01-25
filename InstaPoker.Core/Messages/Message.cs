namespace InstaPoker.Core.Messages;

public abstract class Message : IBinarySerializable {
    public abstract Guid UniqueId { get; }

    public abstract void Write(BinaryWriter bw);

    public abstract void Read(BinaryReader br);
    
}