using InstaPoker.Client.Graphics;

namespace InstaPoker.Client.Game;

public class LobbyUser {
    public string Name { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool IsLocal { get; set; }
    public Button? KickButton { get; set; } = null;
}