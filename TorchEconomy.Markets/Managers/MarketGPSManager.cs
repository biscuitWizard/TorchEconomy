using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.ModAPI;
using TorchEconomy.Data;
using TorchEconomy.Markets.Data.DataObjects;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace TorchEconomy.Markets.Managers
{
    public class MarketGPSManager : BaseMarketManager
    {
        private class MarketGPS
        {
            public long MarketId { get; set; }
            public float MarketRange { get; set; }
            public MyGps Gps { get; set; }
            public Vector3D Position { get; set; }
        }
        
        private static readonly Logger Log = LogManager.GetLogger("Economy.Managers.GPS");
        
        private DateTime _lastUpdate;
        private bool _processing = false;
        public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
        
        private readonly MarketManager _marketManager;
        private readonly Dictionary<long, List<MarketGPS>> _playerMarketsInRange 
            = new Dictionary<long, List<MarketGPS>>();
        private readonly Dictionary<MarketDataObject, MarketGPS> _cachedMarkets 
            = new Dictionary<MarketDataObject, MarketGPS>();
        
        public MarketGPSManager(IConnectionFactory connectionFactory, MarketManager marketManager) 
            : base(connectionFactory)
        {
            _marketManager = marketManager;
            _lastUpdate = DateTime.UtcNow;
        }

        public override void Start()
        {
            LoadMarkets();
            
            _marketManager.OnMarketCreated += MarketManager_OnMarketCreated;
            _marketManager.OnMarketDeleted += MarketManager_OnMarketDeleted;
            
            _processing = true;
            UpdateGps();
            _lastUpdate = DateTime.UtcNow;
        }

        private void MarketManager_OnMarketDeleted(MarketDataObject market)
        {
            UnregisterMarket(market.Id);
        }

        private void MarketManager_OnMarketCreated(MarketDataObject market)
        {
            RegisterMarket(market);
        }

        private void LoadMarkets()
        {
            _marketManager
                .GetMarkets()
                .Then(markets =>
                {
                    foreach (var market in markets)
                    {
                        RegisterMarket(market);
                    }
                });
        }

        private void RegisterMarket(MarketDataObject market)
        {
            var marketEntity = MyAPIGateway.Entities.GetEntityById(market.ParentGridId) as MyCubeGrid;
            if (marketEntity == null)
                return;

            var marketPosition = ((IMyEntity) marketEntity).GetPosition();
            var marketGps = new MarketGPS
            {
                Gps = CreateGPS(market, marketPosition),
                MarketRange = market.Range,
                MarketId = market.Id,
                Position = marketPosition
            };

            _cachedMarkets[market] = marketGps;
            marketEntity.OnStaticChanged += MarketEntity_OnStaticChanged;
        }


        private void UnregisterMarket(long marketId)
        {
            var cachedMarket = _cachedMarkets.Keys.FirstOrDefault(cm => cm.Id == marketId);
            if (cachedMarket == null)
                return;
            
            _cachedMarkets.Remove(cachedMarket);
        }

        private void MarketEntity_OnStaticChanged(MyCubeGrid cubeGrid, bool newValue)
        {
            if (newValue)
            {
                _marketManager.GetMarketByGridId(cubeGrid.EntityId)
                    .Then(market =>
                    {
                        var marketPosition = ((IMyEntity) cubeGrid).GetPosition();
                        var marketGps = new MarketGPS
                        {
                            Gps = CreateGPS(market, marketPosition),
                            MarketRange = market.Range,
                            MarketId = market.Id,
                            Position = marketPosition
                        };

                        _cachedMarkets[market] = marketGps;
                    });
            }
            else
            {
                // Remove GPS points for this market for players that have it. Remove it from the cache.
                var market = _cachedMarkets.Keys.FirstOrDefault(m => m.ParentGridId == cubeGrid.EntityId);
                if (market == null)
                    return;
                foreach (var playerId in _playerMarketsInRange.Keys)
                {
                    var marketsInRangeForPlayer = _playerMarketsInRange[playerId];
                    var marketGps = marketsInRangeForPlayer.FirstOrDefault(m => m.MarketId == market.Id);
                    if (marketGps != null)
                    {
                        marketsInRangeForPlayer.Remove(marketGps);
                        MyAPIGateway.Session?.GPS.RemoveGps(playerId, marketGps.Gps);
                    }
                }
                _cachedMarkets.Remove(market);
            }
        }

        public override void Update()
        {
            if (DateTime.UtcNow - _lastUpdate > UpdateInterval)
            {
                if (_processing)
                {
                    // Running behind.
                    Log.Warn("GPS Manager is running behind on updates! Is the server overloaded?");
                    _lastUpdate = DateTime.UtcNow;

                    return;
                }

                _processing = true;
                UpdateGps();
                _lastUpdate = DateTime.UtcNow;
            }
        }

        private Promise UpdateGps()
        {
            return new Promise((resolve, reject) =>
            {
                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    var playerPosition = player.GetPosition();
                    var marketsInRange = _cachedMarkets.Where(kvp =>
                            Utilities.DistanceFrom(kvp.Value.Position, playerPosition) < kvp.Key.Range)
                        .ToArray();

                    if (!_playerMarketsInRange.TryGetValue(player.IdentityId, out var playerGps))
                    {
                        playerGps = new List<MarketGPS>();
                        _playerMarketsInRange[player.IdentityId] = playerGps;
                    }

                    // Check if any GPS are now removed.
                    foreach (var gps in playerGps.ToArray())
                    {
                        if (Utilities.DistanceFrom(gps.Position, playerPosition) <= gps.MarketRange)
                            continue;
                        playerGps.Remove(gps);
                        MyAPIGateway.Session?.GPS.RemoveGps(player.Identity.IdentityId, gps.Gps);
                    }

                    // Add any GPS which are new.
                    foreach (var market in marketsInRange)
                    {
                        if (playerGps.Any(p => p.MarketId == market.Key.Id))
                            continue;

                        // Render it for the player.
                        MyAPIGateway.Session?.GPS.AddGps(player.Identity.IdentityId, market.Value.Gps);

                        // Track it locally.
                        playerGps.Add(market.Value);
                    }
                }

                _processing = false;
                resolve();
            });
        }

        private MyGps CreateGPS(MarketDataObject market, Vector3D position)
        {

            var gps = new MyGps();
            gps.Coords = position;
            gps.Name = $"(Market) {market.Name}";
            gps.DisplayName = $"(Market) {market.Name}";
            gps.GPSColor = new Color(0,275,275);
            gps.IsContainerGPS = true;
            gps.ShowOnHud = true;
            gps.DiscardAt = new TimeSpan?();
            gps.UpdateHash();

            return gps;
        }

        public override void Stop()
        {
            foreach (var player in _playerMarketsInRange.Keys)
            {
                foreach (var market in _playerMarketsInRange[player])
                {
                    MyAPIGateway.Session?.GPS.RemoveGps(player, market.Gps);
                }
            }

            _playerMarketsInRange.Clear();
            _cachedMarkets.Clear();
        }
    }
}