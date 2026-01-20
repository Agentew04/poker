using InstaPoker.Client.Game;

namespace InstaPoker.Client;

class Program {
    static void Main(string[] args) {
        PokerGame window = new();
        window.Run();
    }
}