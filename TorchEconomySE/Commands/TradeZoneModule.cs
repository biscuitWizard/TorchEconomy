using System.Linq;
using System.Text;
using NLog;
using Torch.Commands;
using TorchEconomySE.Managers;

namespace TorchEconomySE.Commands
{
    [Category("econ")]
    public class TradeZoneModule : CommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.TradeZone");

        [Command("zones", "Lists all trade zones player is currently in.")]
        public void GetZones()
        {
            var tradeZoneManager = EconomyPlugin.GetManager<TradeZoneManager>();

            var playerPosition = Context.Player.Character.GetPosition();
            var tradeZones = tradeZoneManager.GetTradeZonesInRange(playerPosition).ToArray();
            if (tradeZones.Length == 0)
            {
                Context.Respond("There are no trade zones in range of you.");
                return;
            }
            
            var stringBuilder = new StringBuilder("Nearby Trade Zones:");
            foreach (var tradeZone in tradeZones)
            {
                stringBuilder.AppendLine($"[{tradeZone.Name}] {tradeZone.Position.DistanceFrom(playerPosition)}m");
            }

            Context.Respond(stringBuilder.ToString());
        }

        [Command("buy", "<itemNameOrIndex> <quantity>: Purchases a quantity of items from nearby tradezones at the lowest prices available.")]
        public void Buy(string itemNameOrIndex, decimal quantity)
        {
            var stringBuilder = new StringBuilder("Available Items: ");
            var marketManager = EconomyPlugin.GetManager<MarketSimManager>();
            
            if (definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
            {
                if (ItemQuantity != Math.Truncate(ItemQuantity))
                {
                    MessageClientTextMessage.SendMessage(SenderSteamId, "SELL", "You must provide a whole number for the quantity of that item.");
                    return;
                }
                //ItemQuantity = Math.Round(ItemQuantity, 0);  // Or do we just round the number?
            }
        }

        [Command("sell", "<itemName> <quantity>: Attempts to sell a quantity of items to nearby trade zones at best possible price from character inventory and ship inventory.")]
        public void Sell(string itemName, decimal quantity)
        {

        }
    }
}
