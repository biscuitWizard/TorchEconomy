namespace TorchEconomy.ShipTrading.Data.Models
{
	public class ShipOrder
	{
		/// <summary>
		/// Used if the ship is an already-spawned ship for sale.
		/// </summary>
		public string EntityId { get; set; }
		
		/// <summary>
		/// Used if the ship is a blueprint being sold by a market.
		/// </summary>
		public string BlueprintDefinitionId { get; set; }
		
		public decimal Price { get; set; }
		
		public string DisplayName { get; set; }
	}
}