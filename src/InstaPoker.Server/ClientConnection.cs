using System.Net.Sockets;
using System.Threading.Channels;
using InstaPoker.Core.Messages;

namespace InstaPoker.Server;

public class ClientConnection {

    public required TcpClient Client { get; set; }

    public string Username { get; set; } = string.Empty;

    public Channel<Message> IncomingMessages { get; set; } = Channel.CreateUnbounded<Message>();

    public List<(Type,TaskCompletionSource<Message>)> PendingRequests { get; set; } = [];
    
    public required NetworkStream NetworkStream { get; set; }

    public required MessageWriter MessageWriter { get; set; }

    private CancellationTokenSource? cts = new();


    public async Task StartReceivingAsync() {
        using MessageReader mr = new(NetworkStream, false);

        while (!cts!.IsCancellationRequested && Client.Connected) {
            // wait for new packet
            Message? m;
            try {
                m = await mr.ReadNextMessageAsync(cts.Token);
            }
            catch (IOException) {
                Console.WriteLine($"Lost connection of user {Username}");
                break;
            }
            catch (Exception e) {
                Console.WriteLine($"Unknown exception in {Username} listen loop, skipping. {e.GetType().Name} {e.Message}");
                continue;
            }

            Console.WriteLine("Got message with type: " + m.GetType().Name);
            await IncomingMessages.Writer.WriteAsync(m);
        }

        if (!Client.Connected) {
            Console.WriteLine("User disconnected");
            Client.Close();
            // let user manager tell other pieces of the application that we have been disconnected
            UserManager.RemoveConnection(this);
        }
    }

    public async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request) where TRequest : Message where TResponse : Message{
        TaskCompletionSource<Message> tcs = new();
        PendingRequests.Add((typeof(TResponse), tcs));
        await MessageWriter.WriteAsync(request);
        Message response = await tcs.Task;
        return (TResponse)response;
    }
    
    public Task StopReceiving() {
        return cts!.CancelAsync();
    }
}