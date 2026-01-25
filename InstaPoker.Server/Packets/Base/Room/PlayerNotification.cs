namespace InstaPoker.Server;

// notification
public class PlayerLeftNotification : Packet
{
    public override PacketType Type => PacketType.PlayerLeftNotification;
    public override PacketCategory Category => PacketCategory.Notification;
    public string PlayerName { get; set; }
}