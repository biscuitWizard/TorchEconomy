using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NLog;
using NLog.Fluent;
using Sandbox.ModAPI;
using Torch.API;
using Torch.API.Managers;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Messages.Accounts;
using VRage;

namespace TorchEconomy.Managers
{
    public class AccountsManager : BaseManager
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Managers.Account");
        
        public const long SystemAccountId = long.MaxValue;
        
        private readonly IMultiplayerManagerServer _multiplayerManager;
        private MessageRouterManager _messageRouter;

        public AccountsManager(IConnectionFactory connectionFactory, 
            IMultiplayerManagerServer multiplayerManager) 
            : base(connectionFactory)
        {
            _multiplayerManager = multiplayerManager;
        }

        public override void Start()
        {
            base.Start();

            _multiplayerManager.PlayerJoined += PlayerJoined;
            
            _messageRouter = EconomyPlugin.GetManager<MessageRouterManager>();
            
            _messageRouter.Subscribe<AdjustBalanceMessage>(OnAdjustBalance);
            _messageRouter.Subscribe<CreateAccountMessage>(OnCreateAccount);
            _messageRouter.Subscribe<GetAccountsMessage>(OnGetAccounts);
        }

#region Messages
        private void OnGetAccounts(GetAccountsMessage message)
        {
            GetAccounts(message.PlayerId)
                .Then(accounts =>
                {
                    _messageRouter.SendResponse(new GetAccountsResponseMessage
                    {
                        Accounts = accounts.ToArray()
                    });
                });
        }

        private void OnCreateAccount(CreateAccountMessage message)
        {
            CreateAccount(message.ForPlayerId, message.InitialBalance, message.Nickname, message.IsNPC)
                .Then(newAccount =>
                {
                    _messageRouter.SendResponse(new CreateAccountResponseMessage
                    {
                        Account = newAccount
                    });
                });
        }

        private void OnAdjustBalance(AdjustBalanceMessage message)
        {
            AdjustAccountBalance(message.AccountId, message.Amount, message.FromAccountId, message.Reason);
            _messageRouter.SendResponse(new AdjustBalanceResponseMessage());
        }
#endregion

        public Promise<IEnumerable<TransactionDataObject>> GetTransactions(long accountId)
        {
            return new Promise<IEnumerable<TransactionDataObject>>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(connection.Query<TransactionDataObject>(
                        SQL.SELECT_TRANSACTIONS,
                        new { accountId = accountId }));
                }
            });
        }

        public IPromise CloseAccount(long accountId)
        {
            return new Promise((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    connection.Execute("UPDATE `Account` SET `IsDeleted`=1 WHERE Id=@id",
                        new {id = accountId});

                    resolve();
                }
            });
        }
        
        public Promise<AccountDataObject> CreateAccount(ulong playerId, decimal initialBalance, 
            string nickname = "default", bool isNpc = false)
        {
            return new Promise<AccountDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    var primaryAccount = connection.QueryFirstOrDefault<AccountDataObject>(
                        SQL.SELECT_PRIMARY_ACCOUNT,
                        new {playerId = playerId});

                    // Is this their first account?
                    var firstAccount = primaryAccount == null;

                    connection.Execute(
                        SQL.INSERT_ACCOUNT,
                        new
                        {
                            playerId = playerId, balance = initialBalance,
                            isNPC = isNpc, isPrimary = firstAccount,
                            nickname = nickname
                        });
                    
                    primaryAccount = connection.QueryFirstOrDefault<AccountDataObject>(
                        SQL.SELECT_PRIMARY_ACCOUNT,
                        new {playerId = playerId});
                    
                    // Create a transaction log.
                    connection.ExecuteAsync(
                        SQL.INSERT_TRANSACTION,
                        new
                        {
                            toAccountId = primaryAccount.Id, fromAccountId = SystemAccountId,
                            transactionAmount = initialBalance, transactedOn = DateTime.UtcNow.ToUnixTimestamp(),
                            reason = "Initial account creation credit"
                        });
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
                    resolve(connection.QueryFirstOrDefault<AccountDataObject>(
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
        public Promise<AccountDataObject> GetAccount(long accountId)
        {
            return new Promise<AccountDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(connection.QueryFirstOrDefault<AccountDataObject>(
                        SQL.SELECT_ACCOUNT,
                        new {id = accountId}));
                }
            });
        }

        /// <summary>
        /// Gets a player's account by its nickname or ID.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="accountNameOrId"></param>
        /// <returns></returns>
        public Promise<AccountDataObject> GetAccount(ulong playerId, string accountNameOrId)
        {
            return new Promise<AccountDataObject>((result, reject) =>
            {
                GetAccounts(playerId)
                    .Then(accounts =>
                    {
                        AccountDataObject account = null;
                        if (long.TryParse(accountNameOrId, out var accountId))
                        {
                            account = accounts.FirstOrDefault(a => a.Id == accountId);
                        }
                        else
                        {
                            account = accounts.FirstOrDefault(a => a.Nickname.StartsWith(accountNameOrId));
                        }

                        if (account == null)
                        {
                            reject(new Exception($"Unable to find account with a name or id of {accountNameOrId}."));
                            return;
                        }

                        result(account);
                    });
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
                        new {playerId = playerId})
                        .ToArray());
                }
            });
        }

        /// <summary>
        /// Gets a player's balance from a specific account.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Promise<decimal> GetAccountBalance(long accountId)
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
        /// <param name="fromAccountId">If this is specified, the money will also be deducted from this account id.</param>
        public void AdjustAccountBalance(long accountId, decimal amount, 
            long? fromAccountId = null, string optionalReason = null)
        {
            var transactionDate = DateTime.UtcNow.ToUnixTimestamp();
            using (var connection = ConnectionFactory.Open())
            {
                connection.ExecuteAsync(
                    SQL.MUTATE_ACCOUNT_BALANCE,
                    new {id = accountId, amount = amount});

                var sourceAccountId = fromAccountId ?? SystemAccountId;
                connection.ExecuteAsync(
                    SQL.INSERT_TRANSACTION,
                    new
                    {
                        toAccountId = accountId, fromAccountId = sourceAccountId,
                        transactionAmount = amount, transactedOn = transactionDate,
                        reason = optionalReason
                    });

                if (fromAccountId.HasValue)
                {
                    connection.ExecuteAsync(
                        SQL.MUTATE_ACCOUNT_BALANCE,
                        new {id = fromAccountId, amount = amount * -1});
                }
            }
        }

        /// <summary>
        /// Sets a player's accountID as primary, and sets a player's all other accounts as non-primary.
        /// </summary>
        /// <param name="accountId"></param>
        public void SetAccountAsPrimary(long accountId)
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

                Log.Info("Creating Bank Account for Player#" + player.SteamId);
                Log.Info($"Allocating {Config.StartingFunds} to Player#{player.SteamId}");

                // Create a new account with the starting money defined in config.
                CreateAccount(player.SteamId, Config.StartingFunds, "default", false);
            })
            .Catch(error =>
            {
                if (error != null) Log.Error(error);
            });
        }
    }
}
