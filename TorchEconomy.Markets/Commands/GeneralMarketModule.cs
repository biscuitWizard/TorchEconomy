using System.Linq;
using System.Text;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using TorchEconomy.Markets.Data;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Managers;
using VRage.Game.ModAPI;

namespace TorchEconomy.Markets.Commands
{
    [Category("econ")]
    public class GeneralMarketModule : EconomyCommandModule
    {
        [Command("values", "Gets a list of what approximately every item is valued at.")]
        [Permission(MyPromoteLevel.None)]
        public void GlobalValueList()
        {
            var responseBuilder = new StringBuilder();
            responseBuilder.AppendLine();

            var index = 1;
            var items = GetDataProvider<MarketSimulationProvider>()
                .GetUniversalItems()
                .ToArray();
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.FriendlyName))
                    continue;
                
                responseBuilder.AppendLine($"{index.ToString().PadLeft(4)}. {item.FriendlyName.PadLeft(40)}: {Utilities.FriendlyFormatCurrency(item.Value)}");
                index++;
            }

            responseBuilder.AppendLine($"{items.Length} Total Items");
            ModCommunication.SendMessageTo(
                new DialogMessage("Global Value List", null, null, responseBuilder.ToString()),
                Context.Player.SteamUserId);
        }
    }
}