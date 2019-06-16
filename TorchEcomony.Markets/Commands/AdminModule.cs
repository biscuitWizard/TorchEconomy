using System.Linq;
using NLog;
using Sandbox.Game.Entities;
using Torch.Commands;
using Torch.Commands.Permissions;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Managers;
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
        public void CreateNPCMarket(string stationGridName)
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
            
            var marketManager = EconomyPlugin.GetManager<MarketManager>();
            
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
                    
                })
                .Catch(HandleError);
        }
    }
}
