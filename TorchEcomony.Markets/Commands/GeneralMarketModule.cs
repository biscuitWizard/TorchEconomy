using System.Linq;
using System.Text;
using Torch.Commands;
using Torch.Commands.Permissions;
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
            var simManager = GetManager<MarketSimManager>();

            var responseBuilder = new StringBuilder("Global Value List:");
            responseBuilder.AppendLine();

            var index = 1;
            var items = simManager.GetUniversalItems().ToArray();
            foreach (var item in items)
            {
                
                responseBuilder.AppendLine($"{index.ToString().PadLeft(4)}. {item.FriendlyName.PadLeft(40)}: {Utilities.FriendlyFormatCurrency(item.Value)}");
                index++;
            }

            responseBuilder.AppendLine($"{items.Length} Total Items");
            Context.Respond(responseBuilder.ToString());
        }
    }
}