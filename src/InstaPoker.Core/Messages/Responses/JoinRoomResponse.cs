using InstaPoker.Core.Messages.Requests;

namespace InstaPoker.Core.Messages.Responses;

/// <summary>
/// Response to a user that wanted to join a room. Pairs with <see cref="JoinRoomRequest"/>.
/// </summary>
/// <remarks>Server to Client</remarks>
public class JoinRoomResponse : Message {
    
    /// <summary>
    /// The status of the request. Wether the join operation was successful or the reason that it was not.
    /// </summary>
    public JoinRoomResult Result { get; set; }
    
    /// <summary>
    /// The list of users that were in the room. Does not include the user that just joined.
    /// </summary>
    public List<string> ConnectedUsers { get; set; } = [];
    
    /// <summary>
    /// The settings of the room that the player joined.
    /// </summary>
    public RoomSettings Settings { get; set; } = new();
    
    /// <summary>
    /// The username of the owner of the room. Must have an equal entry in <see cref="ConnectedUsers"/>.
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
    
    public override Guid UniqueId => new("02E08670-47CE-4E20-97FC-8F10D101CFC5");
    
    public override void Write(BinaryWriter bw) {
        bw.Write((int)Result);
        bw.Write(ConnectedUsers.Count);
        foreach (string user in ConnectedUsers) {
            bw.Write(user);
        }
        Settings.Write(bw);
        bw.Write(OwnerName);
    }

    public override void Read(BinaryReader br) {
        Result = (JoinRoomResult)br.ReadInt32();
        int userCount = br.ReadInt32();
        ConnectedUsers.Clear();
        ConnectedUsers.EnsureCapacity(userCount);
        for (int i = 0; i < userCount; i++) {
            ConnectedUsers.Add(br.ReadString());
        }
        Settings.Read(br);
        OwnerName = br.ReadString();
    }
}

/// <summary>
/// Enumerator of the result status of a <see cref="JoinRoomResponse"/>.
/// </summary>
public enum JoinRoomResult {
    Success,
    RoomDoesNotExist,
    RoomFull,
    UsernameAlreadyExist,
    AlreadyInOtherRoom,
    GameAlreadyStarted
}