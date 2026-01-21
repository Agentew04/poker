using InstaPoker.Client.Game;

namespace InstaPoker.Client.Network;

public static class NetworkManager {
    // todo
    public static async Task<string> CreateRoom() {
        // await Task.Delay(Random.Shared.Next(500, 1000));
        return Random.Shared.Next(100000, 1000000).ToString();
    }
    
    public static async Task JoinRoom(string code) {
        // return Task.Delay(Random.Shared.Next(500, 1000));
    }

    public static async Task KickUser(string name) {
        //return Task.Delay(Random.Shared.Next(100,500));
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