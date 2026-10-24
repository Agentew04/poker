using System.Net.Sockets;
using System.Threading.Channels;
using InstaPoker.Core.Messages;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Server;

public sealed class ClientConnection {

    public required TcpClient Client { get; set; }

    public required string Username { get; set; }

    public List<(Type, TaskCompletionSource<Message>)> PendingRequests { get; } = [];

    public required Stream Stream { get; set; }

    public required MessageWriter MessageWriter { get; set; }

    public required UserManager UserManager { get; set; }

    public required MessageBus MessageBus { get; set; }

    public required ILogger<ClientConnection> Logger { get; set; }

    public required CancellationToken ShutdownToken { get; set; }

    private readonly CancellationTokenSource cts = new();
    private volatile bool closed;

    public bool IsClosed => closed || cts.IsCancellationRequested;

    public async Task StartReceivingAsync() {
        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ShutdownToken);
        using MessageReader mr = new(Stream, false);

        try {
            while (!linked.IsCancellationRequested) {
                // wait for new packet
                Message m;
                try {
                    m = await mr.ReadNextMessageAsync(linked.Token);
                }
                catch (OperationCanceledException) when (linked.IsCancellationRequested) {
                    break;
                }
                catch (IOException) {
                    Logger.LogInformation("Lost connection of user {username}", Username);
                    break;
                }
                catch (Exception e) {
                    Logger.LogWarning(e, "Unknown exception in {username} listen loop, skipping", Username);
                    continue;
                }

                Logger.LogTrace("Received {type} from {username}", m.GetType().Name, Username);

                try {
                    await MessageBus.PublishAsync(this, m, linked.Token);
                }
                catch (OperationCanceledException) when (linked.IsCancellationRequested) {
                    break;
                }
                catch (ChannelClosedException) {
                    // the server is shutting down and the router stopped consuming
                    break;
                }
            }
        }
        finally {
            // leaving the loop always means the connection is gone: closed by the client, by an IO error,
            // or because the server is shutting down
            Logger.LogInformation("User {username} disconnected", Username);
            closed = true;
            FailPendingRequests();
            Client.Close();
            // let user manager tell other pieces of the application that we have been disconnected,
            // unless the whole server is going down and every connection is closing
            UserManager.RemoveConnection(this, notifyRooms: !ShutdownToken.IsCancellationRequested);
        }
    }

    public async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request)
        where TRequest : Message where TResponse : Message {
        TaskCompletionSource<Message> tcs = new();
        PendingRequests.Add((typeof(TResponse), tcs));
        await MessageWriter.WriteAsync(request);
        Message response = await tcs.Task;
        return (TResponse)response;
    }

    /// <summary>
    /// Stops the receive loop and closes the socket. Pending requests are failed so callers do not hang.
    /// </summary>
    public void Disconnect() {
        cts.Cancel();
        Client.Close();
        FailPendingRequests();
    }

    public Task StopReceiving() {
        return cts.CancelAsync();
    }

    private void FailPendingRequests() {
        if (PendingRequests.Count == 0) {
            return;
        }

        IOException reason = new("Connection closed.");
        foreach ((Type _, TaskCompletionSource<Message> tcs) in PendingRequests.ToArray()) {
            tcs.TrySetException(reason);
        }
        PendingRequests.Clear();
    }
}
