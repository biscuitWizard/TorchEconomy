using System.Collections.ObjectModel;
using Torch;
using TorchEconomy.ShipTrading.Data.Models;

namespace TorchEconomy.ShipTrading
{
	public class EconomyShipTradingConfig : ViewModel
	{
		public class ShipOrderBlueprint
		{
			public string DefinitionId { get; set; }
			public string DisplayName { get; set; }
			public decimal Value { get; set; }
		}
		
		private ObservableCollection<ShipOrderBlueprint> _sellableShips = new ObservableCollection<ShipOrderBlueprint>();
		public ObservableCollection<ShipOrderBlueprint> SellableShips
		{
			get => _sellableShips;
			set => SetValue(ref _sellableShips, value);
		} 
	}
}