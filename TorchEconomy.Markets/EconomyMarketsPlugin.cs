using System.IO;
using System.Windows.Controls;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Session;
using Torch.Views;

namespace TorchEconomy.Markets
{
	public class EconomyMarketsPlugin : TorchPluginBase, IWpfPlugin
	{
		private static readonly Logger Log = LogManager.GetLogger("Economy.Markets");
        
		public static EconomyMarketsPlugin Instance;
		private Persistent<EconomyMarketConfig> _config;
		private UserControl _control;
		
		
		public EconomyMarketConfig Config => _config?.Data;
		
		/// <inheritdoc />
		public UserControl GetControl() => _control ?? (_control = new EconomyMarketsControl(this) { DataContext = Config/*, IsEnabled = false*/});

		public override void Init(ITorchBase torch)
		{
			base.Init(torch);
			
			// Load up the configuration
			string path = Path.Combine(StoragePath, "Economy.Markets.cfg");
			Log.Info($"Attempting to load config from {path}");
			_config = Persistent<EconomyMarketConfig>.Load(path);

			Instance = this;
		}
		
		public void Save()
		{
			_config.Save();
		}
	}
}