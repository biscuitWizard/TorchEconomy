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
    public class MarketManagerModule : EconomyCommandModule
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

        [Command("list")]
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
    }
}
