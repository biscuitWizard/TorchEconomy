using Dapper;
using NLog.Fluent;
using Torch.API;
using Torch.API.Managers;
using TorchEconomySE.Data;
using TorchEconomySE.Models;

namespace TorchEconomySE.Managers
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
        
        private void PlayerJoined(IPlayer player)
        {
            using(var connection = ConnectionFactory.Open())
            {
                var playerAccount = connection.QueryFirstOrDefault<AccountDataObject>(
                    SQL.SELECT_ACCOUNT, 
                    new { playerId = player.SteamId });

                if(playerAccount == null)
                {
                    Log.Info("Creating Bank Account for Player# " + player.SteamId);
                    Log.Info(string.Format("Allocating {0} to Player# {1}", Config.StartingFunds, player.SteamId));

                    // Create a new account with the starting money defined in config.
                    connection.Execute(
                        SQL.INSERT_ACCOUNT,
                        new { playerId = player.SteamId, balance = Config.StartingFunds });
                }
            }
        }
    }
}
