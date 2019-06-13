using Torch;

namespace TorchEconomy.Markets
{
	public class EconomyMarketConfig : ViewModel
	{
		public EconomyMarketConfig()
		{
			
			_energySecondsValue = 10;
		}
		private decimal _energySecondsValue;
		public decimal EnergySecondsValue
		{
			get => _energySecondsValue;
			set => SetValue(ref _energySecondsValue, value);
		}
	}
}