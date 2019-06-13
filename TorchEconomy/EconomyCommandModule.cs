using Torch.API;
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
		protected ITorchBase Torch
		{
			get { return EconomyPlugin.Instance.Torch; }
		}

		protected EconomyConfig Config
		{
			get { return EconomyPlugin.Instance.Config; }
		}

		protected T GetManager<T>() where T : BaseManager
		{
			return EconomyPlugin.GetManager<T>();
		}
	}
}