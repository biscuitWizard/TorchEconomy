using System;
using System.Linq;
using System.Text;
using NLog;
using Torch.Commands;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Managers;
using VRage.Game;

namespace TorchEconomy.Markets.Commands
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

        [Command("list", "Lists available goods to buy.")]
        public void List()
        {
            var marketManager = EconomyPlugin.GetManager<MarketSimManager>();
            var stringBuilder = new StringBuilder("Available Items:");

            var index = 0;
            foreach (var item in marketManager.GetUniversalItems())
            {
                stringBuilder.AppendLine($"{index}. {item.FriendlyName}: ${item.FriendlyValue}");
                index++;
            }

            Context.Respond(stringBuilder.ToString());
        }

        [Command("buy", "<itemNameOrIndex> <quantity>: Purchases a quantity of items from nearby tradezones at the lowest prices available.")]
        public void Buy(string itemNameOrIndex, decimal quantity)
        {
//            var stringBuilder = new StringBuilder("Available Items: ");
            var marketManager = EconomyPlugin.GetManager<MarketSimManager>();

            var marketItem = default(MarketValueItem);
            if (int.TryParse(itemNameOrIndex, out var index))
            {
                // Find the item by tradezone index.
            }
            else
            {
                marketItem = marketManager.GetUniversalMarketValueItem(itemNameOrIndex);
            }

            if (!IsValidQuantity(marketItem, quantity))
                return;
            if (quantity <= 0)
            {
                Context.Respond("Invalid quantity, or you dont have any to trade!");
                return;
            }

            var buyingPlayer = Context.Player;
            var buyingCharacter = buyingPlayer.Character;
            // TODO: do players in Cryochambers count as a valid trading partner? They should be alive, but the connected player may be offline.
            // I think we'll have to do lower level checks to see if a physical player is Online.
            if (buyingCharacter == null)
            {
                // Player has no body. Could mean they are dead.
                // Either way, there is no inventory.
                Context.Respond( "You are dead. You cannot trade while dead.");
//                EconomyScript.Instance.ServerLogger.WriteVerbose("Action /Sell Create aborted by Steam Id '{0}' -- player is dead.", SenderSteamId);
                return;
            }
            
            var playerInventory = buyingCharacter.GetInventory();
            Support.InventoryAdd(playerInventory, (VRage.MyFixedPoint)quantity, marketItem.Definition.Id);
        }

        [Command("sell", "<itemName> <quantity>: Attempts to sell a quantity of items to nearby trade zones at best possible price from character inventory and ship inventory.")]
        public void Sell(string itemName, decimal quantity)
        {

        }

        private bool IsValidQuantity(MarketValueItem marketItem, decimal quantity)
        {
            if (marketItem.Definition.Id.TypeId != typeof(MyObjectBuilder_Ore) && marketItem.Definition.Id.TypeId != typeof(MyObjectBuilder_Ingot))
            {
                if (quantity != Math.Truncate(quantity))
                {
                    Context.Respond("You must provide a whole number for the quantity of that item.");
                    return false;
                }
            }

            return true;
        }
    }
}
