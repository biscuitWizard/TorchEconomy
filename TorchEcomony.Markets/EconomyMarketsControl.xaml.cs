﻿using System;
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

        public EconomyMarketsControl()
        {
            InitializeComponent();
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
    }
}
