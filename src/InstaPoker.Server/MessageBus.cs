using System.Threading.Channels;
using InstaPoker.Core.Messages;

namespace InstaPoker.Server;

/// <summary>
/// Single queue of incoming messages from every connection. ClientConnection writes to it and the Router
/// awaits on it, so the router does not need to poll connections or allocate snapshots.
/// </summary>
public sealed class MessageBus {

    private readonly Channel<(ClientConnection Connection, Message Message)> channel =
        Channel.CreateUnbounded<(ClientConnection, Message)>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelReader<(ClientConnection Connection, Message Message)> Reader => channel.Reader;

    public ValueTask PublishAsync(ClientConnection connection, Message message, CancellationToken cancellationToken) {
        return channel.Writer.WriteAsync((connection, message), cancellationToken);
    }

    public void Complete() {
        channel.Writer.TryComplete();
    }
}
