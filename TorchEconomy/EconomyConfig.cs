using System;
using ProtoBuf;
using Torch;
using Torch.Views;
using TorchEconomy.Data;

namespace TorchEconomy
{
    [ProtoContract]
    public class EconomyConfig : ViewModel
    {
        public const ushort ModChannelId = 39384;
        
        public EconomyConfig()
        {
            StartingFunds = 2000000;
            CurrencyName = "Credits";
            CurrencyAbbreviation = "C";
            MySQL = true;
            ConnectionStringEnabled = true;
            ConnectionString = "Server=localhost;Database=space_engineers;Uid=root;Pwd=password;";
//            SqlLite = true;
//            ConnectionStringEnabled = false;
            TransactionKey = Guid.NewGuid().ToString();
            ForceTransactionCheck = true;
            MaxPlayerAccounts = 10;
        }

        //[Display(Name = "Database Connection String", GroupName = "Database Settings", Order = 0, Description = "Set backend server connection to store economy data for players.")]
        private string _connectionString;
        public string ConnectionString { get => _connectionString; set => SetValue(ref _connectionString, value); }

        private bool _connectionStringEnabled;
        public bool ConnectionStringEnabled
        {
            get => _connectionStringEnabled;
            set => SetValue(ref _connectionStringEnabled, value);
        }
        
        private bool _sqlLite;
        public bool SqlLite
        {
            get => _sqlLite;
            set
            {
                SetValue(ref _sqlLite, value);
                ConnectionStringEnabled = !value;
                if (value)
                    ConnectionString = "Data Source=" + SqliteConnectionFactory.DbPath;
            }
        }

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

        private bool _forceTransactionCheck;
        public bool ForceTransactionCheck
        {
            get => _forceTransactionCheck;
            set => SetValue(ref _forceTransactionCheck, value);
        }

        private int _maxPlayerAccounts;
        public int MaxPlayerAccounts
        {
            get => _maxPlayerAccounts;
            set => SetValue(ref _maxPlayerAccounts, value);
        }
    }
}
