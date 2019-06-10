using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace TorchEconomySE.Commands
{
    [Category("admin")]
    public class AdminModule : CommandModule
    {
        [Command("zone create", "<name>: Creates a new zone at current position.")]
        [Permission(MyPromoteLevel.Admin)]
        public void CreateZone(string name)
        {

        }

        [Command("give", "<playerNameOrId> <amount>: Gives player that amount of currency.")]
        [Permission(MyPromoteLevel.Admin)]
        public void GiveCurrency(string playerNameOrId, decimal amount)
        {

        }
    }
}
