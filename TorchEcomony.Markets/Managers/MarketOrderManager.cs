using System;
using System.Linq;
using Dapper;
using TorchEconomy.Data;
using TorchEconomy.Markets.Data.DataObjects;
using TorchEconomy.Markets.Data.Types;
using VRage;
using VRage.Game;

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

        public Promise<MarketOrderDataObject> UpdateOrAddMarketOrder(BuyOrderType orderType, long marketId, 
            MyDefinitionId itemDefinitionId, decimal pricePerOne, decimal quantity)
        {
            return new Promise<MarketOrderDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    var createdOn = DateTime.UtcNow.ToUnixTimestamp();
                    
                    var definitionHash = itemDefinitionId.ToString();
                    connection.Execute(
                        SQL.INSERT_OR_UPDATE_MARKET_ORDER,
                        new
                        {
                            marketId = marketId, definitionId = definitionHash,
                            price = pricePerOne, quantity = quantity, createdOn = createdOn,
                            orderType = (int)orderType
                        });

                    resolve(connection.QueryFirstOrDefault<MarketOrderDataObject>(
                        SQL.SELECT_MARKET_ORDER_BY_ITEM,
                        new
                        {
                            marketId = marketId, definitionId = definitionHash,
                            orderType = (int) orderType
                        }));
                }
            });
        }
    }
}