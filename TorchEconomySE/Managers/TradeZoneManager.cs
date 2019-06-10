using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TorchEconomySE.Data;
using TorchEconomySE.Models;

namespace TorchEconomySE.Managers
{
    public class TradeZoneManager : BaseManager
    {
        // ReSharper disable once InconsistentNaming
        private readonly LazyProperty<List<TradeZoneDataObject>> TradeZones;
        
        public TradeZoneManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
            TradeZones = new LazyProperty<List<TradeZoneDataObject>>(RefreshTradeZones, 
                TimeSpan.FromMinutes(30));
        }

        protected virtual List<TradeZoneDataObject> RefreshTradeZones()
        {
            using (var connection = ConnectionFactory.Open())
            {
                return connection
                    .Query<TradeZoneDataObject>(SQL.SELECT_TRADEZONES)
                    .ToList();
            }
        }

        public IEnumerable<TradeZoneDataObject> GetTradeZonesInRange(VRageMath.Vector3D position)
        {
            foreach (var tradeZone in TradeZones.Get())
            {
                if (tradeZone.Position.DistanceFrom(position) <= tradeZone.Range)
                    yield return tradeZone;
            }
        }
    }
}
