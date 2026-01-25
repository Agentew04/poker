namespace InstaPoker.Server;

public class RoomSettings
{
    public int MaxPlayers { get; set; } = 4;
    public int SmallBlind { get; set; } = 10;
    public int MaxBet { get; set; } = 1000;
    public bool IsAllInEnabled { get; set; } = true;
}