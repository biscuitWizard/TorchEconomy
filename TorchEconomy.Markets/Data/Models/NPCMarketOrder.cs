using VRage.Game;

namespace TorchEconomy.Markets.Data.Models
{
	public struct NPCMarketOrder
	{
		public MyDefinitionBase Definition { get; set; }
		public long MarketId { get; set; }
		public long OrderId { get; set; }
		public decimal DesiredStock { get; set; }
		public decimal MarginFlux { get; set; }
		public decimal DemandMultiplier { get; set; }
	}
}