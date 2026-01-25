using System.Runtime.Intrinsics.X86;
using System.Text;
using InstaPoker.Core;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Server;

public class RoomManager {

    private readonly List<Room> rooms = [];

    public async Task CreateNewRoom(ClientConnection owner) {
        Room r = new() {
            Code = GenerateRandomCode(),
            ConnectedUsers = [owner],
            Owner = owner,
            Settings = GetDefaultRoomSettings()
        };
        Console.WriteLine("Create room with code: " + r.Code);
        rooms.Add(r);
        await owner.MessageWriter.WriteAsync(new CreateRoomResponse() {
            RoomCode = r.Code,
            Settings = r.Settings
        });
    }

    public async Task UpdateRoomSettings(ClientConnection owner, RoomSettings settings) {
        Room? r = rooms.Find(x => x.Owner == owner);
        if (r is null) {
            Console.WriteLine("Non owner tried to update room settings");
            return;
        }

        r.Settings = settings;
        // notify all players, including owner
        await Parallel.ForEachAsync(r.ConnectedUsers, async (conn,_) => {
            Console.WriteLine("sending new settings to: " + conn.Username);
            await conn.MessageWriter.WriteAsync(new RoomSettingsChangeNotification() {
                NewSettings = r.Settings
            });
        });
    }

    public async Task UserLeaveRoom(ClientConnection conn) {
        Room? emptyRoom = null;
        foreach (Room room in rooms) {
            if (!room.ConnectedUsers.Contains(conn)) {
                continue;
            }
            room.ConnectedUsers.Remove(conn);
            // notify users that player has left the room
            await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
                await user.MessageWriter.WriteAsync(new RoomListUpdatedNotification() {
                    Username = conn.Username,
                    UpdateType = LobbyListUpdateType.UserLeft
                });
            });

            if (room.ConnectedUsers.Count == 0) {
                emptyRoom = room;
            }
            
            if (room.Owner != conn) {
                continue;
            }
            // if user was owner decide new owner
            ClientConnection newOwner = room.ConnectedUsers[Random.Shared.Next(room.ConnectedUsers.Count)];
            Console.WriteLine("New room owner: " + newOwner.Username);
            room.Owner = newOwner;
            await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
                await user.MessageWriter.WriteAsync(new NewRoomOwnerNotification() {
                    Owner = newOwner.Username
                });
            });
        }

        if (emptyRoom is not null) {
            rooms.Remove(emptyRoom);
        }
    }

    public async Task UserKick(ClientConnection conn, string kickedUser) {
        Room? room = rooms.Find(x => x.Owner == conn);
        if (room is null) {
            return;
        }

        ClientConnection? kicked = room.ConnectedUsers.Find(x => x.Username == kickedUser);
        if (kicked is null) {
            return;
        }
        
        // send notification and remove later. need to notify kicked player as well
        await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
            await user.MessageWriter.WriteAsync(new RoomListUpdatedNotification() {
                Username = kickedUser,
                UpdateType = LobbyListUpdateType.UserKicked
            });
        });
        room.ConnectedUsers.Remove(kicked);
    }
    
    
    private string GenerateRandomCode() {
        StringBuilder sb = new();
        const int count = 6;
        for (int i = 0; i < count; i++) {
            sb.Append(Random.Shared.Next(0, 10));
        }

        return sb.ToString();
    }

    private RoomSettings GetDefaultRoomSettings() {
        return new RoomSettings() {
            MaxPlayers = 8,
            SmallBlind = 10,
            MaxBet = 1000,
            IsAllInEnabled = true
        };
    }
}