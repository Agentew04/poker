using InstaPoker.Core.Messages.Requests;

namespace InstaPoker.Core.Messages.Responses;

/// <summary>
/// Message answering <see cref="BalanceQueryRequest"/>, telling the server
/// how much money a player currently has.
/// </summary>
/// <remarks>Client to Server</remarks>
public class BalanceQueryResponse : Message {
    public override Guid UniqueId => new("ECCCFCFB-6CCF-422F-883F-4A62AE043905");
    
    public int Balance { get; set; }
    
    public override void Write(BinaryWriter bw) {
        bw.Write(Balance);
    }

    public override void Read(BinaryReader br) {
        Balance = br.ReadInt32();
    }
}