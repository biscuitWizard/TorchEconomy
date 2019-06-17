using System;
using System.Linq;
using NLog;
using Sandbox.Game.Entities;
using Torch.Commands;
using Torch.Commands.Permissions;
using TorchEconomy.Data.Types;
using TorchEconomy.Managers;
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
    }
}
