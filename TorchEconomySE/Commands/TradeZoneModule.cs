using NLog;
using Torch.Commands;

namespace TorchEconomySE.Commands
{
    [Category("econ")]
    public class TradeZoneModule : CommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.TradeZone");

        [Command("zones", "Lists all trade zones player is currently in.")]
        public void GetZones()
        {

        }

        [Command("buy", "<itemNameOrIndex> <quantity>: Purchases a quantity of items from nearby tradezones at the lowest prices available.")]
        public void Buy(string itemNameOrIndex, decimal quantity)
        {

        }

        [Command("sell", "<itemName> <quantity>: Attempts to sell a quantity of items to nearby trade zones at best possible price from character inventory and ship inventory.")]
        public void Sell(string itemName, decimal quantity)
        {

        }
    }
}
