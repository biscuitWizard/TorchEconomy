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
                        new {parentGridId = parentGridId, creatorPlayerId = creatorPlayerId,
                            name = marketName, @range = range});

                    var market = connection.QueryFirst<MarketDataObject>(
                        SQL.SELECT_MARKET_BY_GRID,
                        new {parentGridId = parentGridId});
                    resolve(market);
                }
            });
        }
    }
}
