using StructureMap;
using Torch;
using Torch.API.Plugins;
using TorchEconomy.Data;

namespace TorchEconomy
{
	public abstract class EconomyPluginBase : TorchPluginBase
	{
		private static IContainer _container;

		protected IContainer GetContainer()
		{
			if (_container == null)
			{
				_container = new Container();
				_container.Initialize();
			}

			return _container;
		}

		public IConnectionFactory GetConnectionFactory()
		{
			return GetContainer().GetInstance<IConnectionFactory>();
		}
	}
}