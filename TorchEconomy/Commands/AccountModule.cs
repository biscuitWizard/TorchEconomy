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
        
        [Command("balance", "Lists current bank account balance.")]
        public void Balance()
        {
            var manager = GetManager<AccountsManager>();
            var playerId = Context.Player.SteamUserId;
            Log.Info(playerId + " is requesting balance");

            var responseBuilder = new StringBuilder("Your Accounts:");
            responseBuilder.AppendLine();

            var accounts = manager.GetAccounts(playerId).ToArray();
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
        }

        [Command("give", "Alias for /transfer <target> <amount>")]
        public void GiveBalance(string target, decimal amount) { TransferBalance(target, amount); }
        [Command("pay", "Alias for /transfer <target> <amount>")]
        public void PayBalance(string target, decimal amount) { TransferBalance(target, amount); }
        [Command("transfer", "Transfers currency to the target player.")]        
        public void TransferBalance(string targetPlayerNameOrId, decimal amount)
        {
            var fromPlayerId = Context.Player.SteamUserId;
            
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

            var connectionFactory = new MysqlConnectionFactory();
            using (var connection = connectionFactory.Open())
            {
                var playerAccount = connection.QueryFirst<AccountDataObject>(
                    SQL.SELECT_ACCOUNTS,
                    new { playerId = fromPlayerId });
                if(playerAccount.Balance - amount < 0)
                {
                    Context.Respond($"Lacking {amount - playerAccount.Balance}{EconomyPlugin.Instance.Config.CurrencyAbbreviation} to complete that transfer.");
                    return;
                }

                connection.Execute(
                    SQL.MUTATE_ACCOUNT_BALANCE,
                    new { playerId = toPlayerId, amount });

                var inverseAmount = amount * -1;
                connection.Execute(
                    SQL.MUTATE_ACCOUNT_BALANCE,
                    new { playerId = fromPlayerId, inverseAmount });

                ModCommunication.SendMessageTo(new DialogMessage("Transfer Received", null, $"You have been sent {amount}{EconomyPlugin.Instance.Config.CurrencyAbbreviation} by {Context.Player.DisplayName}."), toPlayerId);
            }
        }
    }
}
