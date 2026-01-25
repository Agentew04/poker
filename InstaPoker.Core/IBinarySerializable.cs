namespace InstaPoker.Core;

public interface IBinarySerializable {
    
    /// <summary>
    /// Serializes this object as binary data.
    /// </summary>
    /// <param name="bw">The writer that receives data</param>
    public void Write(BinaryWriter bw);

    /// <summary>
    /// Deserialized this object from binary data.
    /// </summary>
    /// <param name="br">The reader that provides data</param>
    public void Read(BinaryReader br);

}