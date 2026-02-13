using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;

namespace InstaPoker.Server;

public class Router {
    
    public async Task RouteMessagesLoop(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            foreach (ClientConnection conn in UserManager.Connections) {
                while (conn.IncomingMessages.Reader.TryRead(out Message item)) {
                    Route(conn, item);
                }
            }

            await Task.Delay(1, token);
        }
    }

    private async Task Route(ClientConnection conn, Message msg) {
        try {
            switch (msg) {
                case CreateRoomRequest: {
                    await RoomManager.CreateNewRoom(conn);
                    break;
                }
                case RoomSettingsChangeNotification roomSettingsChangeNotification: {
                    await RoomManager.UpdateRoomSettings(conn, roomSettingsChangeNotification.NewSettings);
                    break;
                }
                case LeaveRoomNotification: {
                    await RoomManager.UserLeaveRoom(conn);
                    break;
                }
                case KickUserNotification kickUserNotification: {
                    await RoomManager.UserKick(conn, kickUserNotification.Username);
                    break;
                }
                case JoinRoomRequest joinRoomRequest: {
                    await RoomManager.UserJoinRoom(conn, joinRoomRequest);
                    break;
                }
            }
        }
        catch (Exception e) {
            Console.WriteLine($"Router caught an exception! Connection: {conn.Username}. Message Type: {msg.GetType().Name}");
            Console.WriteLine($"Error Type: {e.GetType().Name}; Message: {e.Message}");
            Console.WriteLine($"Stacktrace: {e.StackTrace}");
        }
    }
}