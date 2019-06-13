using System.IO;
using System.Windows.Controls;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Session;
using Torch.Views;

namespace TorchEconomy.ShipTrading
{
	public class EconomyShipTradingPlugin : EconomyPluginBase, IWpfPlugin
	{
		private static readonly Logger Log = LogManager.GetLogger("Economy.ShipTrading");
        
		public static EconomyShipTradingPlugin Instance;
		private Persistent<EconomyShipTradingConfig> _config;
		private UserControl _control;
		
		
		public EconomyShipTradingConfig Config => _config?.Data;
		
		/// <inheritdoc />
		public UserControl GetControl() => _control ?? (_control = new PropertyGrid() { DataContext = Config/*, IsEnabled = false*/});

		public override void Init(ITorchBase torch)
		{
			base.Init(torch);
			
			// Load up the configuration
			string path = Path.Combine(StoragePath, "Economy.ShipTrading.cfg");
			Log.Info($"Attempting to load config from {path}");
			_config = Persistent<EconomyShipTradingConfig>.Load(path);
		}
	}
}