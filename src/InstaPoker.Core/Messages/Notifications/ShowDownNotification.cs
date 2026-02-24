using InstaPoker.Core.Games;

namespace InstaPoker.Core.Messages.Notifications;

/// <summary>
/// Broadcast at showdown — reveals all remaining players' cards.
/// </summary>

public class ShowdownNotification : Message {

    public static readonly Guid Id = new("29F7AE17-7C6D-4852-9E03-FFF3DC8977AC");
    public override Guid UniqueId => Id;

    /// <summary>Username → their 2 hole cards.</summary>
    public Dictionary<string, (GameCard, GameCard)> Hands { get; set; } = [];

    public override void Write(BinaryWriter bw) {
        bw.Write(Hands.Count);
        foreach ((string user, (GameCard c1, GameCard c2)) in Hands) {
            bw.Write(user);
            c1.Write(bw);
            c2.Write(bw);
        }
    }

    public override void Read(BinaryReader br) {
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++) {
            string user = br.ReadString();
            GameCard c1 = new(); c1.Read(br);
            GameCard c2 = new(); c2.Read(br);
            Hands[user] = (c1, c2);
        }
    }
}

/// <summary>
/// Broadcast at the end of a hand - announces the winner(s) and updated balances.
/// </summary>
public class RoundEndNotification : Message {

    public static readonly Guid Id = new("E39884C9-964E-4274-876C-241231D6E097");
    public override Guid UniqueId => Id;

    public List<string> Winners { get; set; } = [];
    public int PotWon { get; set; }
    public string WinningHandDescription { get; set; } = string.Empty;

    /// <summary>Username → balance after pot distribution.</summary>
    public Dictionary<string, int> UpdatedBalances { get; set; } = [];

    public override void Write(BinaryWriter bw) {
        bw.Write(Winners.Count);
        foreach (string w in Winners) bw.Write(w);
        bw.Write(PotWon);
        bw.Write(WinningHandDescription);
        bw.Write(UpdatedBalances.Count);
        foreach ((string user, int balance) in UpdatedBalances) {
            bw.Write(user);
            bw.Write(balance);
        }
    }

    public override void Read(BinaryReader br) {
        int wCount = br.ReadInt32();
        for (int i = 0; i < wCount; i++) Winners.Add(br.ReadString());
        PotWon = br.ReadInt32();
        WinningHandDescription = br.ReadString();
        int bCount = br.ReadInt32();
        for (int i = 0; i < bCount; i++)
            UpdatedBalances[br.ReadString()] = br.ReadInt32();
    }
}