namespace InstaPoker.Server;

class Program {
    public static async Task Main(string[] args) {
        Server server = new();
        await server.Run();
    }
}