using System;
using VRage.Game;

namespace TorchEconomy.Data.Models
{
    public struct MarketValueItem
    {
        public MyDefinitionBase Definition { get; set; }
        public decimal Value { get; set; }
        public string FriendlyName
        {
            get { return Definition.DisplayNameText; }
        }

        public decimal FriendlyValue
        {
            get { return Math.Round(Value, 2);  }
        }

        public MarketValueItem(MyDefinitionBase definition, decimal value)
        {
            Definition = definition;
            Value = value;
        }
    }
}