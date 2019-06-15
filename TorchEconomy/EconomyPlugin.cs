using Dapper;
using NLog;
using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Sandbox.Engine.Voxels;
using Sandbox.ModAPI;
using StructureMap;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using Torch.Views;
using TorchEconomy.Data;
using TorchEconomy.Managers;
using VRage.Game;

namespace TorchEconomy
{
    public class EconomyPlugin : TorchPluginBase, IWpfPlugin
    {
        private static IContainer _container;

        protected IContainer GetContainer()
        {
            if (_container == null)
            {
                _container = new Container();
                _container.Initialize();
            }

            return _container;
        }

        public IConnectionFactory GetConnectionFactory()
        {
            return GetContainer().GetInstance<IConnectionFactory>();
        }
        
        private static readonly Logger Log = LogManager.GetLogger("Economy");
        
        public static EconomyPlugin Instance;
        public EconomyConfig Config => _config?.Data;
        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new EconomyControl(this) { DataContext = Config/*, IsEnabled = false*/});
        
        
        private Persistent<EconomyConfig> _config;
        private TorchSessionManager _sessionManager;
        private UserControl _control;
        
        private readonly List<BaseManager> _managers = new List<BaseManager>();

        public static T GetManager<T>() where T : BaseManager
        {
            return Instance._managers.FirstOrDefault(m => m is T) as T;
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

            // Associate Torch containers for lifetime management.
            _sessionManager = Torch.Managers.GetManager(typeof(TorchSessionManager)) as TorchSessionManager;
            if (_sessionManager != null)
                _sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager.  Economy system won't work.");
            
            
            
            Log.Info("Torch Economy Initialized!");
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
                    
                    // Get all the managers.
                    var managers = GetContainer().GetAllInstances<BaseManager>();
                    _managers.AddRange(managers);
                    
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
