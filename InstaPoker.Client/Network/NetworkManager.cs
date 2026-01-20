using InstaPoker.Client.Game;

namespace InstaPoker.Client.Network;

public static class NetworkManager {
    // todo
    public static async Task<string> CreateRoom() {
        await Task.Delay(Random.Shared.Next(1000, 3000));
        return "123456";
    }
    
    public static Task JoinRoom(string code) {
        return Task.Delay(Random.Shared.Next(1000, 3000));
    }

    public static RoomSettings GetSettings() {
        // return mock settings
        return new RoomSettings() {
            IsAllInEnabled = true,
            MaxBet = 1000,
            MaxPlayers = 4,
            SmallBlind = 10
        };
    }
    
    
}