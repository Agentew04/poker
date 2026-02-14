using InstaPoker.Core.Games;

namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Notification to tell players that the game has started and they need
/// to change to the game screen.
/// </summary>
/// <remarks>Server to Client</remarks>
public class GameStartNotification : Message {
    public override Guid UniqueId => new("DA3E0163-9F44-49D2-AC31-84ED6206E8DF");

    public string Dealer { get; set; } = string.Empty;

    public List<GameCard> Hand { get; set; } = [];

    public List<GamePlayerMetadata> Players { get; set; } = [];
    
    public override void Write(BinaryWriter bw) {
        bw.Write(Dealer);
        bw.Write(Hand.Count);
        foreach (GameCard card in Hand) {
            card.Write(bw);
        }
        bw.Write(Players.Count);
        foreach (GamePlayerMetadata player in Players) {
            player.Write(bw);
        }
    }

    public override void Read(BinaryReader br) {
        Dealer = br.ReadString();
        int handCount = br.ReadInt32();
        Hand.Clear();
        for (int i = 0; i < handCount; i++) {
            GameCard card = new();
            card.Read(br);
            Hand.Add(card);
        }

        int playerCount = br.ReadInt32();
        Players.Clear();
        for (int i = 0; i < playerCount; i++) {
            GamePlayerMetadata player = new();
            player.Read(br);
            Players.Add(player);
        }
    }
}

public class GamePlayerMetadata : IBinarySerializable{
    
    public string Username { get; set; } = string.Empty;
    
    public int Balance { get; set; }
    
    public void Write(BinaryWriter bw) {
        bw.Write(Username);
        bw.Write(Balance);
    }

    public void Read(BinaryReader br) {
        Username = br.ReadString();
        Balance = br.ReadInt32();
    }
}