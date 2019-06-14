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

		private float _defaultMarketRange;
		public float DefaultMarketRange
		{
			get => _defaultMarketRange;
			set => SetValue(ref _defaultMarketRange, value);
		}
	}
}