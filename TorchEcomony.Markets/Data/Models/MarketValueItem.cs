using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TorchEconomy.Data.Types;
using TorchEconomy.Markets.Data.Types;
using VRage.Game;

namespace TorchEconomy.Markets.Data.Models
{
    public struct MarketValueItem
    {
        public MyDefinitionBase Definition { get; set; }
        public decimal Value { get; set; }
        public ConcurrentDictionary<IndustryTypeEnum, MarketAffinity> IndustryAffinities { get; set; }
        public string FriendlyName
        {
            get { return Definition.DisplayNameText; }
        }

        public decimal FriendlyValue
        {
            get { return Math.Round(Value, 2);  }
        }

        public MarketValueItem(MyDefinitionBase definition, decimal value, 
            ConcurrentDictionary<IndustryTypeEnum, MarketAffinity> industryAffinities)
        {
            Definition = definition;
            Value = value;
            IndustryAffinities = industryAffinities;
        }
    }
}