using TorchEconomy.Data;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Managers;
using TorchEconomy.ShipTrading.Managers.Generators;

namespace TorchEconomy.ShipTrading.Managers
{
	public class ShipOrderManager : BaseManager
	{
		public ShipOrderManager(IConnectionFactory connectionFactory) : base(connectionFactory)
		{
		}

		public override void Awake()
		{
			base.Start();

			var simulationManager = EconomyPlugin.GetManager<MarketSimulationManager>();
			simulationManager.RegisterGenerator(new ShipyardOrderGenerator());
		}
	}
}