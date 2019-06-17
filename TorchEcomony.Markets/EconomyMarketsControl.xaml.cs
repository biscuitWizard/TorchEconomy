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

namespace TorchEconomy.Markets
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EconomyMarketsControl : UserControl
    {
        public EconomyMarketsPlugin Plugin { get; }
        private EconomyMarketConfig _config => Plugin.Config;

        public EconomyMarketsControl()
        {
            InitializeComponent();

            ButtonValueRemoveItem.IsEnabled = false;
            ButtonBlacklistRemoveItem.IsEnabled = false;
        }

        public EconomyMarketsControl(EconomyMarketsPlugin plugin) : this()
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
            _config.ValueDefinitionBindings.Add(new EconomyMarketConfig.ValueDefinitionBinding { Protected = false});
        }

        private void RemoveValueItem(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedValueItem();
            if (item == null)
            {
                ButtonValueRemoveItem.IsEnabled = false;
                return;
            }

            _config.ValueDefinitionBindings.Remove(item);
            ButtonValueRemoveItem.IsEnabled = false;
        }

        private void AddBlacklistItem(object sender, RoutedEventArgs e)
        {
            _config.Blacklist.Add(new EconomyMarketConfig.BlacklistItem());
        }

        private void RemoveBlacklistItem(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedBlacklistItem();
            if (item == null)
            {
                ButtonBlacklistRemoveItem.IsEnabled = false;
                return;
            }

            _config.Blacklist.Remove(item);
            ButtonBlacklistRemoveItem.IsEnabled = false;
        }

        private void BlacklistSelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if(GetSelectedBlacklistItem() != null)
                ButtonBlacklistRemoveItem.IsEnabled = true;
            ButtonBlacklistRemoveItem.IsEnabled = false;
        }

        private void ValueSelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var item = GetSelectedValueItem();
            if (item == null || item.Protected)
            {
                ButtonValueRemoveItem.IsEnabled = false;
                return;
            }

            ButtonValueRemoveItem.IsEnabled = true;
        }

        private EconomyMarketConfig.BlacklistItem GetSelectedBlacklistItem()
        {
            return BlacklistDataGrid.SelectedItem as EconomyMarketConfig.BlacklistItem;
        }

        private EconomyMarketConfig.ValueDefinitionBinding GetSelectedValueItem()
        {
            return ValueDataGrid.SelectedItem as EconomyMarketConfig.ValueDefinitionBinding;
        }

        private void DataGridCell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && cell.IsSelected)
            {
                var item = cell.DataContext as EconomyMarketConfig.ValueDefinitionBinding;
                if (item!=null && item.Protected)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
