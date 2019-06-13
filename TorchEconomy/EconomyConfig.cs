using ProtoBuf;
using Torch;
using Torch.Views;

namespace TorchEconomy
{
    [ProtoContract]
    public class EconomyConfig : ViewModel
    {
        public EconomyConfig()
        {
            _startingFunds = 2000000;
            _currencyName = "KeenBucks";
            _currencyName = "KB";
        }

        //[Display(Name = "Database Connection String", GroupName = "Database Settings", Order = 0, Description = "Set backend server connection to store economy data for players.")]
        private string _connectionString;
        public string ConnectionString { get => _connectionString; set => SetValue(ref _connectionString, value); }

        private bool _sqlLite;
        public bool SqlLite { get => _sqlLite; set => SetValue(ref _sqlLite, value); }

        private bool _mysql;
        public bool MySQL { get => _mysql; set => SetValue(ref _mysql, value); }

        private decimal _startingFunds;
        [ProtoMember(101)]
        public decimal StartingFunds { get => _startingFunds; set => SetValue(ref _startingFunds, value); }

        private string _currencyName;
        [ProtoMember(102)]
        public string CurrencyName { get => _currencyName; set => SetValue(ref _currencyName, value); }

        private string _currencyAbbreviation;
        [ProtoMember(103)]
        public string CurrencyAbbreviation { get => _currencyAbbreviation; set => SetValue(ref _currencyAbbreviation, value); }

        private string _transactionKey;
        public string TransactionKey
        {
            get => _transactionKey;
            set => SetValue(ref _transactionKey, value);
        }
    }
}
