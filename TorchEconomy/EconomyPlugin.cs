﻿using Dapper;
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
using TorchEconomy.Managers;
using VRage.Game;

namespace TorchEconomy
{
    public class EconomyPlugin : EconomyPluginBase, IWpfPlugin
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

        public static T GetManager<T>() where T : BaseManager
        {
            return Instance._managers.FirstOrDefault(m => m is T) as T;
        }
        
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

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
                    _managers.AddRange(GetContainer().GetAllInstances<BaseManager>());
                    
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
    }
}
