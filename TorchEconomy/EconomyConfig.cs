using System;
using System.Windows;
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
            MySQLConnectionString = "Server=localhost;Database=space_engineers;Uid=root;Pwd=password;";
            SQLiteConnectionString = "Data Source=" + SqliteConnectionFactory.DbPath;
            SqlLite = true;
            ConnectionStringEnabled = false;
            TransactionKey = Guid.NewGuid().ToString();
            ForceTransactionCheck = true;
            MaxPlayerAccounts = 10;
        }

        //[Display(Name = "Database Connection String", GroupName = "Database Settings", Order = 0, Description = "Set backend server connection to store economy data for players.")]
        private string _mySQLConnectionString;
        public string MySQLConnectionString { get => _mySQLConnectionString; set => SetValue(ref _mySQLConnectionString, value); }
        public Visibility MySQLConnectionStringVisibility
        {
            get => _mysql ? Visibility.Visible : Visibility.Collapsed;
        }
        
        private string _sqliteConnectionString;
        public string SQLiteConnectionString { get => _sqliteConnectionString; set => SetValue(ref _sqliteConnectionString, value); }
        public Visibility SQLiteConnectionStringVisibility
        {
            get => _sqlite ? Visibility.Visible : Visibility.Collapsed;
        }
        
        private bool _connectionStringEnabled;
        public bool ConnectionStringEnabled
        {
            get => _connectionStringEnabled;
            set => SetValue(ref _connectionStringEnabled, value);
        }
        
        private bool _sqlite;
        public bool SqlLite
        {
            get => _sqlite;
            set
            {
                SetValue(ref _sqlite, value);
                ConnectionStringEnabled = !value;
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
