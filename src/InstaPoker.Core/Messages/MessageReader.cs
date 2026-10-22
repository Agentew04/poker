using System.Buffers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InstaPoker.Core.Messages;

/// <summary>
/// A class that reads <see cref="Message"/>s from a <see cref="Stream"/>.
/// </summary>
/// <remarks>
/// Accepts any stream (a <see cref="System.Net.Sockets.NetworkStream"/> or a
/// <see cref="System.Net.Security.SslStream"/>) so the same framing can be used on the client and the server.
/// </remarks>
public class MessageReader : IDisposable {

    private readonly bool leaveOpen;
    private readonly Stream ns;
    private readonly ILogger<MessageReader> logger;

    public MessageReader(Stream ns, bool leaveOpen, ILogger<MessageReader>? logger = null) {
        this.leaveOpen = leaveOpen;
        this.ns = ns;
        this.logger = logger ?? NullLogger<MessageReader>.Instance;
    }

    public void Dispose() {
        if (!leaveOpen) {
            ns.Dispose();
        }
    }

    /// <summary>
    /// Waits for the next message asyncronously and parses it.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<Message> ReadNextMessageAsync(CancellationToken cancellationToken) {
        byte[] guidBytes = ArrayPool<byte>.Shared.Rent(16);
        byte[] sizeBytes = ArrayPool<byte>.Shared.Rent(4);
        byte[]? payloadBuffer = null;
        try {
            await ns.ReadExactlyAsync(guidBytes, 0, 16, cancellationToken);
            Guid messageId = new(guidBytes.AsSpan(0, 16));

            await ns.ReadExactlyAsync(sizeBytes, 0, 4, cancellationToken);
            int payloadSize = BitConverter.ToInt32(sizeBytes);
            if (payloadSize is < 0 or > 10_000_000) {
                throw new InvalidOperationException("Invalid payload size");
            }

            payloadBuffer = ArrayPool<byte>.Shared.Rent(payloadSize);
            await ns.ReadExactlyAsync(payloadBuffer, 0, payloadSize, cancellationToken);

            using MemoryStream ms = new(payloadBuffer, 0, payloadSize);
            using BinaryReader payloadReader = new(ms);

            Type? messageType = MessageTypeCache.GetMessageType(messageId);
            if (messageType is null) {
                logger.LogError("Message ID not recognized, throwing Exception. Exception may be lost in async environments.");
                throw new Exception("Message ID not recognized");
            }

            Message message = (Message)Activator.CreateInstance(messageType)!;
            message.Read(payloadReader);
            return message;
        }
        finally {
            ArrayPool<byte>.Shared.Return(guidBytes);
            ArrayPool<byte>.Shared.Return(sizeBytes);
            if (payloadBuffer is not null) {
                ArrayPool<byte>.Shared.Return(payloadBuffer);
            }
        }
    }
}
