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

        public Promise<AccountDataObject> CreateAccount(ulong playerId, decimal initialBalance, bool isNpc = false)
        {
            return new Promise<AccountDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    var primaryAccount = connection.QueryFirst<AccountDataObject>(
                        SQL.SELECT_PRIMARY_ACCOUNT,
                        new {playerId = playerId});

                    // Is this their first account?
                    var firstAccount = primaryAccount == null;

                    connection.Execute(
                        SQL.INSERT_ACCOUNT,
                        new
                        {
                            playerId = playerId, balance = initialBalance,
                            isNPC = isNpc, isPrimary = firstAccount
                        });
                    
                    primaryAccount = connection.QueryFirst<AccountDataObject>(
                        SQL.SELECT_PRIMARY_ACCOUNT,
                        new {playerId = playerId});
                    resolve(primaryAccount);
                }
            });
        }

        /// <summary>
        /// Gets a player's primary account.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public Promise<AccountDataObject> GetPrimaryAccount(ulong playerId)
        {
            return new Promise<AccountDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(connection.QueryFirst<AccountDataObject>(
                        SQL.SELECT_PRIMARY_ACCOUNT,
                        new {playerId = playerId}));
                }
            });
        }

        /// <summary>
        /// Gets a specific account by that account's id.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Promise<AccountDataObject> GetAccount(ulong accountId)
        {
            return new Promise<AccountDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(connection.QueryFirst<AccountDataObject>(
                        SQL.SELECT_ACCOUNT,
                        new {id = accountId}));
                }
            });
        }

        /// <summary>
        /// Gets all accounts associated with a player.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public Promise<IEnumerable<AccountDataObject>> GetAccounts(ulong playerId)
        {
            return new Promise<IEnumerable<AccountDataObject>>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(connection.Query<AccountDataObject>(
                        SQL.SELECT_ACCOUNTS,
                        new {playerId = playerId}));
                }
            });
        }

        /// <summary>
        /// Gets a player's balance from a specific account.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Promise<decimal> GetAccountBalance(ulong accountId)
        {
            return GetAccount(accountId).Then(result => result.Balance) as Promise<decimal>;
        }

        /// <summary>
        /// Gets a player's balance from their primary account.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public Promise<decimal> GetPrimaryAccountBalance(ulong playerId)
        {
            return GetPrimaryAccount(playerId).Then(result => result.Balance) as Promise<decimal>;
        }

        /// <summary>
        /// Adjusts a player's account balance and creates the associated
        /// transaction log to accompany it.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="amount"></param>
        /// <param name="optionalReason"></param>
        public void AdjustAccountBalance(ulong accountId, decimal amount, string optionalReason = null)
        {
            using (var connection = ConnectionFactory.Open())
            {
                connection.ExecuteAsync(
                    SQL.MUTATE_ACCOUNT_BALANCE,
                    new {id = accountId, amount = amount});
            }
        }

        /// <summary>
        /// Sets a player's accountID as primary, and sets a player's all other accounts as non-primary.
        /// </summary>
        /// <param name="accountId"></param>
        public void SetAccountAsPrimary(ulong accountId)
        {
            using (var connection = ConnectionFactory.Open())
            {
                connection.ExecuteAsync(
                    SQL.MUTATE_ACCOUNT_PRIMARY,
                    new {id = accountId });
            }
        }
        
        private void PlayerJoined(IPlayer player)
        {
            GetPrimaryAccount(player.SteamId).Then(result =>
            {
                if (result != null)
                {
                    // They already have an account. Ignore them.
                    return;
                }

                Log.Info("Creating Bank Account for Player# " + player.SteamId);
                Log.Info($"Allocating {Config.StartingFunds} to Player#{player.SteamId}");

                // Create a new account with the starting money defined in config.
                CreateAccount(player.SteamId, Config.StartingFunds, false);
            });
        }
    }
}
