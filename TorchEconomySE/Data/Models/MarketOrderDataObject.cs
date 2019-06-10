using TorchEconomySE.Data.Types;

namespace TorchEconomySE.Data.Models
{
    public class MarketOrderDataObject
    {
        public ulong Id { get; set; }
        public BuyOrderType OrderType { get; set; }
    }
}
