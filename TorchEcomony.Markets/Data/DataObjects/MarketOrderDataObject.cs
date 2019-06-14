using TorchEconomy.Markets.Data.Types;

namespace TorchEconomy.Markets.Data.DataObjects
{
    public class MarketOrderDataObject
    {
        public long Id { get; set; }
        public BuyOrderType OrderType { get; set; }
        public string DefinitionId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public ulong TradeZoneId { get; set; }
        public int CreatedOn { get; set; }
    }
}
