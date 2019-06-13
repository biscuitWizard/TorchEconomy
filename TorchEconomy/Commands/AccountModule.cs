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

        [Command("log", "<account id>: Lists the last 50 transactions to happen on the specified account.")]
        public void TransactionLog(string accountIdString)
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;

            if (!ulong.TryParse(accountIdString, out var accountId)
                || accountId <= 0)
            {
                Context.Respond("Invalid account ID entered. Must be a number above 0.");
                return;
            }

            manager.GetAccount(accountId).Then(account =>
            {
                if (account.PlayerId != playerId)
                {
                    Context.Respond("[ACCESS DENIED] This account does not belong to you.");
                    return;
                }

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
                            $"{operation} {to}==>{from} [${log.TransactionAmount}]: {log.Reason}");
                    }
                    
                    EconomyPlugin.Instance
                        .Torch
                        .CurrentSession?
                        .Managers?
                        .GetManager<IChatManagerServer>()?
                        .SendMessageAsOther("Server",
                            responseBuilder.ToString(),
                            MyFontEnum.Blue,
                            playerId);
                });
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
                        responseBuilder.AppendLine($"+ Acct#{account.Id} [PRIMARY]: ${account.Balance}");
                    responseBuilder.AppendLine($"+ Acct#{account.Id}: ${account.Balance}");
                }

                responseBuilder.AppendLine($"Accounts Total: {accounts.Sum(a => a.Balance)}");
            
                EconomyPlugin.Instance
                    .Torch
                    .CurrentSession?
                    .Managers?
                    .GetManager<IChatManagerServer>()?
                    .SendMessageAsOther("Server",
                        responseBuilder.ToString(),
                        MyFontEnum.Green,
                        playerId);
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
                            $"Lacking {amount - fromAccount.Balance}{EconomyPlugin.Instance.Config.CurrencyAbbreviation} to complete that transfer.");
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
                           
                            ModCommunication.SendMessageTo(new DialogMessage("Transfer Received", null, 
                                $"You have been sent {amount}{EconomyPlugin.Instance.Config.CurrencyAbbreviation} by {Context.Player.DisplayName}."), toPlayerId);
                            ModCommunication.SendMessageTo(new DialogMessage("Transfer Sent", null, 
                                $"You have sent {amount}{EconomyPlugin.Instance.Config.CurrencyAbbreviation} to {targetPlayerNameOrId}."), fromPlayerId);
                        });
                });
        }
    }
}
