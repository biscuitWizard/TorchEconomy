using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dapper;
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
using TorchEconomy.Markets.Managers.Generators;
using VRage.Game;
using VRage.ObjectBuilders;

namespace TorchEconomy.Markets.Managers
{
    public class MarketSimulationManager : BaseMarketManager
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Markets.Managers.Market_Simulation");
        
        private readonly ConcurrentDictionary<long, List<NPCMarketOrder>> _npcMarketOrders 
            = new ConcurrentDictionary<long, List<NPCMarketOrder>>();
        private readonly List<IOrderGenerator> _orderGenerators = new List<IOrderGenerator>();
        
        public MarketSimulationManager(IConnectionFactory connectionFactory) 
            : base(connectionFactory)
        {

            RegisterGenerator(new GeneralOrderGenerator());
        }

        public override void Start()
        {
            base.Start();

            using (var connection = ConnectionFactory.Open())
            {
                var npcMarkets = connection.Query<MarketDataObject>(SQL.SELECT_MARKETS)
                    .Where(m => m.IsNPC)
                    .ToArray();
                var orderManager = EconomyPlugin.GetManager<MarketOrderManager>();
                var npcManager = EconomyPlugin.GetManager<NPCManager>();
                
                foreach (var npcMarket in npcMarkets)
                {
                    orderManager.DeleteMarketOrders(npcMarket.Id)
                        .Then(() =>
                        {
                            npcManager.GetNPC((long)npcMarket.CreatorId)
                                .Then(npc => { GenerateNPCOrders(npc, npcMarket); });
                        });
                }
            }
        }

        public void RegisterGenerator(IOrderGenerator generator)
        {
            _orderGenerators.Add(generator);
        }

        public Promise GenerateNPCOrders(NPCDataObject npc, MarketDataObject market)
        {
            return new Promise((resolve, reject) =>
            {
                var orders = new List<NPCMarketOrder>();
                foreach (var generator in _orderGenerators)
                {
                    if (!generator.CanHandle(npc.IndustryType))
                        continue;
                    
                    orders.AddRange(generator.GenerateOrders(npc.IndustryType, npc, market));
                }

                _npcMarketOrders[market.Id] = orders;
                resolve();
            });
        }
    }
}