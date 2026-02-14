using System.Reflection;
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
        List<Type> types = typeof(Message).Assembly.GetTypes()
            .Where(x => typeof(Message).IsAssignableFrom(x) && !x.IsAbstract)
            .ToList();
        foreach (Type messageType in types) {
            Message? msg = (Message?)Activator.CreateInstance(messageType);
            if (msg is null) {
                Console.WriteLine($"Could not register {messageType.Name} in MessageTypeCache");
                continue;
            }
            MessageTypes[msg.UniqueId] = messageType;
        }
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