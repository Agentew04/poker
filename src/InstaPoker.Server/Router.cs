using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Server;

public sealed class Router(
    MessageBus messageBus,
    RoomManager roomManager,
    ILogger<Router> logger) : BackgroundService {

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        logger.LogInformation("Message router started");

        try {
            // blocks until a connection publishes a message; no polling and no snapshots
            await foreach ((ClientConnection conn, Message item) in messageBus.Reader.ReadAllAsync(stoppingToken)) {
                if (conn.IsClosed) {
                    continue;
                }

                // Find returns default((Type, TaskCompletionSource)) when there is no match, so the
                // tuple itself must be checked instead of a nullable wrapper
                (Type, TaskCompletionSource<Message>) pending =
                    conn.PendingRequests.Find(x => x.Item1 == item.GetType());

                if (pending.Item2 is null) {
                    // process messages syncronously. avoid race conditions(?)
                    await Route(conn, item);
                }
                else {
                    logger.LogTrace("Completed request {type} for {username}",
                        item.GetType().Name, conn.Username);
                    pending.Item2.TrySetResult(item);
                    conn.PendingRequests.Remove(pending);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            // expected during shutdown
        }
        finally {
            logger.LogInformation("Message router stopped");
        }
    }

    private async Task Route(ClientConnection conn, Message msg) {
        try {
            switch (msg) {
                case CreateRoomRequest: {
                    await roomManager.CreateNewRoom(conn);
                    break;
                }
                case RoomSettingsChangeNotification roomSettingsChangeNotification: {
                    await roomManager.UpdateRoomSettings(conn, roomSettingsChangeNotification.NewSettings);
                    break;
                }
                case LeaveRoomNotification: {
                    await roomManager.UserLeaveRoom(conn);
                    break;
                }
                case KickUserNotification kickUserNotification: {
                    await roomManager.UserKick(conn, kickUserNotification.Username);
                    break;
                }
                case JoinRoomRequest joinRoomRequest: {
                    await roomManager.UserJoinRoom(conn, joinRoomRequest);
                    break;
                }
                case OwnerStartGameNotification: {
                    await roomManager.StartGame(conn);
                    break;
                }
            }
        }
        catch (Exception e) {
            logger.LogError(e, "Router caught an exception. Connection: {username}. Message: {type}",
                conn.Username, msg.GetType().Name);
        }
    }
}
