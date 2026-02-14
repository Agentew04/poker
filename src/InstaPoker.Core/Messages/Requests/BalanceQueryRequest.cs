using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Core.Messages.Requests;

/// <summary>
/// Message sent by the server to check how much money a player have.
/// Response is <see cref="BalanceQueryResponse"/>.
/// </summary>
/// <remarks>Server to Client</remarks>
public class BalanceQueryRequest : Message {
    public override Guid UniqueId => new("68955238-CDCD-4124-8632-5AD534B8494A");
    
    public override void Write(BinaryWriter bw) {
        // empty
    }

    public override void Read(BinaryReader br) {
        // empty
    }
}