using ProtoBuf;

namespace TorchEconomy.Data.DataObjects
{
    [ProtoContract]
    public class AccountDataObject
    {
        [ProtoMember(101)]
        public ulong Id { get; set; }
        [ProtoMember(102)]
        public ulong PlayerId { get; set; }
        [ProtoMember(103)]
        public decimal Balance { get; set; }
        [ProtoMember(104)]
        public bool IsNPC { get; set; }
        [ProtoMember(105)]
        public bool IsPrimary { get; set; }
    }
}
