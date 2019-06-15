using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Schema;
using TorchEconomy.Markets.Data.Types;

namespace TorchEconomy.Markets.Data.DataObjects
{
    public class MarketOrderDataObject : IDataObject
    {
        [PrimaryKey]
        public long Id { get; set; }
        [EnumAsByte]
        [Required]
        public BuyOrderType OrderType { get; set; }
        [Required]
        [StringLength(128)]
        public string DefinitionId { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public ulong TradeZoneId { get; set; }
        [Required]
        public int CreatedOn { get; set; }
    }
}
