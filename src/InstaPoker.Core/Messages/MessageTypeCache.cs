using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Core.Messages;

/// <summary>
/// Static class that holds mappings of <see cref="Guid"/> and its respective <see cref="Message"/>.
/// </summary>
public static class MessageTypeCache {
    private static readonly Dictionary<Guid, Type> MessageTypes;

    static MessageTypeCache() {
        MessageTypes = [];
        MessageTypes[new KickUserNotification().UniqueId] = typeof(KickUserNotification);
        MessageTypes[new LeaveRoomNotification().UniqueId] = typeof(LeaveRoomNotification);
        MessageTypes[new NewRoomOwnerNotification().UniqueId] = typeof(NewRoomOwnerNotification);
        MessageTypes[new RoomListUpdatedNotification().UniqueId] = typeof(RoomListUpdatedNotification);
        MessageTypes[new RoomSettingsChangeNotification().UniqueId] = typeof(RoomSettingsChangeNotification);
        MessageTypes[new CreateRoomRequest().UniqueId] = typeof(CreateRoomRequest);
        MessageTypes[new JoinRoomRequest().UniqueId] = typeof(JoinRoomRequest);
        MessageTypes[new CreateRoomResponse().UniqueId] = typeof(CreateRoomResponse);
        MessageTypes[new JoinRoomResponse().UniqueId] = typeof(JoinRoomResponse);
    }

    /// <summary>
    /// Returns the <see cref="Type"/> associated with a <see cref="Guid"/> or <see langword="null"/> if
    /// not registered.
    /// </summary>
    /// <param name="id">The message id to be queried</param>
    /// <returns>The type of the message, inheriting from <see cref="Message"/> or <see langword="null"/></returns>
    public static Type? GetMessageType(Guid id) {
        return MessageTypes.GetValueOrDefault(id);
    }
}