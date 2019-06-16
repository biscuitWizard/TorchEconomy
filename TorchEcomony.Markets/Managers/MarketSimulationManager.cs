using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Sandbox.Definitions;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Types;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Data;
using TorchEconomy.Markets.Data.DataObjects;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Data.Types;
using VRage.Game;
using VRage.ObjectBuilders;

namespace TorchEconomy.Markets.Managers
{
    public class MarketSimulationManager : BaseMarketManager
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Markets.Managers.Market_Simulation");
        
        private readonly MarketSimulationProvider _simulationProvider;
        private readonly DefinitionResolver _definitionResolver;
        
        public MarketSimulationManager(IConnectionFactory connectionFactory, 
            MarketSimulationProvider simulationProvider,
            DefinitionResolver definitionResolver) 
            : base(connectionFactory)
        {
            _simulationProvider = simulationProvider;
            _definitionResolver = definitionResolver;
        }

        public Promise GenerateNPCOrders(NPCDataObject npc, MarketDataObject market)
        {
            return new Promise((resolve, reject) =>
            {
                decimal marginFlux = 0;
                MarketValueItem[] items = null;
                var buyMarketItems = new List<MarketValueItem>();
                var sellMarketItems = new List<MarketValueItem>();
                
                switch (npc.IndustryType)
                {
                    case IndustryTypeEnum.Industrial:
                        // Industrial buys industrial trade goods at a high price.
                        // Industrial buys ore at a moderate price.
                        // Industrial sells ingots at a low price.
                        marginFlux = new decimal(0.02);
                        items = _simulationProvider.GetUniversalItems(IndustryTypeEnum.Industrial);
                        break;
                    case IndustryTypeEnum.Consumer:
                        // Consumer buys ingots at a high price.
                        // Consumer sells components at a low price.
                        marginFlux = new decimal(0.04);
                        items = _simulationProvider.GetUniversalItems(IndustryTypeEnum.Consumer);
                        break;
                    case IndustryTypeEnum.Research:
                        // Research buys components at a high price.
                        // Research sells research trade goods at a low price.
                        marginFlux = new decimal(0.06);
                        items = _simulationProvider.GetUniversalItems(IndustryTypeEnum.Research);
                        break;
                    case IndustryTypeEnum.Military:
                        // Military buys research trade goods at a high price.
                        // Military sells industrial trade goods & ammo at a low price.
                        marginFlux = new decimal(0.08);
                        items = _simulationProvider.GetUniversalItems(IndustryTypeEnum.Military);
                        break;
                }

                var orderManager = EconomyPlugin.GetManager<MarketOrderManager>();
                foreach (var item in buyMarketItems)
                {
                    var multiplier = (1 + marginFlux);
                    if (npc.IndustryType == IndustryTypeEnum.Industrial)
                        multiplier = 1; // Industrial override.
                    
                    orderManager.UpdateOrAddMarketOrder(BuyOrderType.Buy, market.Id, item.Definition.Id,
                        item.Value * multiplier, -1);
                }
                
                foreach (var item in sellMarketItems)
                {
                    orderManager.UpdateOrAddMarketOrder(BuyOrderType.Sell, market.Id, item.Definition.Id,
                        item.Value * (1 - marginFlux), -1);
                }

                resolve();
            });
        }
    }
}