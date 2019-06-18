using System;
using System.Linq;
using NLog;
using Sandbox.Game.Entities;
using Torch.Commands;
using Torch.Commands.Permissions;
using TorchEconomy.Data.Types;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Data.DataObjects;
using TorchEconomy.Markets.Data.Types;
using TorchEconomy.Markets.Managers;
using TorchEconomy.Resources;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace TorchEconomy.Commands
{
    [Category("admin markets")]
    public class AdminModule : EconomyCommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.Admin");

        [Command("delete", "<marketName>: Deletes a market from the game.")]
        [Permission(MyPromoteLevel.Admin)]
        public void DeleteMarket(string marketNameOrId)
        {
            var manager = EconomyPlugin.GetManager<MarketManager>();
            manager.GetMarkets()
                .Then(markets =>
                {
                    MarketDataObject market = null;
                    if (long.TryParse(marketNameOrId, out var marketId))
                    {
                        market = markets.FirstOrDefault(m => m.Id == marketId);
                    }
                    else
                    {
                        market = markets.FirstOrDefault(m =>
                            m.Name.Equals(marketNameOrId, StringComparison.InvariantCultureIgnoreCase));
                    }

                    if (market == null)
                    {
                        Context.Respond($"Unable to find market by name or id of '{marketNameOrId}'.");
                        return;
                    }

                    manager.DeleteMarket(market.Id)
                        .Then(() => { Context.Respond($"Successfully deleted Mrkt#{market.Id}."); });
                }).Catch(HandleError);
        }
        
        [Command("createNPC", "<gridName> <provider>: Creates an NPC market of the specified industry type at the provider grid.")]
        [Permission(MyPromoteLevel.Admin)]
        public void CreateNPCMarket(string stationGridName, string industryName)
        {
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

            if (!Enum.TryParse(industryName, out IndustryTypeEnum industryType))
            {
                var types = string.Join(", ", Enum.GetNames(typeof(IndustryTypeEnum)));
                Context.Respond($"Unable to find industry type '{industryName}'. Valid industry types are: {types}.");
                return;
            }
            
            var marketManager = GetManager<MarketManager>();
            
            // Market checks to see if the station is already registered.
            marketManager.GetMarkets()
                .Then(markets =>
                {
                    Log.Info("Received markets");
                    if (markets.Any(m => m.ParentGridId == entity.EntityId))
                    {
                        Context.Respond("This station is already marked as a market.");
                        return;
                    }

                    var npcManager = EconomyPlugin.GetManager<NPCManager>();
                    var npcName = NameGeneratorHelper.GetName();
                    npcManager.CreateNPC(npcName, industryType)
                        .Then(npc =>
                        {
                            // NPC is now created.. Need to make them a bank account.
                            var accountsManager = GetManager<AccountsManager>();

                            accountsManager.CreateAccount((ulong)npc.Id, 0, "default", true)
                                .Then(npcAccount =>
                                {
                                    // Now they have a bank account.. Time to make a station market.
                                    var marketName = NameGeneratorHelper.GetIndustryName(industryType);

                                    marketManager.CreateMarket(stationEntity.EntityId, (ulong) npc.Id, marketName, 
                                            3000, npcAccount.Id, true, true)
                                        .Then(market =>
                                        {
                                            // Market is created.. Now to create buy orders.
                                            var simManager = GetManager<MarketSimulationManager>();
                                            simManager.GenerateNPCOrders(npc, market)
                                                .Then(() =>
                                                {
                                                    stationEntity.DisplayName = market.Name;
                                                    Context.Respond($"{npc.Name} has founded {market.Name}, specializing in {industryType} trade.");
                                                })
                                                .Catch(HandleError);
                                        })
                                        .Catch(HandleError);
                                })
                                .Catch(HandleError);
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }
        
        [Command("buy.quantity", "<marketNameOrId> <itemName> <newQuantity>: Overrides a buy order quantity on a market.")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetBuyOrderQuantity(string marketNameOrId, string itemName, decimal newQuantity)
        {
            UpdateOrderQuantity(BuyOrderType.Buy, marketNameOrId, itemName, newQuantity);
        }

        [Command("sell.quantity", "<marketNameOrId> <itemName> <newQuantity>: Overrides a sell order quantity on a market.")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetSellOrderQuantity(string marketNameOrId, string itemName, decimal newQuantity)
        {
            UpdateOrderQuantity(BuyOrderType.Sell, marketNameOrId, itemName, newQuantity);
        }
        
        [Command("buy.price", "<marketNameOrId> <itemName> <newQuantity>: Overrides a buy order price on a market.")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetBuyOrderPrice(string marketNameOrId, string itemName, decimal newPrice)
        {
            UpdateOrderPrice(BuyOrderType.Buy, marketNameOrId, itemName, newPrice);
        }

        [Command("sell.price", "<marketNameOrId> <itemName> <newQuantity>: Overrides a sell order price on a market.")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetSellOrderPrice(string marketNameOrId, string itemName, decimal newPrice)
        {
            UpdateOrderPrice(BuyOrderType.Sell, marketNameOrId, itemName, newPrice);
        }
        
        private void UpdateOrderQuantity(BuyOrderType orderType, string marketNameOrId, string itemName, decimal newQuantity)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
            
            if (!DefinitionResolver.TryGetDefinitionByName(itemName, out var itemDefinition))
            {
                Context.Respond($"Unable to find an item by the name of {itemName}");
                return;
            }

            if (newQuantity < new decimal(0.01))
            {
                Context.Respond($"{newQuantity} is too small. Please use a higher value.");
                return;
            }
            
            var manager = EconomyPlugin.GetManager<MarketManager>();
            var orderManager = EconomyPlugin.GetManager<MarketOrderManager>();
            manager.GetMarkets()
                .Then(markets =>
                {
                    MarketDataObject market = null;
                    if (long.TryParse(marketNameOrId, out var marketId))
                    {
                        market = markets.FirstOrDefault(m => m.Id == marketId);
                    }
                    else
                    {
                        market = markets.FirstOrDefault(m =>
                            m.Name.Equals(marketNameOrId, StringComparison.InvariantCultureIgnoreCase));
                    }
                    
                    if (market == null)
                    {
                        Context.Respond($"Unable to find market by name or id of '{marketNameOrId}'.");
                        return;
                    }
                    
                    orderManager.GetMarketOrder(orderType, market.Id, itemDefinition.Id)
                        .Then(order =>
                        {
                            orderManager.UpdateOrderQuantity(order.Id, newQuantity)
                                .Then(() => Context.Respond(
                                    $"Successfully updated Order#{order.Id}'s quantity to {newQuantity}."))
                                .Catch(HandleError);
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }
        
        private void UpdateOrderPrice(BuyOrderType orderType, string marketNameOrId, string itemName, decimal newPrice)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
            
            if (!DefinitionResolver.TryGetDefinitionByName(itemName, out var itemDefinition))
            {
                Context.Respond($"Unable to find an item by the name of {itemName}");
                return;
            }

            if (newPrice < new decimal(0.01))
            {
                Context.Respond($"{newPrice} is too small. Please use a higher value.");
                return;
            }
            
            var manager = EconomyPlugin.GetManager<MarketManager>();
            var orderManager = EconomyPlugin.GetManager<MarketOrderManager>();
            manager.GetMarkets()
                .Then(markets =>
                {
                    MarketDataObject market = null;
                    if (long.TryParse(marketNameOrId, out var marketId))
                    {
                        market = markets.FirstOrDefault(m => m.Id == marketId);
                    }
                    else
                    {
                        market = markets.FirstOrDefault(m =>
                            m.Name.Equals(marketNameOrId, StringComparison.InvariantCultureIgnoreCase));
                    }
                    
                    if (market == null)
                    {
                        Context.Respond($"Unable to find market by name or id of '{marketNameOrId}'.");
                        return;
                    }
                    
                    orderManager.GetMarketOrder(orderType, market.Id, itemDefinition.Id)
                        .Then(order =>
                        {
                            orderManager.UpdateOrderPrice(order.Id, newPrice)
                                .Then(() => Context.Respond(
                                    $"Successfully updated Order#{order.Id}'s price to {Utilities.FriendlyFormatCurrency(newPrice)}."))
                                .Catch(HandleError);
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }
    }
}
