using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using InstaPoker.Core.Messages;

namespace InstaPoker.Client.Network;

public class MessageHandler(NetworkStream ns, MessageWriter writer, MessageReader reader) {
    
    private readonly Dictionary<Type, TaskCompletionSource<Message>> pendingRequests = [];

    public IReadOnlyList<Type> GetPendingRequests() {
        return pendingRequests.Keys.ToImmutableList();
    }

    public List<Message> PendingMessages = [];

    public async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request) where TRequest : Message where TResponse : Message {
        await writer.WriteAsync(request);
        TaskCompletionSource<Message> tcs = new();
        pendingRequests[typeof(TResponse)] = tcs;
        Message response = await tcs.Task;
        return (TResponse)response;
    }

    public async Task SendNotification<TNotification>(TNotification notification) where TNotification : Message {
        await writer.WriteAsync(notification);
    }

    public bool TryGetPendingMessage<T>([NotNullWhen(true)] out T? msg) where T : Message {
        var first = (T?)PendingMessages.Find(x => x is T);
        if (first is not null) {
            PendingMessages.Remove(first);
            msg = first;
            return true;
        }

        msg = null;
        return false;
    }

    public void CheckForNewMessages() {
        if (PendingMessages.Count > 0) {
            Console.WriteLine("Warning. deleting untreated messages in 1 frame time: ");
            foreach (Message msg in PendingMessages) {
                Console.WriteLine("\t- " + msg.GetType().Name);
            }
        }
        PendingMessages.Clear();

        if (pendingRequests.Count > 0) {
            Console.WriteLine("Pending requests: " + pendingRequests.Count);
            
        }
        
        while (ns.DataAvailable) {
            Message m = reader.ReadNextMessageAsync(CancellationToken.None).GetAwaiter().GetResult();
            Type type = m.GetType();
            if (pendingRequests.ContainsKey(type)) {
                Console.WriteLine("fulfilled request");
                pendingRequests[type].SetResult(m);
                pendingRequests.Remove(type);
            }
            else {
                PendingMessages.Add(m);
            }
        }
    }
}