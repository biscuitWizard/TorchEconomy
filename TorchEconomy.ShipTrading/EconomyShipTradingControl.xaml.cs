using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TorchEconomy.ShipTrading
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EconomyShipTradingControl : UserControl
    {
        public EconomyShipTradingPlugin Plugin { get; }
        private EconomyShipTradingConfig _config => Plugin.Config;

        public EconomyShipTradingControl()
        {
            InitializeComponent();

            ButtonValueRemoveItem.IsEnabled = false;
        }

        public EconomyShipTradingControl(EconomyShipTradingPlugin plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

        private static readonly Regex _numericRegex = new Regex("[^0-9.-]+");
        private void PreviewNumericInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _numericRegex.IsMatch(e.Text);
        }
        
        private void SaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }

        private void RevertConfig_OnClick(object sender, RoutedEventArgs e)
        {
//            Plugin.NewConfig();
            Plugin.Save();
        }

        private void AddValueItem(object sender, RoutedEventArgs e)
        {
            _config.SellableShips.Add(new EconomyShipTradingConfig.ShipOrderBlueprint());
        }

        private void RemoveValueItem(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedValueItem();
            if (item == null)
            {
                ButtonValueRemoveItem.IsEnabled = false;
                return;
            }

            _config.SellableShips.Remove(item);
            ButtonValueRemoveItem.IsEnabled = false;
        }

        private void ValueSelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var item = GetSelectedValueItem();
            if (item == null)
            {
                ButtonValueRemoveItem.IsEnabled = false;
                return;
            }

            ButtonValueRemoveItem.IsEnabled = true;
        }

        private EconomyShipTradingConfig.ShipOrderBlueprint GetSelectedValueItem()
        {
            return ValueDataGrid.SelectedItem as EconomyShipTradingConfig.ShipOrderBlueprint;
        }

        private void DataGridCell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && cell.IsSelected)
            {
                var item = cell.DataContext as EconomyShipTradingConfig.ShipOrderBlueprint;
                if (item!=null)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
