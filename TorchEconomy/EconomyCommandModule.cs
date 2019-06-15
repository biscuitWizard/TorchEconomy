using Torch.API;
using Torch.API.Managers;
using Torch.Commands;
using TorchEconomy.Data;
using TorchEconomy.Managers;

namespace TorchEconomy
{
	public abstract class EconomyCommandModule : CommandModule
	{
		protected IConnectionFactory ConnectionFactory
		{
			get { return EconomyPlugin.Instance.GetConnectionFactory(); }
		}

		protected DefinitionResolver DefinitionResolver => EconomyPlugin.DefinitionResolver;

		protected ITorchBase Torch => EconomyPlugin.Instance.Torch;

		protected EconomyConfig Config => EconomyPlugin.Instance.Config;

		protected T GetManager<T>() where T : BaseManager
		{
			return EconomyPlugin.GetManager<T>();
		}

		protected void SendMessage(ulong targetSteamId, string message)
		{
			var manager = Torch
				.CurrentSession
				.Managers
				.GetManager(typeof(IChatManagerServer))
				as IChatManagerServer;

			if (manager == null)
				return;
			
			manager.SendMessageAsOther("Server", message, null, targetSteamId);
		}
	}
}