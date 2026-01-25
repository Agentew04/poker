namespace InstaPoker.Server;

// request
public class CreateRoomRequest : Packet
{
    public override PacketType Type => PacketType.CreateRoomRequest;
    public override PacketCategory Category => PacketCategory.Request;
}

// response
public class CreateRoomResponse : Packet
{
    public override PacketType Type => PacketType.CreateRoomResponse;
    public override PacketCategory Category => PacketCategory.Response;
    
    public string RoomCode { get; set; }
}