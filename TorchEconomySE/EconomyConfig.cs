using Torch;
using Torch.Views;

namespace TorchEconomySE
{
    public class EconomyConfig : ViewModel
    {
        public EconomyConfig()
        {
            _startingFunds = 2000000;
            _currencyName = "KeenBucks";
            _currencyName = "KB";
            _energySecondsValue = 10;
        }

        //[Display(Name = "Database Connection String", GroupName = "Database Settings", Order = 0, Description = "Set backend server connection to store economy data for players.")]
        private string _connectionString;
        public string ConnectionString { get => _connectionString; set => SetValue(ref _connectionString, value); }

        private bool _sqlLite;
        public bool SqlLite { get => _sqlLite; set => SetValue(ref _sqlLite, value); }

        private bool _mysql;
        public bool MySQL { get => _mysql; set => SetValue(ref _mysql, value); }

        public decimal _startingFunds;
        public decimal StartingFunds { get => _startingFunds; set => SetValue(ref _startingFunds, value); }

        public string _currencyName;
        public string CurrencyName { get => _currencyName; set => SetValue(ref _currencyName, value); }

        public string _currencyAbbreviation;
        public string CurrencyAbbreviation { get => _currencyAbbreviation; set => SetValue(ref _currencyAbbreviation, value); }

        public decimal _energySecondsValue;
        public decimal EnergySecondsValue
        {
            get => _energySecondsValue;
            set => SetValue(ref _energySecondsValue, value);
        }
    }
}
