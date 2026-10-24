using System.Text;
using InstaPoker.Core;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;
using InstaPoker.Server.Games;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Server;

public sealed class RoomManager(ILogger<RoomManager> logger) {

    private readonly List<Room> rooms = [];

    private readonly Dictionary<ClientConnection, Room?> userRoom = [];

    public async Task CreateNewRoom(ClientConnection owner) {
        Room r = new() {
            Code = GenerateRandomCode(),
            ConnectedUsers = [owner],
            Owner = owner,
            Settings = GetDefaultRoomSettings()
        };
        logger.LogInformation("Created room {code}", r.Code);
        rooms.Add(r);
        await owner.MessageWriter.WriteAsync(new CreateRoomResponse() {
            RoomCode = r.Code,
            Settings = r.Settings
        });
        userRoom[owner] = r;
    }

    public async Task UpdateRoomSettings(ClientConnection owner, RoomSettings settings) {
        Room? r = userRoom[owner];
        if (r is null || r.Owner != owner) {
            logger.LogWarning("Non owner {username} tried to update room settings", owner.Username);
            return;
        }

        r.Settings = settings;
        // notify all players, including owner
        await Parallel.ForEachAsync(r.ConnectedUsers, async (conn, _) => {
            logger.LogDebug("Sending new settings to {username}", conn.Username);
            await conn.MessageWriter.WriteAsync(new RoomSettingsChangeNotification() {
                NewSettings = r.Settings
            });
        });
    }

    public async Task UserLeaveRoom(ClientConnection conn) {
        Room? room = userRoom[conn];
        if (room is null) {
            logger.LogDebug("User {username} was not in a room", conn.Username);
            return;
        }
        userRoom[conn] = null;

        room.ConnectedUsers.Remove(conn);
        // notify users that player has left the room
        await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
            await user.MessageWriter.WriteAsync(new RoomListUpdatedNotification() {
                Username = conn.Username,
                UpdateType = LobbyListUpdateType.UserLeft
            });
        });

        if (room.ConnectedUsers.Count == 0) {
            logger.LogInformation("Room {code} is empty, removing it", room.Code);
            rooms.Remove(room);
        }

        // if user was owner decide new owner
        if (room.Owner == conn && room.ConnectedUsers.Count > 0) {
            ClientConnection newOwner = room.ConnectedUsers[Random.Shared.Next(room.ConnectedUsers.Count)];
            logger.LogInformation("New room owner: {username}", newOwner.Username);
            room.Owner = newOwner;
            await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
                await user.MessageWriter.WriteAsync(new NewRoomOwnerNotification() {
                    Owner = newOwner.Username
                });
            });
        }
    }

    public async Task UserKick(ClientConnection conn, string kickedUser) {
        Room? room = userRoom[conn];
        if (room is null || room.Owner != conn) {
            return;
        }

        ClientConnection? kicked = room.ConnectedUsers.Find(x => x.Username == kickedUser);
        if (kicked is null) {
            logger.LogWarning("Tried kicking non existent user {username}", kickedUser);
            return;
        }

        // send notification and remove later. need to notify kicked player as well
        await Parallel.ForEachAsync(room.ConnectedUsers, async (user, _) => {
            await user.MessageWriter.WriteAsync(new RoomListUpdatedNotification() {
                Username = kickedUser,
                UpdateType = LobbyListUpdateType.UserKicked
            });
        });
        userRoom[kicked] = null;
        room.ConnectedUsers.Remove(kicked);
    }

    public async Task UserJoinRoom(ClientConnection conn, JoinRoomRequest req) {
        userRoom.TryAdd(conn, null);
        if (userRoom[conn] != null) {
            logger.LogWarning("User {username} tried to join a room while already in one", conn.Username);
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.AlreadyInOtherRoom
            });
            return;
        }

        Room? r = rooms.Find(x => x.Code == req.RoomCode);

        if (r is null) {
            logger.LogWarning("User {username} tried entering non existent room with code {code}. Rooms available: {rooms}",
                conn.Username, req.RoomCode, string.Join(", ", rooms.Select(x => x.Code)));
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.RoomDoesNotExist
            });
            return;
        }

        if (r.Settings.MaxPlayers <= r.ConnectedUsers.Count) {
            logger.LogWarning("Room {code} is full for {username}", r.Code, conn.Username);
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.RoomFull
            });
            return;
        }

        if (r.ConnectedUsers.Any(x => x.Username == conn.Username)) {
            logger.LogWarning("Duplicated username {username} in room {code}", conn.Username, r.Code);
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.UsernameAlreadyExist
            });
            return;
        }

        if (r.InGame) {
            logger.LogWarning("User {username} tried joining running game in room {code}", conn.Username, r.Code);
            await conn.MessageWriter.WriteAsync(new JoinRoomResponse() {
                Result = JoinRoomResult.GameAlreadyStarted
            });
            return;
        }

        logger.LogInformation("User {username} joined room {code}", conn.Username, r.Code);
        userRoom[conn] = r;
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

    public void UnexpectedDisconnect(ClientConnection conn) {
        // conn has already been disconnected
        if (userRoom.TryGetValue(conn, out Room? room) && room is not null) {
            _ = LeaveRoomSafelyAsync(conn);
        }
    }

    private async Task LeaveRoomSafelyAsync(ClientConnection conn) {
        try {
            await UserLeaveRoom(conn);
        }
        catch (Exception e) {
            logger.LogWarning(e, "Failed to remove {username} from their room after disconnect", conn.Username);
        }
    }

    public async Task StartGame(ClientConnection conn) {
        if (!userRoom.TryGetValue(conn, out Room? room) || room is null) {
            return;
        }

        room.InGame = true;
        room.Table = new PokerTable(room);

        _ = room.Table.StartGame();
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
