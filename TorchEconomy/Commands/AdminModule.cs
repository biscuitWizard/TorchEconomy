using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using TorchEconomy.Managers;
using VRage.Game.ModAPI;

namespace TorchEconomy.Commands
{
	[Category("admin accounts")]
	public class AdminModule : EconomyCommandModule
	{
		[Command("give", "<playerNameOrId> <amount>: Gives player that amount of currency.")]
		[Permission(MyPromoteLevel.Admin)]
		public void GiveCurrency(string playerNameOrId, decimal amount)
		{
			ulong.TryParse(playerNameOrId, out var toPlayerId);
			var player = Utilities.GetPlayerByNameOrId(playerNameOrId);
			toPlayerId = player?.SteamUserId ?? toPlayerId;

			if (toPlayerId == 0)
			{
				Context.Respond($"Player '{playerNameOrId}' not found or ID is invalid.");
				return;
			}

			var manager = GetManager<AccountsManager>();
			manager
				.GetPrimaryAccount(toPlayerId)
				.Then(result =>
				{
					manager.AdjustAccountBalance(result.Id, amount);

					Context.Respond($"Gave {player?.DisplayName} ${Utilities.FriendlyFormatCurrency(amount)}.");
					SendMessage(toPlayerId, $"You have been sent {Utilities.FriendlyFormatCurrency(amount)} by SYSTEM.");
				});
		}
	}
}