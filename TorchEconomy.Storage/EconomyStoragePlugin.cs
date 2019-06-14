using System.IO;
using System.Windows.Controls;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Session;
using Torch.Views;

namespace TorchEconomy.Storage
{
	public class EconomyStoragePlugin : EconomyPluginBase, IWpfPlugin
	{
		private static readonly Logger Log = LogManager.GetLogger("Economy.Storage");
        
		public static EconomyStoragePlugin Instance;
		private Persistent<EconomyStorageConfig> _config;
		private UserControl _control;
		
		
		public EconomyStorageConfig Config => _config?.Data;
		
		/// <inheritdoc />
		public UserControl GetControl() => _control ?? (_control = new PropertyGrid() { DataContext = Config/*, IsEnabled = false*/});

		public override void Init(ITorchBase torch)
		{
			base.Init(torch);
			
			// Load up the configuration
			string path = Path.Combine(StoragePath, "Economy.Storage.cfg");
			Log.Info($"Attempting to load config from {path}");
			_config = Persistent<EconomyStorageConfig>.Load(path);
		}
		
		public void Save()
		{
			_config.Save();
		}
	}
}