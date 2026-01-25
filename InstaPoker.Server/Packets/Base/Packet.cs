namespace InstaPoker.Server;

public enum PacketCategory
{
    Request,
    Response,
    Notification
}

public abstract class Packet
{
    public abstract PacketType Type { get; }
    public abstract PacketCategory Category { get; }
}