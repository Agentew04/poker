using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;

namespace InstaPoker.Server;

public class Router {
    
    public UserManager UserManager { get; set; }
    public RoomManager RoomManager { get; set; }
    
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
        }
    }
}