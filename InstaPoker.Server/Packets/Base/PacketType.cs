namespace InstaPoker.Server;

public enum PacketType
{
    Unknown = 0,
    
    // requests
    CreateRoomRequest = 1,
    JoinRoomRequest = 2,
    
    // responses
    CreateRoomResponse = 11,
    JoinRoomResponse = 12,
    
    // notifications
    PlayerJoinedNotification = 21,
    PlayerLeftNotification = 22,
}