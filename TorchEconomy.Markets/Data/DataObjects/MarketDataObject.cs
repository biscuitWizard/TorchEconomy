using System;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Schema;
using TorchEconomy.Markets.Data.Types;

namespace TorchEconomy.Markets.Data.DataObjects
{
    public class MarketDataObject : IDataObject
    {
        [PrimaryKey]
        public long Id { get; set; }
        [Required]
        [StringLength(32)]
        public string Name { get; set; }
        [Required]
        public float Range { get; set; }
        [Required]
        public ulong CreatorId { get; set; }
        [Required]
        public long ParentGridId { get; set; }
        public long? AccountId { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
        
        [Required]
        [DefaultValue(false)]
        public bool IsOpen { get; set; }
        
        [Required]
        [DefaultValue(false)]
        public bool IsNPC { get; set; }
    }
}
