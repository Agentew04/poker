using Microsoft.Extensions.Hosting;

namespace InstaPoker.Server;

class Program {
    public static async Task Main(string[] args) {
        using IHost host = ServerHost.Build(args);
        await host.RunAsync();
    }
}
