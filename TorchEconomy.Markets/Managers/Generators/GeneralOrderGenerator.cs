using System;
using System.Collections.Generic;
using System.Linq;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Types;
using TorchEconomy.Markets.Data;
using TorchEconomy.Markets.Data.DataObjects;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Data.Types;

namespace TorchEconomy.Markets.Managers.Generators
{
	public class GeneralOrderGenerator : IOrderGenerator
	{
		public bool CanHandle(IndustryTypeEnum industryType)
        {
            return industryType == IndustryTypeEnum.Military || industryType == IndustryTypeEnum.Consumer
                                                             || industryType == IndustryTypeEnum.Research ||
                                                             industryType == IndustryTypeEnum.Industrial;
        }

        public NPCMarketOrder[] GenerateOrders(IndustryTypeEnum industryType, NPCDataObject npc,
            MarketDataObject market)
        {
            var simulationProvider = EconomyPlugin.GetDataProvider<MarketSimulationProvider>();
            
            decimal marginFlux = 0;
            var items = simulationProvider.GetUniversalItems(npc.IndustryType);

            switch (npc.IndustryType)
            {
                case IndustryTypeEnum.Industrial:
                    // Industrial buys industrial trade goods at a high price.
                    // Industrial buys ore at a moderate price.
                    // Industrial sells ingots at a low price.
                    marginFlux = new decimal(0.04);
                    break;
                case IndustryTypeEnum.Consumer:
                    // Consumer buys ingots at a high price.
                    // Consumer sells components at a low price.
                    marginFlux = new decimal(0.08);
                    break;
                case IndustryTypeEnum.Research:
                    // Research buys components at a high price.
                    // Research sells research trade goods at a low price.
                    marginFlux = new decimal(0.12);
                    break;
                case IndustryTypeEnum.Military:
                    // Military buys research trade goods at a high price.
                    // Military sells industrial trade goods & ammo at a low price.
                    marginFlux = new decimal(0.16);
                    break;
            }

            var orderManager = EconomyPlugin.GetManager<MarketOrderManager>();
            var npcOrders = new List<NPCMarketOrder>();
            var random = new Random();
            foreach (var item in items)
            {

                if (EconomyMarketsPlugin.Instance.Config.Blacklist.Any(b => b.Value == item.Definition.Id.ToString()))
                    continue; // This entry is blacklisted.

                var affinity = item.IndustryAffinities[npc.IndustryType];
                var minMarginFlux = (double) ((marginFlux / 2m) * -1m);
                var maxMarginFlux = (double) marginFlux;
                var orderMarginFlux = random.NextRange(minMarginFlux, maxMarginFlux);

                var order = new NPCMarketOrder
                {
                    DesiredStock = 10000,
                    Definition = item.Definition,
                    MarketId = market.Id,
                    MarginFlux = new decimal(orderMarginFlux),
                    DemandMultiplier = 1
                };

                var orderType = affinity == MarketAffinity.AmbivalentBuy
                                || affinity == MarketAffinity.ExtremeBuy
                                || affinity == MarketAffinity.Buy
                    ? BuyOrderType.Buy
                    : BuyOrderType.Sell;

                var basePrice = simulationProvider.GetUniversalItemValue(item.Definition.Id);
                var price = basePrice;
                if (orderType == BuyOrderType.Buy)
                    price = price * (1m + order.MarginFlux);
                else
                    price = price * (1m - order.MarginFlux);

                orderManager.UpdateOrAddMarketOrder(orderType, market.Id, item.Definition.Id,
                        price, -1)
                    .Then(newOrder => { order.OrderId = newOrder.Id; });
                npcOrders.Add(order);
            }

            return npcOrders.ToArray();
        }
    }
}