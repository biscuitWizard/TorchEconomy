using System.Linq;
using Dapper;
using TorchEconomy.Data;
using TorchEconomy.Markets.Data.DataObjects;

namespace TorchEconomy.Markets.Managers
{
    public class MarketOrderManager : BaseMarketManager
    {
        public MarketOrderManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Promise<MarketOrderDataObject[]> GetMarketOrders(long marketId)
        {
            return new Promise<MarketOrderDataObject[]>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(
                        connection.Query<MarketOrderDataObject>(
                            SQL.SELECT_MARKET_ORDERS, 
                            new {marketId = marketId})
                            .ToArray());
                }
            });
        }
    }
}