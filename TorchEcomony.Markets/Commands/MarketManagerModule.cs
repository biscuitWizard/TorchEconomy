using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using NLog;
using Sandbox.Definitions;
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
    public class MarketManagerModule : EconomyCommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.TradeZone");

        [Command("list", "Lists all markets that you have permission to modify.")]
        public void ListOwnedMarkets()
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You are dead. Stop entering commands. Please.");
                return;
            }

            var manager = EconomyPlugin.GetManager<MarketManager>();
            manager.GetMarkets()
                .Then(markets =>
                {
                    var ownedMarkets = markets
                        .Where(m => m.CreatorId == Context.Player.SteamUserId)
                        .ToArray();
                    var responseBuilder = new StringBuilder("Markets:");
                    responseBuilder.AppendLine();

                    foreach (var ownedMarket in ownedMarkets)
                    {
                        responseBuilder.AppendLine($"+ Mrkt#{ownedMarket.Id} ({ownedMarket.Name})");
                        responseBuilder.AppendLine($"+-- Account#{ownedMarket.AccountId}, Range: {ownedMarket.Range}m");
                    }

                    responseBuilder.AppendLine($"Total Markets: {ownedMarkets.Length}");
                    
                    SendMessage(Context.Player.SteamUserId, responseBuilder.ToString());
                })
                .Catch(error => Log.Error(error));;
        }

        [Command("create", "<stationGridName> <newMarketName>: Creates a market using the specified station grid and names it based on the new market name.")]
        public void CreateMarket(string stationGridName, string marketName)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You are dead. You cannot create markets while dead.");
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
            
            // Check we have grid ownership.
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

        [Command("buy", "Creates a buy order on the specified market. Must have permission to modify market.")]
        public void SetBuyOrder(string marketNameOrId, string itemName, decimal pricePerOne, decimal quantity)
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
            
            var manager = EconomyPlugin.GetManager<MarketManager>();
        }

        [Command("sell")]
        public void SetSellOrder(string marketNameOrId, string itemName, decimal pricePerOne, decimal quantity)
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
            
            var manager = EconomyPlugin.GetManager<MarketManager>();
        }

        [Command("open", "<marketNameOrId>: Opens the specified market for business.")]
        public void OpenMarket(string marketNameOrId)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
        }

        [Command("close", "<marketNameOrId>: Closes the specified market for business.")]
        public void CloseMarket(string marketNameOrId)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
        }

        [Command("account", "<marketNameOrId> <accountNameOrId>: Links an account to specified market to act as a coffer.")]
        public void SetMarketAccount(string marketNameOrId, string accountNameOrId)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
        }

        [Command("setbuyprice", "<marketNameOrId> <itemName> <newPricePer1>: Sets a price on a specified item at the specified market.")]
        public void SetBuyOrderPrice(string marketNameOrId, string itemName, decimal newPrice)
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
            
            var manager = EconomyPlugin.GetManager<MarketManager>();
        }

        [Command("setsellprice", "<marketNameOrId> <itemName> <newPricePer1>: Sets a price on a specified item at the specified market.")]
        public void SetSellOrderPrice(string marketNameOrId, string itemName, decimal newPrice)
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
            
            var manager = EconomyPlugin.GetManager<MarketManager>();
        }
    }
}
