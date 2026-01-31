namespace InstaPoker.Core.Messages;

/// <summary>
/// Base class that all messages exchanged between the client and the server must inherit.
/// </summary>
public abstract class Message : IBinarySerializable {
    
    /// <summary>
    /// The unique id for this type of message.
    /// </summary>
    public abstract Guid UniqueId { get; }

    /// <inheritdoc cref="IBinarySerializable.Write"/>
    public abstract void Write(BinaryWriter bw);

    /// <inheritdoc cref="IBinarySerializable.Read"/>
    public abstract void Read(BinaryReader br);
    
}