using System;
using System.Collections.Generic;
using System.Linq;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Types;
using TorchEconomy.Markets;
using TorchEconomy.Markets.Data;
using TorchEconomy.Markets.Data.DataObjects;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Data.Types;
using TorchEconomy.Markets.Managers;
using TorchEconomy.Markets.Managers.Generators;

namespace TorchEconomy.ShipTrading.Managers.Generators
{
	public class ShipyardOrderGenerator : IOrderGenerator
	{
		public bool CanHandle(IndustryTypeEnum industryType)
		{
			return industryType == IndustryTypeEnum.Shipyard;
		}

		public NPCMarketOrder[] GenerateOrders(IndustryTypeEnum industryType, NPCDataObject npc, MarketDataObject market)
		{
			var simulationProvider = EconomyPlugin.GetDataProvider<MarketSimulationProvider>();

			const decimal marginFlux = 0.1m;
			var items = simulationProvider.GetUniversalItems(npc.IndustryType);

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