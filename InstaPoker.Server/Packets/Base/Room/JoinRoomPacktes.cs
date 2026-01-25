using InstaPoker.Server.Packets.Base;

namespace InstaPoker.Server.Packets.Room;

// request
public class JoinRoomRequest : Packet
{
    public override PacketType Type => PacketType.JoinRoomRequest;
    public override PacketCategory Category => PacketCategory.Request;
}

// response
public class JoinRoomResponse : Packet
{
    public override PacketType Type => PacketType.JoinRoomResponse;
    public override PacketCategory Category => PacketCategory.Response;
}