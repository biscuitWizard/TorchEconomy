using System;
using System.Linq;
using System.Text;
using NLog;
using Sandbox.Definitions;
using Torch.Commands;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Data.Types;
using TorchEconomy.Markets.Managers;
using VRage.Game;
using VRage.Game.ModAPI;

namespace TorchEconomy.Markets.Commands
{
    [Category("econ")]
    public class MarketModule : EconomyCommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.Markets");
        
        [Command("list", "Lists available goods to buy.")]
        public void List()
        {
            var marketManager = EconomyPlugin.GetManager<MarketManager>();
            var marketOrderManager = EconomyPlugin.GetManager<MarketOrderManager>();
            var marketSimManager = EconomyPlugin.GetManager<MarketSimManager>();
            
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do that while dead.");
                return;
            }
            
            var controllingCube = Context.Player.Controller.ControlledEntity as IMyCubeBlock;
            if (controllingCube == null)
            {
                Context.Respond("Trading by hand is not supported.");
                return;
            }

            marketManager.GetConnectedMarket(controllingCube.CubeGrid)
                .Then(market =>
                {
                    if (market == null)
                    {
                        Context.Respond("Unable to find any connected markets. Have you docked to a market?");
                        return;
                    }

                    marketOrderManager
                        .GetMarketOrders(market.Id)
                        .Then(orders =>
                        {
                            var responseBuilder = new StringBuilder($"Mrkt#{market.Id} ({market.Name}) Inventory:");
                            responseBuilder.AppendLine();

                            var maxNameLength = 30;
                            
                            responseBuilder.AppendLine("+-- Buy Orders:");
                            var buyOrders = orders.Where(o => o.OrderType == BuyOrderType.Buy).ToArray();
                            if (buyOrders.Length == 0)
                                responseBuilder.AppendLine("None");
                            else
                            {
                                foreach (var order in buyOrders)
                                {
                                    var definition = MyDefinitionManager.Static.GetDefinition(order.MyDefinitionId);
                                    var orderQuantity = (order.Quantity + "x").PadLeft(6);
                                    var valueDifference =
                                        marketSimManager.GetOrCalculateUniversalItemValue(order.MyDefinitionId);
                                    responseBuilder.AppendLine(
                                        $"+ {definition.DisplayNameText.PadRight(maxNameLength)} {orderQuantity}: {Utilities.FriendlyFormatCurrency(order.Price)} ({Utilities.FriendlyFormatCurrency(valueDifference)})");
                                }
                            }

                            responseBuilder.AppendLine("+-- Sell Orders:");
                            var sellOrders = orders.Where(o => o.OrderType == BuyOrderType.Sell).ToArray();
                            if (sellOrders.Length == 0)
                                responseBuilder.AppendLine("None");
                            else
                            {
                                foreach (var order in sellOrders)
                                {
                                    var definition = MyDefinitionManager.Static.GetDefinition(order.MyDefinitionId);
                                    var orderQuantity = (order.Quantity + "x").PadLeft(6);
                                    var valueDifference =
                                        marketSimManager.GetOrCalculateUniversalItemValue(order.MyDefinitionId);
                                    responseBuilder.AppendLine(
                                        $"+ {definition.DisplayNameText.PadRight(maxNameLength)} {orderQuantity}: {Utilities.FriendlyFormatCurrency(order.Price)} ({Utilities.FriendlyFormatCurrency(valueDifference)})");
                                }
                            }

                            responseBuilder.AppendLine($"Total Orders: {orders.Length}");
                            
                            Context.Respond(responseBuilder.ToString());
                        })
                        .Catch(error => Log.Error(error));
                })
                .Catch(error => Log.Error(error));
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