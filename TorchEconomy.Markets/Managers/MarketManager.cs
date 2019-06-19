using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Sandbox.ModAPI;
using TorchEconomy;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Data.DataObjects;
using VRage.Game.ModAPI;

namespace TorchEconomy.Markets.Managers
{
    public class MarketManager : BaseManager
    {
        // ReSharper disable once InconsistentNaming
        public MarketManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Promise<MarketDataObject> CreateMarket(long parentGridId, ulong creatorPlayerId,
            string marketName, float range, long? accountId = null, bool isOpen = false, bool isNPC = false)
        {
            return new Promise<MarketDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    connection.Execute(
                        SQL.INSERT_MARKET,
                        new {parentGridId = parentGridId, creatorId = creatorPlayerId,
                            name = marketName, range = range, accountId = accountId,
                            isOpen = isOpen, isNPC = isNPC
                        });

                    var market = connection.QueryFirst<MarketDataObject>(
                        SQL.SELECT_MARKET_BY_GRID,
                        new {parentGridId = parentGridId});
                    resolve(market);
                }
            });
        }

        public Promise<MarketDataObject[]> GetMarkets()
        {
            return new Promise<MarketDataObject[]>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(connection.Query<MarketDataObject>(SQL.SELECT_MARKETS).ToArray());
                }
            });
        }

        public Promise<MarketDataObject> GetMarketByNameOrId(string marketNameOrId, ulong playerId)
        {
            return new Promise<MarketDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    var searchString = marketNameOrId;
                    if (!long.TryParse(searchString, out var index))
                        searchString += "%";

                    var market = connection.QueryFirstOrDefault<MarketDataObject>(
                        SQL.SELECT_MARKET_BY_NAME_AND_OWNER,
                        new {creatorId = playerId, marketNameOrId = searchString});

                    if (market.CreatorId != playerId)
                        reject(null);
                    resolve(market);
                }
            });
        }

        public IPromise SetMarketOpenStatus(long marketId, bool isOpen)
        {
            return new Promise((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    connection.Execute(
                        SQL.MUTATE_MARKET_OPEN,
                        new {id = marketId, isOpen = isOpen});
                    resolve();
                }
            });
        }

        public IPromise SetMarketAccount(long marketId, long accountId)
        {
            return new Promise((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    connection.Execute(
                        SQL.MUTATE_MARKET_ACCOUNT,
                        new {id = marketId, accountId = accountId});
                    resolve();
                }
            });
        }

        public Promise<MarketDataObject> GetConnectedMarket(IMyCubeGrid fromGrid)
        {
            return new Promise<MarketDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    var markets = connection.Query<MarketDataObject>(SQL.SELECT_MARKETS);
                    
                    foreach (var market in markets)
                    {
                        var marketGrid = MyAPIGateway.Entities.GetEntityById(market.ParentGridId) as IMyCubeGrid;
                        if (MyAPIGateway.GridGroups.HasConnection(fromGrid, marketGrid,
                            GridLinkTypeEnum.Logical))
                        {
                            resolve(market);
                            return;
                        }
                    }
                    
                    reject(new LogicLevelException("Unable to find any connected markets. Have you docked to a market?"));
                }
            });
        }

        public Promise DeleteMarket(long marketId)
        {
            return new Promise((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    connection.Execute(SQL.DELETE_MARKET, new {id = marketId});
                    resolve();
                }
            });
        }
    }
}
