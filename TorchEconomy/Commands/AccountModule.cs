using System;
using System.Linq;
using System.Text;
using Dapper;
using NLog;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Mod;
using Torch.Mod.Messages;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Managers;
using VRage.Game;

namespace TorchEconomy.Commands
{
    [Category("econ")]
    public class AccountModule : EconomyCommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.Account");

        [Command("accounts primary")]
        public void SetPrimaryAccount(string accountIdString)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            manager.GetAccount(playerId, accountIdString)
                .Then(account =>
                {
                    manager.SetAccountAsPrimary(account.Id);
                    Context.Respond($"Acct#{account.Id} has been set as your primary account.");
                })
                .Catch(error => { Context.Respond(error.Message); });
        }

        [Command("accounts close")]
        public void CloseAccount(string accountIdString)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            manager.GetAccount(playerId, accountIdString)
                .Then(account =>
                {
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
        public void OpenAccount(string accountNickname)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            manager.GetAccounts(playerId)
                .Then(accounts =>
                {
                    if (accounts.Count() >= Config.MaxPlayerAccounts)
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
                        responseBuilder.AppendLine($"+ Acct#{account.Id} [PRIMARY]: ${Utilities.FriendlyFormatCurrency(account.Balance)}");
                    else
                        responseBuilder.AppendLine($"+ Acct#{account.Id}: {Utilities.FriendlyFormatCurrency(account.Balance)}");
                }

                responseBuilder.AppendLine($"Accounts Total: {Utilities.FriendlyFormatCurrency(accounts.Sum(a => a.Balance))}");
            
                SendMessage(Context.Player.SteamUserId, responseBuilder.ToString());
            });
        }

        [Command("give", "Alias for /transfer <target> <amount>")]
        public void GiveBalance(string target, decimal amount) { TransferBalance(target, amount); }
        [Command("pay", "Alias for /transfer <target> <amount>")]
        public void PayBalance(string target, decimal amount) { TransferBalance(target, amount); }
        [Command("transfer", "Transfers currency to the target player.")]        
        public void TransferBalance(string targetPlayerNameOrId, decimal amount)
        {
            var fromPlayerId = Context.Player.SteamUserId;
            var manager = GetManager<AccountsManager>();
            
            ulong.TryParse(targetPlayerNameOrId, out var toPlayerId);
            toPlayerId = Utilities.GetPlayerByNameOrId(targetPlayerNameOrId)?.SteamUserId ?? toPlayerId;

            if (toPlayerId == 0)
            {
                Context.Respond($"Player '{targetPlayerNameOrId}' not found or ID is invalid.");
                return;
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
