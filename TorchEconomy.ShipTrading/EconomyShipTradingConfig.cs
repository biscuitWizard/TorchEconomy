using System.Collections.ObjectModel;
using Torch;
using TorchEconomy.ShipTrading.Data.Models;

namespace TorchEconomy.ShipTrading
{
	public class EconomyShipTradingConfig : ViewModel
	{
		private ObservableCollection<ShipOrder> _sellableShips = new ObservableCollection<ShipOrder>();
		public ObservableCollection<ShipOrder> SellableShips
		{
			get => _sellableShips;
			set => SetValue(ref _sellableShips, value);
		} 
	}
}