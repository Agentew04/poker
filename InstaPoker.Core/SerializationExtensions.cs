namespace InstaPoker.Core;

public static class SerializationExtensions {
    
    
    public static void Write(this BinaryWriter bw, Version v) {
        bw.Write7BitEncodedInt(v.Major);
        bw.Write7BitEncodedInt(v.Minor);
        bw.Write7BitEncodedInt(v.Build);
        bw.Write7BitEncodedInt(v.Revision);
    }

    public static Version ReadVersion(this BinaryReader br) {
        int major = br.Read7BitEncodedInt();
        int minor = br.Read7BitEncodedInt();
        int build = br.Read7BitEncodedInt();
        int rev = br.Read7BitEncodedInt();
        return new Version(major, minor, build, rev);
    }

    public static void Write(this BinaryWriter bw, Guid guid) {
        Span<byte> buffer = stackalloc byte[16];
        guid.TryWriteBytes(buffer);
        bw.Write(buffer);
    }

    public static Guid ReadGuid(this BinaryReader br) {
        Span<byte> buffer = stackalloc byte[16];
        br.ReadExactly(buffer);
        return new Guid(buffer);
    }
}