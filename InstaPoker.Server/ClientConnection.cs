using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using CommunityToolkit.Mvvm.Messaging;
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
            Console.WriteLine("Waiting for client packet");
            Message m = await mr.ReadNextMessageAsync();
            Console.WriteLine("Got message with type: " + m.GetType().Name);
            await IncomingMessages.Writer.WriteAsync(m);
        }

        if (!Client.Connected) {
            Client.Close();
        }
    }

    public Task StopReceiving() {
        return cts!.CancelAsync();
    }
}