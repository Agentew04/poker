using System.Net.Sockets;
using System.Threading.Channels;
using InstaPoker.Core.Messages;

namespace InstaPoker.Server;

public class ClientConnection {

    public required TcpClient Client { get; set; }

    public string Username { get; set; } = string.Empty;

    public Channel<Message> IncomingMessages { get; set; } = Channel.CreateUnbounded<Message>();
    
    public required NetworkStream NetworkStream { get; set; }

    public required MessageWriter MessageWriter { get; set; }

    private CancellationTokenSource? cts = new();


    public async Task StartReceivingAsync() {
        using MessageReader mr = new(NetworkStream, false);
        
        while (!cts!.IsCancellationRequested && Client.Connected) {
            // wait for new packet
            Message m = await mr.ReadNextMessageAsync(cts.Token);
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

    public Task StopReceiving() {
        return cts!.CancelAsync();
    }
}