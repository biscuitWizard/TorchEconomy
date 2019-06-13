using TorchEconomy.Data.Types;

namespace TorchEconomy.Markets.Data.DataObjects
{
    public class MarketOrderDataObject
    {
        public ulong Id { get; set; }
        public BuyOrderType OrderType { get; set; }
        public string DefinitionIdHash { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public ulong TradeZoneId { get; set; }
    }
}
