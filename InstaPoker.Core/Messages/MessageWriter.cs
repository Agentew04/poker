using System.Net.Sockets;
using System.Threading.Channels;

namespace InstaPoker.Core.Messages;

public sealed class MessageWriter : IDisposable, IAsyncDisposable {

    private readonly bool leaveOpen;
    private readonly NetworkStream ns;
    
    public MessageWriter(NetworkStream ns, bool leaveOpen) {
        this.ns = ns;
        this.leaveOpen = leaveOpen;
    }

    public void Dispose() {
        if (!leaveOpen) {
            ns.Dispose();
        }
    }

    public ValueTask DisposeAsync() {
        return !leaveOpen ? ns.DisposeAsync() : ValueTask.CompletedTask;
    }

    public async Task WriteAsync(Message item) {
        Guid guid = item.UniqueId;
        using MemoryStream ms = new();
        await using BinaryWriter bw = new(ms);
        item.Write(bw);
        bw.Flush();

        Span<byte> guidBytes = stackalloc byte[16];
        guid.TryWriteBytes(guidBytes);
        ns.Write(guidBytes);
        int payloadSize = (int)ms.Length;
        Span<byte> sizeBytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(sizeBytes, payloadSize);
        ns.Write(sizeBytes);
        ns.Write(ms.ToArray());
        ns.Flush();
    }
}