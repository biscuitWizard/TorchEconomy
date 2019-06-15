using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using NLog;
using Sandbox.Game.Entities;
using Torch.Commands;
using TorchEconomy;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Managers;
using VRage.Game;
using VRage.ModAPI;

namespace TorchEconomy.Markets.Commands
{
    [Category("econ markets")]
    public class MarketModule : EconomyCommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.TradeZone");
//
//        [Command("zones", "Lists all trade zones player is currently in.")]
//        public void GetZones()
//        {
//            var tradeZoneManager = EconomyPlugin.GetManager<TradeZoneManager>();
//
//            var playerPosition = Context.Player.Character.GetPosition();
//            var tradeZones = tradeZoneManager.GetTradeZonesInRange(playerPosition).ToArray();
//            if (tradeZones.Length == 0)
//            {
//                Context.Respond("There are no trade zones in range of you.");
//                return;
//            }
//            
//            var stringBuilder = new StringBuilder("Nearby Trade Zones:");
//            foreach (var tradeZone in tradeZones)
//            {
//                stringBuilder.AppendLine($"[{tradeZone.Name}] {tradeZone.Position.DistanceFrom(playerPosition)}m");
//            }
//
//            Context.Respond(stringBuilder.ToString());
//        }

        [Command("create", "<stationGridName> <newMarketName>: Creates a market using the specified station grid and names it based on the new market name.")]
        public void CreateMarket(string stationGridName, string marketName)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You are dead. You cant trade ships while dead.");
                return;
            }

            MyCubeGrid stationEntity = null;
            if (!Utilities.TryGetEntityByNameOrId(stationGridName, out IMyEntity entity))
            {
                Context.Respond($"Unable to find a station by the name of '{stationGridName}'.");
                return;
            }
            
            stationEntity = entity as MyCubeGrid;
            if (stationEntity == null
                || !stationEntity.IsStatic)
            {
                Context.Respond($"Unable to find a station by the name of '{stationGridName}'.");
                return;
            }

            var manager = EconomyPlugin.GetManager<MarketManager>();
            
            // Check we have ownership.
            if (!stationEntity.BigOwners.Contains(Context.Player.IdentityId))
            {
                Context.Respond("You must own a majority of the grid to be able authorize a station as a market.");
                return;
            }
            
            // Market checks to see if the station is already registered.
            manager.GetMarkets()
                .Then(markets =>
                {
                    Log.Info("Received markets");
                    if (markets.Any(m => m.ParentGridId == entity.EntityId))
                    {
                        Context.Respond("This station is already marked as a market.");
                        return;
                    }

                    manager.CreateMarket(entity.EntityId, Context.Player.SteamUserId, marketName,
                            EconomyMarketsPlugin.Instance.Config.DefaultMarketRange)
                        .Then(newMarket => { Context.Respond($"{marketName} has been successfully established."); })
                        .Catch(error => { Context.Respond($"[ERROR] Unable to create market: {error.Message}"); });
                }).Catch(error => Log.Error(error));
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
