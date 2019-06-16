using TorchEconomy.Data;
using TorchEconomy.Managers;

namespace TorchEconomy.Markets.Managers
{
	public abstract class BaseMarketManager : BaseManager
	{
		protected BaseMarketManager(IConnectionFactory connectionFactory) : base(connectionFactory)
		{
		}

		protected EconomyMarketConfig MarketConfig => EconomyMarketsPlugin.Instance.Config;
	}
}