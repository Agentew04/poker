namespace InstaPoker.Core;

public interface IBinarySerializable {
    public void Write(BinaryWriter bw);

    public void Read(BinaryReader br);

}