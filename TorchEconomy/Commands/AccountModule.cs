using System;
using System.Linq;
using System.Text;
using Dapper;
using NLog;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Managers;
using VRage.Game;
using VRage.Game.ModAPI;

namespace TorchEconomy.Commands
{
    [Category("econ")]
    public class AccountModule : EconomyCommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.Account");

        [Command("accounts primary")]
        [Permission(MyPromoteLevel.None)]
        public void SetPrimaryAccount(string accountIdString)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            manager.GetAccount(playerId, accountIdString)
                .Then(account =>
                {
                    manager.SetAccountAsPrimary(playerId, account.Id);
                    Context.Respond($"Acct#{account.Id} has been set as your primary account.");
                })
                .Catch(error => { Context.Respond(error.Message); });
        }

        [Command("accounts close")]
        [Permission(MyPromoteLevel.None)]
        public void CloseAccount(string accountIdString)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            manager.GetAccount(playerId, accountIdString)
                .Then(account =>
                {
                    if (account.Nickname == "default")
                    {
                        Context.Respond("It is not possible to delete your default account.");
                        return;
                    }

                    if (account.IsPrimary)
                    {
                        Context.Respond("It is not possible to delete an account marked as primary.");
                        return;
                    }
                    
                    if (account.Balance != 0)
                    {
                        Context.Respond($"Acct#{account.Id}'s balance must be zero in order to close the account.");
                        return;
                    }

                    manager.CloseAccount(account.Id);
                    Context.Respond($"Acct#{account.Id} has successfully been closed.");
                })
                .Catch(error => { Context.Respond(error.Message); });
        }

        [Command("accounts open")]
        [Permission(MyPromoteLevel.None)]
        public void OpenAccount(string accountNickname)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            manager.GetAccounts(playerId)
                .Then(accounts =>
                {
                    if (accounts.Count() + 1 >= Config.MaxPlayerAccounts)
                    {
                        Context.Respond(
                            $"You have reached the maximum number of open accounts. It is not possible to open another account.");
                        return;
                    }

                    if (accounts.Any(a =>
                        a.Nickname.Equals(accountNickname, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Context.Respond(
                            $"You may not have two accounts with the same nickname. Please choose a unique nickname.");
                        return;
                    }

                    manager.CreateAccount(playerId, 0, accountNickname, false)
                        .Then(newAccount =>
                        {
                            Context.Respond(
                                $"Acct#{newAccount.Id}[{accountNickname}] has been created successfully.");
                        });
                });
        }
        
        [Command("logs", "<account id>: Lists the last 50 transactions to happen on the specified account.")]
        [Permission(MyPromoteLevel.None)]
        public void TransactionLog(string accountIdString)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            manager.GetAccount(playerId, accountIdString)
                .Then(account =>
                {
                    var accountId = account.Id;
                    
                    manager.GetTransactions(accountId).Then(logs =>
                    {
                        var responseBuilder = new StringBuilder($"Transaction Log for Acct#{accountId}:");
                        responseBuilder.AppendLine();

                        foreach (var log in logs.Take(50))
                        {
                            var operation = log.ToAccountId == accountId ? "[RECV]" : "[SEND]";
                            var to = log.ToAccountId == AccountsManager.SystemAccountId 
                                ? "SYSTEM" : log.ToAccountId.ToString();
                            var from = log.FromAccountId == AccountsManager.SystemAccountId
                                ? "SYSTEM"
                                : log.FromAccountId.ToString();
                        
                            responseBuilder.AppendLine(
                                $"{operation} {from}==>{to} [{Utilities.FriendlyFormatCurrency(log.TransactionAmount)}]: {log.Reason}");
                        }
                    
                        SendMessage(Context.Player.SteamUserId, responseBuilder.ToString());
                    });
                })
                .Catch(error =>
                {
                    Context.Respond(error.Message);
                });
        }
        
        [Command("balance", "Lists current bank account balance.")]
        [Permission(MyPromoteLevel.None)]
        public void Balance()
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;
            Log.Info(playerId + " is requesting balance");

            var responseBuilder = new StringBuilder("Your Accounts:");
            responseBuilder.AppendLine();

            manager.GetAccounts(playerId).Then(result =>
            {
                var accounts = result.ToArray();
                
                foreach (var account in accounts)
                {
                    if (account.IsPrimary)
                        responseBuilder.AppendLine($"+ Acct#{account.Id} ({account.Nickname}) [PRIMARY]: ${Utilities.FriendlyFormatCurrency(account.Balance)}");
                    else
                        responseBuilder.AppendLine($"+ Acct#{account.Id} ({account.Nickname}): {Utilities.FriendlyFormatCurrency(account.Balance)}");
                }

                responseBuilder.AppendLine($"Accounts Total: {Utilities.FriendlyFormatCurrency(accounts.Sum(a => a.Balance))}");
            
                SendMessage(Context.Player.SteamUserId, responseBuilder.ToString());
            });
        }

        [Command("move", "<fromAccount> <toAccount> <amount>: Manually transfer money between two accounts you own.")]
        public void MoveBalance(string fromAccountNameOrId, string toAccountNameOrId, decimal amount)
        {
            var fromPlayerId = Context.Player.SteamUserId;
            var manager = GetManager<AccountsManager>();

            if (amount < new decimal(0.01))
            {
                Context.Respond($"'{amount}' is too small to transfer. Please choose a number higher than 0.01.");
                return;
            }

            manager
                .GetAccounts(fromPlayerId)
                .Then(rawAccounts =>
                {
                    var accounts = rawAccounts.ToArray();
                    
                    AccountDataObject fromAccount;
                    AccountDataObject toAccount;

                    if (long.TryParse(fromAccountNameOrId, out var fromAccountId))
                        fromAccount = accounts.FirstOrDefault(a => a.Id == fromAccountId);
                    fromAccount = accounts.FirstOrDefault(a => a.Nickname.Equals(fromAccountNameOrId, StringComparison.InvariantCultureIgnoreCase));

                    if (fromAccount == null)
                    {
                        Context.Respond($"Cannot find an account by the name of '{fromAccountNameOrId}'.");
                        return;
                    }

                    if (fromAccount.Balance < amount)
                    {
                        Context.Respond($"Acct#{fromAccount.Id} is short {amount - fromAccount.Balance} to make that transfer.");
                        return;
                    }
                    
                    if (long.TryParse(fromAccountNameOrId, out var toAccountId))
                        toAccount = accounts.FirstOrDefault(a => a.Id == toAccountId);
                    toAccount = accounts.FirstOrDefault(a => a.Nickname.Equals(toAccountNameOrId, StringComparison.InvariantCultureIgnoreCase));
                    
                    if (toAccount == null)
                    {
                        Context.Respond($"Cannot find an account by the name of '{toAccountNameOrId}'.");
                        return;
                    }
                    
                    manager.AdjustAccountBalance(toAccount.Id, amount, fromAccount.Id, "Self-transfer between owned accounts.");
                    SendMessage(fromPlayerId, $"Successfully transferred {Utilities.FriendlyFormatCurrency(amount)} from Acct#{fromAccount.Id} to Acct#{toAccount.Id}.");
                });
        }

        [Command("give", "Alias for !econ transfer <target> <amount>")]
        [Permission(MyPromoteLevel.None)]
        public void GiveBalance(string target, decimal amount) { TransferBalance(target, amount); }
        [Command("pay", "Alias for !econ transfer <target> <amount>")]
        [Permission(MyPromoteLevel.None)]
        public void PayBalance(string target, decimal amount) { TransferBalance(target, amount); }
        [Command("transfer", "Transfers currency to the target player.")]      
        [Permission(MyPromoteLevel.None)]
        public void TransferBalance(string targetPlayerNameOrId, decimal amount)
        {
            var fromPlayerId = Context.Player.SteamUserId;
            var manager = GetManager<AccountsManager>();

            if (amount < new decimal(0.01))
            {
                Context.Respond($"'{amount}' is too small to transfer. Please choose a number higher than 0.01.");
                return;
            }
            
            if (!ulong.TryParse(targetPlayerNameOrId, out var toPlayerId))
            {
                var toPlayer = Utilities.GetPlayerByNameOrId(targetPlayerNameOrId);
                if (toPlayer == null)
                {
                    Context.Respond($"Player '{targetPlayerNameOrId}' not found or ID is invalid.");
                    return;
                }
                toPlayerId = toPlayer.SteamUserId;
            }

            if(toPlayerId == fromPlayerId)
            {
                Context.Respond($"Cannot send {EconomyPlugin.Instance.Config.CurrencyName} to yourself.");
                return;
            }

            manager.GetPrimaryAccount(fromPlayerId)
                .Then(fromAccount =>
                {
                    if (fromAccount.Balance - amount < 0)
                    {
                        Context.Respond(
                            $"Lacking {Utilities.FriendlyFormatCurrency(amount - fromAccount.Balance)} to complete that transfer.");
                        return;
                    }

                    manager.GetPrimaryAccount(toPlayerId)
                        .Then(toAccount =>
                        {
                            if (toAccount == null)
                            {
                                Context.Respond($"Unable to find {targetPlayerNameOrId}.");
                                return;
                            }
                            
                            manager.AdjustAccountBalance(toAccount.Id, amount, fromAccount.Id, 
                                $"{Context.Player.DisplayName} is transferring {amount}.");
                           
                            SendMessage(toPlayerId, $"You have been sent {Utilities.FriendlyFormatCurrency(amount)} by {Context.Player.DisplayName}.");
                            SendMessage(fromPlayerId, $"You have sent {Utilities.FriendlyFormatCurrency(amount)} to {targetPlayerNameOrId}.");
                        });
                });
        }
    }
}
