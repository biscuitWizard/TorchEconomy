using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace TorchEconomy.Commands
{
    [Category("admin")]
    public class AdminModule : CommandModule
    {
        [Command("zone create", "<name>: Creates a new zone at current position.")]
        [Permission(MyPromoteLevel.Admin)]
        public void CreateZone(string name)
        {

        }
    }
}
