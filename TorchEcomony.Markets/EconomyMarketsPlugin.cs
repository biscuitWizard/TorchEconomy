using System.Windows.Controls;
using NLog;
using Torch;
using Torch.API.Plugins;
using Torch.Session;
using Torch.Views;

namespace TorchEconomy.Markets
{
	public class EconomyMarketsPlugin : EconomyPluginBase, IWpfPlugin
	{
		private static readonly Logger Log = LogManager.GetLogger("Economy.Markets");
        
		public static EconomyPlugin Instance;
		private Persistent<EconomyMarketConfig> _config;
		private TorchSessionManager _sessionManager;
		private UserControl _control;
		
		
		public EconomyMarketConfig Config => _config?.Data;
		
		/// <inheritdoc />
		public UserControl GetControl() => _control ?? (_control = new PropertyGrid() { DataContext = Config/*, IsEnabled = false*/});
	}
}