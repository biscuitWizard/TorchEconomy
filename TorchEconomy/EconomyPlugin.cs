using Dapper;
using NLog;
using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Sandbox;
using Sandbox.Engine.Voxels;
using Sandbox.ModAPI;
using StructureMap;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Server.Managers;
using Torch.Server.ViewModels;
using Torch.Session;
using Torch.Views;
using TorchEconomy.Data;
using TorchEconomy.Managers;
using TorchEconomy.Resources;
using VRage.Game;

namespace TorchEconomy
{
    public class EconomyPlugin : EconomyPluginBase, IWpfPlugin
    {
        public static DefinitionResolver DefinitionResolver { get; private set; }
        
        private static readonly Logger Log = LogManager.GetLogger("Economy");
        
        public static EconomyPlugin Instance;
        public EconomyConfig Config => _config?.Data;
        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new EconomyControl(this) { DataContext = Config/*, IsEnabled = false*/});
        
        
        private Persistent<EconomyConfig> _config;
        private TorchSessionManager _sessionManager;
        private UserControl _control;
        
        private readonly List<BaseManager> _managers = new List<BaseManager>();
        private readonly List<IDataProvider> _dataProviders = new List<IDataProvider>();

        public static T GetManager<T>() where T : BaseManager
        {
            return Instance._managers.FirstOrDefault(m => m is T) as T;
        }

        public static TProvider GetDataProvider<TProvider>() where TProvider : class, IDataProvider
        {
            return Instance._dataProviders.FirstOrDefault(p => p is TProvider) as TProvider;
        }
        
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            
            // Set static instance.
            Instance = this;

            Log.Info("Loading Torch Economy...");
            // Load up the configuration
            string path = Path.Combine(StoragePath, "Economy.cfg");
            Log.Info($"Attempting to load config from {path}");
            _config = Persistent<EconomyConfig>.Load(path);

            // Build the SQLite dependencies.
            SQLiteInstaller.CheckSQLiteInstalled();
            
            // Associate Torch containers for lifetime management.
            _sessionManager = Torch.Managers.GetManager(typeof(TorchSessionManager)) as TorchSessionManager;
            if (_sessionManager != null)
            {
                _sessionManager.SessionStateChanged += SessionChanged;
                
                // Add mod override for the economy client mod for client enhancements.
                _sessionManager.AddOverrideMod(1772298664);
            }
            else
            {
                Log.Warn("No session manager.  Economy system won't work.");
            }



            GetConnectionFactory().Setup();
            DefinitionResolver = GetContainer().GetInstance<DefinitionResolver>();
            
            Torch.GameStateChanged += OnGameStateChanged;
            Log.Info("Torch Economy Initialized!");
        }

        private void OnGameStateChanged(MySandboxGame game, TorchGameState newstate)
        {
            switch (newstate)
            {
                case TorchGameState.Creating:
                    var instanceManager = Torch
                        .Managers
                        .GetManager(typeof(InstanceManager)) as InstanceManager;

                    if (instanceManager == null)
                        return;
                    
                    var clientMod = new ModItemInfo(new MyObjectBuilder_Checkpoint.ModItem(1772298664));
                    if(!instanceManager.DedicatedConfig.Mods.Contains(clientMod))
                        instanceManager.DedicatedConfig.Mods
                            .Add(clientMod);
                    break;
            }
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
                    _managers.Clear();
                    _dataProviders.Clear();
                    
                    // Get all the managers.
                    var managers = GetContainer().GetAllInstances<BaseManager>();
                    _managers.AddRange(managers);
                    
                    // Get all the data providers.
                    _dataProviders.AddRange(GetContainer().GetAllInstances<IDataProvider>());
                    
                    // Start the data providers.
                    foreach (var dataProvider in _dataProviders)
                    {
                        dataProvider.Start();
                    }
                    
                    // Awaken the managers.
                    foreach (var manager in _managers)
                    {
                        manager.Awake();
                    }
                    
                    // Start the managers.
                    foreach (var manager in _managers)
                    {
                        manager.Start();
                    }
                    break;
                case TorchSessionState.Unloading:
                    // Stop and unload all the managers.
                    foreach (var manager in _managers)
                    {
                        manager.Stop();
                    }
                    _managers.Clear();
                    _dataProviders.Clear();
                    break;
            }
        }
        
        public void Save()
        {
            _config.Save();
        }

        public void NewConfig()
        {
            _config = new Persistent<EconomyConfig>("", new EconomyConfig());;
            _config.Path = Path.Combine(StoragePath, "Economy.cfg");
        }
    }
}
