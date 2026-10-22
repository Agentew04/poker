using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// A <see cref="SynchronizationContext"/> that stores posted work in a queue instead of executing it.
/// The queue is drained by <see cref="Pump"/>, which must be called from the main (window) thread once per frame.
/// </summary>
/// <remarks>
/// The application main loop is a blocking Allegro loop with no message pump, so the default
/// <see cref="SynchronizationContext"/> cannot be used. Installing this context means every
/// <c>await</c> continuation started on the main thread is queued and runs on the main thread on the
/// next <see cref="Pump"/>, instead of running on a thread pool thread and touching UI state from there.
/// </remarks>
public sealed class MainThreadDispatcher : SynchronizationContext {

    private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> queue = new();
    private readonly ILogger<MainThreadDispatcher> logger;

    public MainThreadDispatcher(ILogger<MainThreadDispatcher> logger) {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public override void Post(SendOrPostCallback callback, object? state) {
        queue.Enqueue((callback, state));
    }

    /// <summary>
    /// Runs every queued continuation. Must only be called from the main thread.
    /// </summary>
    public void Pump() {
        while (queue.TryDequeue(out (SendOrPostCallback Callback, object? State) work)) {
            try {
                work.Callback(work.State);
            }
            catch (Exception e) {
                logger.LogError(e, "Unhandled exception in a main thread callback");
            }
        }
    }
}
