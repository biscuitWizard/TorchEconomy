using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NLog;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using Torch;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using TorchEconomySE.Data;
using TorchEconomySE.Models;
using VRage.Game;

namespace TorchEconomySE.Commands
{
    [Category("econ")]
    public class AccountModule : CommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.Account");

        [Command("balance", "Lists current bank account balance.")]
        public void Balance()
        {
            var connectionFactory = new MysqlConnectionFactory();
            using (var connection = connectionFactory.Open())
            {
                var playerId = Context.Player.SteamUserId;

                Log.Info(playerId + " is requesting balance");

                var playerAccount = connection.QueryFirst<AccountDataObject>(
                    SQL.SELECT_ACCOUNT,
                    new { playerId = playerId });

                EconomyPlugin.Instance
                    .Torch
                    .CurrentSession?
                    .Managers?
                    .GetManager<IChatManagerServer>()?
                    .SendMessageAsOther("Server",
                        string.Format($"Current Balance: {0}{EconomyPlugin.Instance.Config.CurrencyAbbreviation}", playerAccount.Balance),
                        MyFontEnum.Blue,
                        playerId);
            }
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
                    SQL.SELECT_ACCOUNT,
                    new { playerId = fromPlayerId });
                if(playerAccount.Balance - amount < 0)
                {
                    Context.Respond($"Lacking {amount - playerAccount.Balance}{EconomyPlugin.Instance.Config.CurrencyAbbreviation} to complete that transfer.");
                    return;
                }

                connection.Execute(
                    SQL.MODIFY_ACCOUNT_BALANCE,
                    new { playerId = toPlayerId, amount });

                var inverseAmount = amount * -1;
                connection.Execute(
                    SQL.MODIFY_ACCOUNT_BALANCE,
                    new { playerId = fromPlayerId, inverseAmount });

                ModCommunication.SendMessageTo(new DialogMessage("Transfer Received", null, $"You have been sent {amount}{EconomyPlugin.Instance.Config.CurrencyAbbreviation} by {Context.Player.DisplayName}."), toPlayerId);
            }
        }
    }
}
