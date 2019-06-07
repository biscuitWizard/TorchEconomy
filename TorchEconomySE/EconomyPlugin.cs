using System.Windows.Controls;
using Torch;
using Torch.API.Plugins;
using Torch.Views;

namespace TorchEconomySE
{
    public class EconomyPlugin : TorchPluginBase, IWpfPlugin
    {
        public EconomyConfiguration Config => _config?.Data;
        private Persistent<EconomyConfiguration> _config;

        private UserControl _control;

        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new PropertyGrid() { DataContext = Config/*, IsEnabled = false*/});
    }
}
