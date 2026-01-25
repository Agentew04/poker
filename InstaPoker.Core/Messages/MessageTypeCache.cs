using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Core.Messages;

public static class MessageTypeCache {
    private static readonly Dictionary<Guid, Type> MessageTypes;

    static MessageTypeCache() {
        MessageTypes = [];
        MessageTypes[new LeaveRoomNotification().UniqueId] = typeof(LeaveRoomNotification);
        MessageTypes[new NewRoomOwnerNotification().UniqueId] = typeof(NewRoomOwnerNotification);
        MessageTypes[new RoomListUpdatedNotification().UniqueId] = typeof(RoomListUpdatedNotification);
        MessageTypes[new RoomSettingsChangeNotification().UniqueId] = typeof(RoomSettingsChangeNotification);
        MessageTypes[new CreateRoomRequest().UniqueId] = typeof(CreateRoomRequest);
        MessageTypes[new JoinRoomRequest().UniqueId] = typeof(JoinRoomRequest);
        MessageTypes[new CreateRoomResponse().UniqueId] = typeof(CreateRoomResponse);
        MessageTypes[new JoinRoomResponse().UniqueId] = typeof(JoinRoomResponse);
    }

    public static Type? GetMessageType(Guid id) {
        return MessageTypes.GetValueOrDefault(id);
    }
}