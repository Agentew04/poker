using System.Collections.Concurrent;
using System.Threading.Channels;
using InstaPoker.Core.Messages;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// Owns the connection to the server. Reading and writing happen on background tasks; the main thread
/// only ever touches <see cref="Pump"/> and the events raised by it.
/// </summary>
/// <remarks>
/// A <see cref="System.Net.Security.SslStream"/> allows exactly one concurrent read and one concurrent write,
/// which is why there is a single read loop and a single write loop. Outgoing messages go through a
/// <see cref="Channel{T}"/> so they are serialized and never interleave on the wire.
/// </remarks>
public sealed class MessageHandler : IDisposable {

    private readonly MessageReader reader;
    private readonly MessageWriter writer;
    private readonly MainThreadDispatcher dispatcher;
    private readonly ILogger<MessageHandler> logger;

    private readonly Channel<Message> outbound = Channel.CreateUnbounded<Message>(
        new UnboundedChannelOptions { SingleReader = true });
    private readonly ConcurrentQueue<Message> inbox = new();
    private readonly ConcurrentDictionary<Type, TaskCompletionSource<Message>> pendingRequests = new();
    private readonly CancellationTokenSource cts = new();

    private int disconnectedPosted;
    private int disposed;

    public MessageHandler(MessageReader reader, MessageWriter writer, MainThreadDispatcher dispatcher,
        ILogger<MessageHandler> logger) {
        this.reader = reader;
        this.writer = writer;
        this.dispatcher = dispatcher;
        this.logger = logger;

        _ = Task.Run(ReadLoopAsync);
        _ = Task.Run(WriteLoopAsync);
    }

    /// <summary>
    /// Raised from <see cref="Pump"/> on the main thread for every message that was not an answer to a request.
    /// </summary>
    public event Action<Message>? MessageReceived;

    /// <summary>
    /// Raised from <see cref="Pump"/> on the main thread when the connection is lost or closed.
    /// </summary>
    public event Action<Exception?>? Disconnected;

    /// <summary>
    /// Response types the application is currently waiting for. Safe to read from the main thread.
    /// </summary>
    public IReadOnlyCollection<Type> PendingRequests => pendingRequests.Keys.ToArray();

    /// <summary>
    /// Sends a request and waits for its response. The continuation runs on the main thread because the
    /// client installs a <see cref="MainThreadDispatcher"/> as its synchronization context.
    /// </summary>
    public async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request)
        where TRequest : Message where TResponse : Message {
        TaskCompletionSource<Message> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!pendingRequests.TryAdd(typeof(TResponse), tcs)) {
            throw new InvalidOperationException(
                $"A request expecting {typeof(TResponse).Name} is already in flight.");
        }

        if (!outbound.Writer.TryWrite(request)) {
            pendingRequests.TryRemove(typeof(TResponse), out _);
            throw new InvalidOperationException("The connection to the server is closed.");
        }

        Message response = await tcs.Task;
        return (TResponse)response;
    }

    /// <summary>
    /// Queues a message that does not expect a response.
    /// </summary>
    public Task SendNotification<TNotification>(TNotification notification) where TNotification : Message {
        if (!outbound.Writer.TryWrite(notification)) {
            throw new InvalidOperationException("The connection to the server is closed.");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispatches everything received since the last frame. Must only be called from the main thread.
    /// </summary>
    public void Pump() {
        while (inbox.TryDequeue(out Message? message)) {
            try {
                MessageReceived?.Invoke(message);
            }
            catch (Exception e) {
                logger.LogError(e, "Unhandled exception while dispatching {type}", message.GetType().Name);
            }
        }
    }

    private async Task ReadLoopAsync() {
        CancellationToken token = cts.Token;
        try {
            while (!token.IsCancellationRequested) {
                Message message = await reader.ReadNextMessageAsync(token);
                if (pendingRequests.TryRemove(message.GetType(), out TaskCompletionSource<Message>? tcs)) {
                    tcs.TrySetResult(message);
                    continue;
                }

                inbox.Enqueue(message);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) {
            // expected when the connection is closed by the application
        }
        catch (Exception) when (Volatile.Read(ref disposed) == 1) {
            // the stream was disposed as part of an intentional shutdown
        }
        catch (Exception e) {
            logger.LogError(e, "Connection lost while reading from the server");
            PostDisconnected(e);
        }
    }

    private async Task WriteLoopAsync() {
        CancellationToken token = cts.Token;
        try {
            await foreach (Message message in outbound.Reader.ReadAllAsync(token)) {
                await writer.WriteAsync(message, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) {
            // expected when the connection is closed by the application
        }
        catch (Exception e) {
            logger.LogError(e, "Connection lost while sending to the server");
            PostDisconnected(e);
        }
    }

    private void PostDisconnected(Exception? error) {
        if (Interlocked.Exchange(ref disconnectedPosted, 1) != 0) {
            return;
        }

        // stop the write loop and make further sends fail instead of queueing forever
        cts.Cancel();
        outbound.Writer.TryComplete();

        IOException reason = new("Disconnected from the server.", error);
        foreach (TaskCompletionSource<Message> pending in pendingRequests.Values) {
            pending.TrySetException(reason);
        }
        pendingRequests.Clear();

        dispatcher.Post(_ => Disconnected?.Invoke(error), null);
    }

    public void Dispose() {
        if (Interlocked.Exchange(ref disposed, 1) != 0) {
            return;
        }

        cts.Cancel();
        outbound.Writer.TryComplete();
        reader.Dispose();
        writer.Dispose();
    }
}
