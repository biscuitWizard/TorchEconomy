using Dapper;
using NLog;
using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Sandbox.Engine.Voxels;
using StructureMap;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using Torch.Views;
using TorchEconomySE.Data;
using TorchEconomySE.Managers;
using TorchEconomySE.Models;
using VRage.Game;

namespace TorchEconomySE
{
    public class EconomyPlugin : TorchPluginBase, IWpfPlugin
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy");
        
        public static EconomyPlugin Instance;
        public EconomyConfig Config => _config?.Data;
        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new EconomyControl() { DataContext = Config/*, IsEnabled = false*/});
        
        
        private Persistent<EconomyConfig> _config;
        private TorchSessionManager _sessionManager;
        private UserControl _control;
        
        private readonly List<BaseManager> _managers = new List<BaseManager>();
        private readonly IContainer _container;

        public EconomyPlugin()
        {
            _container = new Container();
        }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            // Load up the configuration
            _config = new Persistent<EconomyConfig>("", new EconomyConfig());

            // Associate Torch containers for lifetime management.
            _sessionManager = Torch.Managers.GetManager(typeof(TorchSessionManager)) as TorchSessionManager;
            if (_sessionManager != null)
                _sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager.  Economy system won't work.");
            
            // Boot up the injection container.
            _container.Initialize();

            // Set static instance.
            Instance = this;
        }

        public override void Update()
        {
            base.Update();

            foreach (var manager in _managers)
            {
                manager.Update();
            }
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loaded:
                    // Get all the managers.
                    _managers.AddRange(_container.GetAllInstances<BaseManager>());
                    // Start the managers.
                    foreach (var manager in _managers)
                    {
                        manager.Start();
                    }
                    
//                    var planetGenerator = planetGenerators.First();
//                    foreach (var oreMapping in planetGenerator.OreMappings)
//                    {
//                        var material = MyDefinitionManager.Static.GetVoxelMaterialDefinition(oreMapping.Type);
//                        if (material == null)
//                            continue;
//                        material.
//                    }
//                    //planetGenerator.SurfaceMaterialTable.First().
//                    foreach(var oreType in planetGenerator.OreMappings.Select(o => o.Type).Distinct())
//                    {
//                        float oreEntries = planetGenerator.OreMappings.Count(o => o.Type == oreType);
//                        float totalOreEntries = planetGenerator.OreMappings.Count();
//                        float baseInflation = 10f;
//                        float minDepth = planetGenerator.OreMappings.Min(o => o.Start);
//                        float maxDepth = planetGenerator.OreMappings.Max(o => o.Start + o.Depth);
//                        float averageDepthForOre = planetGenerator.OreMappings.Where(o => o.Type == oreType).Average(o => o.Depth + o.Start);
//
//                        var basecost = (oreEntries / totalOreEntries) * baseInflation;
//                        var modifier = (.2f * (minDepth / (maxDepth - averageDepthForOre))) + 1;
//
//                        Log.Info($"Ore {oreType} value is {basecost * modifier}");
//                    }

                    //foreach (var blueprint in MyDefinitionManager.Static.GetBlueprintDefinitions())
                    //{
                    //    CalculateBlueprintPrice(blueprint);
                    //}
                    //Log.Info($"[DONE] Market Data Generated for {_prices.Count} items.");
                    break;
                case TorchSessionState.Unloading:
                    // Stop and unload all the managers.
                    foreach (var manager in _managers)
                    {
                        manager.Stop();
                    }

                    _managers.Clear();
                    break;
            }
        }
    }
}
