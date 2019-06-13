using System.Collections.Generic;
using Dapper;
using NLog.Fluent;
using Torch.API;
using Torch.API.Managers;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;

namespace TorchEconomy.Managers
{
    public class AccountsManager : BaseManager
    {
        private readonly IMultiplayerManagerServer _multiplayerManager;

        public AccountsManager(IConnectionFactory connectionFactory, IMultiplayerManagerServer multiplayerManager) 
            : base(connectionFactory)
        {
            _multiplayerManager = multiplayerManager;
        }

        public override void Start()
        {
            base.Start();

            _multiplayerManager.PlayerJoined += PlayerJoined;
        }

        public AccountDataObject GetPrimaryAccount(ulong playerId)
        {
            using (var connection = ConnectionFactory.Open())
            {
                return connection.QueryFirst<AccountDataObject>(
                    SQL.SELECT_PRIMARY_ACCOUNT, 
                    new {playerId = playerId});
            }
        }

        public AccountDataObject GetAccount(ulong accountId)
        {
            using (var connection = ConnectionFactory.Open())
            {
                return connection.QueryFirst<AccountDataObject>(
                    SQL.SELECT_ACCOUNT,
                    new {id = accountId});
            }
        }

        public IEnumerable<AccountDataObject> GetAccounts(ulong playerId)
        {
            using (var connection = ConnectionFactory.Open())
            {
                return connection.Query<AccountDataObject>(
                    SQL.SELECT_ACCOUNTS,
                    new {playerId = playerId});
            }
        }

        public decimal GetAccountBalance(ulong accountId)
        {
            return GetAccount(accountId).Balance;
        }

        public decimal GetPrimaryAccountBalance(ulong playerId)
        {
            return GetPrimaryAccount(playerId).Balance;
        }

        public void AdjustAccountBalance(ulong accountId, decimal amount)
        {
            
        }
        
        private void PlayerJoined(IPlayer player)
        {
            using(var connection = ConnectionFactory.Open())
            {
                var playerAccount = connection.QueryFirstOrDefault<AccountDataObject>(
                    SQL.SELECT_ACCOUNTS, 
                    new { playerId = player.SteamId });

                if(playerAccount == null)
                {
                    Log.Info("Creating Bank Account for Player# " + player.SteamId);
                    Log.Info(string.Format("Allocating {0} to Player# {1}", Config.StartingFunds, player.SteamId));

                    // Create a new account with the starting money defined in config.
                    connection.Execute(
                        SQL.INSERT_ACCOUNT,
                        new { playerId = player.SteamId, balance = Config.StartingFunds, isNPC = false, IsPrimary = true });
                }
            }
        }
    }
}
