using System.Runtime.InteropServices;

namespace InstaPoker.Client.Graphics;

public sealed class PinnedBytes : IDisposable {
    private GCHandle handle;
    public byte[] Data { get; }

    public PinnedBytes(byte[] data) {
        Data = data;
        handle = GCHandle.Alloc(data, GCHandleType.Pinned);
    }

    public IntPtr Pointer => handle.AddrOfPinnedObject();
    public int Length => Data.Length;
    public void Dispose() {
        if (handle.IsAllocated) handle.Free();
    }
}