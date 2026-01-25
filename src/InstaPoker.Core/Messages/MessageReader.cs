using System.Buffers;
using System.Net.Sockets;

namespace InstaPoker.Core.Messages;

/// <summary>
/// A class that reads <see cref="Message"/>s from a <see cref="NetworkStream"/>.
/// </summary>
public class MessageReader : IDisposable {

    private readonly bool leaveOpen;
    private readonly NetworkStream ns;
    
    public MessageReader(NetworkStream ns, bool leaveOpen) {
        this.leaveOpen = leaveOpen;
        this.ns = ns;
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
    public async Task<Message> ReadNextMessageAsync() {
        byte[] guidBytes = ArrayPool<byte>.Shared.Rent(16);
        await ns.ReadExactlyAsync(guidBytes, 0, 16);
        Guid messageId = new(guidBytes);
        ArrayPool<byte>.Shared.Return(guidBytes);

        byte[] sizeBytes = ArrayPool<byte>.Shared.Rent(4);
        await ns.ReadExactlyAsync(sizeBytes, 0, 4);
        int payloadSize = BitConverter.ToInt32(sizeBytes);
        ArrayPool<byte>.Shared.Return(sizeBytes);
        if (payloadSize is < 0 or > 10_000_000) {
            throw new InvalidOperationException("Invalid payload size");
        }

        byte[] payloadBuffer = ArrayPool<byte>.Shared.Rent(payloadSize);
        await ns.ReadExactlyAsync(payloadBuffer, 0, payloadSize);

        using MemoryStream ms = new(payloadBuffer);
        using BinaryReader payloadReader = new(ms);

        Type? messageType = MessageTypeCache.GetMessageType(messageId);
        if (messageType is null) {
            Console.WriteLine("Message ID not recognized, throwing Exception. Exception may be lost in async environments.");
            throw new Exception("Message ID not recognized");
        }
        Message message = (Message)Activator.CreateInstance(messageType)!;
        message.Read(payloadReader);
        
        ArrayPool<byte>.Shared.Return(payloadBuffer);
        return message;
    }
}