using System.Buffers;
using System.Text;

namespace InstaPoker.Core.Messages;

/// <summary>
/// Serializes <see cref="Message"/>s and writes them to a <see cref="Stream"/>.
/// </summary>
/// <remarks>
/// Accepts any stream (a <see cref="System.Net.Sockets.NetworkStream"/> or a
/// <see cref="System.Net.Security.SslStream"/>) so the same framing can be used on the client and the server.
/// Writing is fully async because a <see cref="System.Net.Security.SslStream"/> does not support
/// synchronous writes on all platforms.
/// </remarks>
public sealed class MessageWriter : IDisposable, IAsyncDisposable {

    private readonly bool leaveOpen;
    private readonly Stream ns;

    public MessageWriter(Stream ns, bool leaveOpen) {
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

    public async Task WriteAsync(Message item, CancellationToken cancellationToken = default) {
        using MemoryStream ms = new();
        using (BinaryWriter bw = new(ms, Encoding.UTF8, true)) {
            item.Write(bw);
            bw.Flush();
        }

        int payloadSize = (int)ms.Length;
        int frameSize = 16 + 4 + payloadSize;
        byte[] frame = ArrayPool<byte>.Shared.Rent(frameSize);
        try {
            item.UniqueId.TryWriteBytes(frame.AsSpan(0, 16));
            BitConverter.TryWriteBytes(frame.AsSpan(16, 4), payloadSize);
            ms.GetBuffer().AsSpan(0, payloadSize).CopyTo(frame.AsSpan(20));

            // a single write per message keeps frames from interleaving on the wire
            await ns.WriteAsync(frame.AsMemory(0, frameSize), cancellationToken);
            await ns.FlushAsync(cancellationToken);
        }
        finally {
            ArrayPool<byte>.Shared.Return(frame);
        }
    }
}
