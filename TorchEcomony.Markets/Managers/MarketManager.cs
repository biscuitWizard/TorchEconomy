using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TorchEconomy;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Data.DataObjects;

namespace TorchEconomy.Markets.Managers
{
    public class MarketManager : BaseManager
    {
        // ReSharper disable once InconsistentNaming
        public MarketManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Promise<MarketDataObject> CreateMarket(long parentGridId, ulong creatorPlayerId,
            string marketName, float range)
        {
            return new Promise<MarketDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    connection.Execute(
                        SQL.INSERT_MARKET,
                        new {parentGridId = parentGridId, creatorId = creatorPlayerId,
                            name = marketName, @range = range});

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
    }
}
