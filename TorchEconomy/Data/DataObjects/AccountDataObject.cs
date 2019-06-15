using ProtoBuf;
using TorchEconomy.Data.Schema;

namespace TorchEconomy.Data.DataObjects
{
    [ProtoContract]
    public class AccountDataObject : IDataObject
    {
        [PrimaryKey]
        [ProtoMember(101)]
        public long Id { get; set; }
        [Required]
        [ProtoMember(102)]
        public ulong PlayerId { get; set; }
        [Required]
        [ProtoMember(103)]
        public decimal Balance { get; set; }
        [Required]
        [DefaultValue(false)]
        [ProtoMember(104)]
        public bool IsNPC { get; set; }
        [Required]
        [DefaultValue(false)]
        [ProtoMember(105)]
        public bool IsPrimary { get; set; }
        [Required]
        [ProtoMember(106)]
        public string Nickname { get; set; }
        [Required]
        [DefaultValue(false)]
        [ProtoMember(107)]
        public bool IsDeleted { get; set; }
    }
}
