using TorchEconomy.Data;
using TorchEconomy.Managers;

namespace TorchEconomy.ShipTrading.Managers
{
	public class ShipOrderManager : BaseManager
	{
		public ShipOrderManager(IConnectionFactory connectionFactory) : base(connectionFactory)
		{
		}

		public override void Start()
		{
			base.Start();
		}
	}
}