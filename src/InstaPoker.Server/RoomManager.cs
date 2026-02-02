using System.Text;
using InstaPoker.Core;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Server;

public static class RoomManager {

    private static readonly List<Room> Rooms = [];

    private static readonly Dictionary<ClientConnection, Room?> UserRoom = [];

    public static async Task CreateNewRoom(ClientConnection owner) {
        Room r = new() {
            Code = GenerateRandomCode(),
            ConnectedUsers = [owner],
            Owner = owner,
            Settings = GetDefaultRoomSettings()
        };
        Console.WriteLine("Create room with code: " + r.Code);
        Rooms.Add(r);
        await owner.MessageWriter.WriteAsync(new CreateRoomResponse() {
            RoomCode = r.Code,
            Settings = r.Settings
        });
        UserRoom[owner] = r;
    }

    public static async Task UpdateRoomSettings(ClientConnection owner, RoomSettings settings) {
        Room? r = UserRoom[owner];
        if (r is null || r.Owner != owner) {
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

    public static async Task UserLeaveRoom(ClientConnection conn) {
        Room? room = UserRoom[conn];
        if (room is null) {
            Console.WriteLine("user was not in room");
            return;
        }
        UserRoom[conn] = null;

        room.ConnectedUsers.Remove(conn);
        // notify users that player has left the room
        await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
            await user.MessageWriter.WriteAsync(new RoomListUpdatedNotification() {
                Username = conn.Username,
                UpdateType = LobbyListUpdateType.UserLeft
            });
        });
        
        if (room.ConnectedUsers.Count == 0) {
            Console.WriteLine($"Room {room.Code} is empty. removing");
            Rooms.Remove(room);
        }

        // if user was owner decide new owner
        if (room.Owner == conn) {
            ClientConnection newOwner = room.ConnectedUsers[Random.Shared.Next(room.ConnectedUsers.Count)];
            Console.WriteLine("New room owner: " + newOwner.Username);
            room.Owner = newOwner;
            await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
                await user.MessageWriter.WriteAsync(new NewRoomOwnerNotification() {
                    Owner = newOwner.Username
                });
            });
        }

        
    }

    public static async Task UserKick(ClientConnection conn, string kickedUser) {
        Room? room = UserRoom[conn];
        if (room is null || room.Owner != conn) {
            return;
        }

        ClientConnection? kicked = room.ConnectedUsers.Find(x => x.Username == kickedUser);
        if (kicked is null) {
            Console.WriteLine("Tried kicking non existent user");
            return;
        }
        
        // send notification and remove later. need to notify kicked player as well
        await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
            await user.MessageWriter.WriteAsync(new RoomListUpdatedNotification() {
                Username = kickedUser,
                UpdateType = LobbyListUpdateType.UserKicked
            });
        });
        UserRoom[kicked] = null;
        room.ConnectedUsers.Remove(kicked);
    }

    public static async Task UserJoinRoom(ClientConnection conn, JoinRoomRequest req) {
        UserRoom.TryAdd(conn, null);
        if (UserRoom[conn] != null) {
            Console.WriteLine("User");
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.AlreadyInOtherRoom
            });
            return;
        }

        Room? r = Rooms.Find(x => x.Code == req.RoomCode);

        if (r is null) {
            Console.WriteLine($"User {conn.Username} tried entering non existent room with code: \"{req.RoomCode}\". Rooms Available: {string.Join(", ", Rooms.Select(x => $"\"{x.Code}\""))}");
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.RoomDoesNotExist
            });
            return;
        }

        if (r.Settings.MaxPlayers <= r.ConnectedUsers.Count) {
            Console.WriteLine($"Room {r.Code} full for {conn.Username}");
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.RoomFull
            });
            return;
        }

        if (r.ConnectedUsers.Any(x => x.Username == conn.Username)) {
            Console.WriteLine($"Duplicated username {conn.Username} in room {r.Code}");
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.UsernameAlreadyExist
            });
            return;
        }

        Console.WriteLine($"User {conn.Username} joined room {r.Code}");
        UserRoom[conn] = r;
        await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
            Result = JoinRoomResult.Success,
            Settings = r.Settings,
            ConnectedUsers = r.ConnectedUsers.Select(x => x.Username).ToList(),
            OwnerName = r.Owner.Username
        });
        // tell other players about new one
        await Parallel.ForEachAsync(r.ConnectedUsers, async (user, _) => {
            await user.MessageWriter.WriteAsync(new RoomListUpdatedNotification() {
                Username = conn.Username,
                UpdateType = LobbyListUpdateType.UserJoined
            });
        });
        r.ConnectedUsers.Add(conn);
    }

    public static void UnexpectedDisconnect(ClientConnection conn) {
        // conn has already been disconnected
        if (UserRoom.TryGetValue(conn, out Room? room)) {
            _ = UserLeaveRoom(conn);
        }
    }
    
    private static string GenerateRandomCode() {
        StringBuilder sb = new();
        const int count = 6;
        for (int i = 0; i < count; i++) {
            sb.Append(Random.Shared.Next(0, 10));
        }

        return sb.ToString();
    }

    private static RoomSettings GetDefaultRoomSettings() {
        return new RoomSettings() {
            MaxPlayers = 8,
            SmallBlind = 10,
            MaxBet = 1000,
            IsAllInEnabled = true
        };
    }
}