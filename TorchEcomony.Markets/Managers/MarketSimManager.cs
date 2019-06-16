using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Sandbox.Definitions;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Types;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Data.DataObjects;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Data.Types;
using VRage.Game;
using VRage.ObjectBuilders;

namespace TorchEconomy.Markets.Managers
{
    public class MarketSimManager : BaseMarketManager
    {
        private readonly DefinitionResolver _definitionResolver;
        private static readonly Logger Log = LogManager.GetLogger("Economy.Markets.Managers.Market_Simulation");
        
        private readonly Dictionary<MyDefinitionId, MarketValueItem> _itemValues 
            = new Dictionary<MyDefinitionId, MarketValueItem>();
        
//        private readonly Dictionary<MyDefinitionId, decimal> _prices = new Dictionary<MyDefinitionId, decimal>();
        private MyDefinitionManager _definitionManager;
        
        public MarketSimManager(IConnectionFactory connectionFactory, DefinitionResolver definitionResolver) 
            : base(connectionFactory)
        {
            _definitionResolver = definitionResolver;
        }

        public override void Start()
        {
            base.Start();
            
            _definitionManager = MyDefinitionManager.Static;

            _itemValues.Clear();
            CalculateUniversalPrices();
        }

        public void CalculateUniversalPrices()
        {
            Log.Info("Generating procedural market price data. This may take some time...");
            
            // set the ingot prices.
            var definitions = MyDefinitionManager.Static.GetAllDefinitions().ToArray();
            foreach (var valueBinding in EconomyMarketsPlugin.Instance.Config.ValueDefinitionBindings)
            {
                SetItemValue(valueBinding.DefinitionId, (decimal)valueBinding.Value);
            }

            foreach (var scrapDefinition in _definitionManager
                .GetAllDefinitions()
                .Where(d => d.Id.SubtypeName.Contains("Scrap")))
            {
                SetItemValue(scrapDefinition.Id, 10);
            }

            var blacklist = new List<MyDefinitionId>();
            // set the prices for everything else
            foreach (var blueprintDefinition in _definitionManager.GetBlueprintDefinitions())
            {
                // Ignore disabled blueprints.
                if (!blueprintDefinition.Enabled || !(blueprintDefinition is MyBlueprintDefinition))
                    continue;
                
                var result = blueprintDefinition.Results.First();

                if (blacklist.Contains(result.Id))
                    continue; // We're not going to calc this. Pass.
                if (_itemValues.ContainsKey(result.Id))
                    continue; // We've already calculated this. Pass.

                var cost = MarketConfig.EnergySecondsValue * (decimal)blueprintDefinition.BaseProductionTimeInSeconds;
                var skipBlueprint = false;
                foreach (var prereq in blueprintDefinition.Prerequisites)
                {
                    if (blacklist.Contains(prereq.Id))
                    {
                        Log.Warn($"Ignoring blueprint for '{result.Id}', unable to find recipe for prerequisite '{prereq.Id}'..");
                        blacklist.Add(result.Id);
                        skipBlueprint = true;
                        break;
                    }
                    
                    try
                    {
                        cost += GetOrCalculateUniversalItemValue(prereq.Id) * (decimal) prereq.Amount;
                    }
                    catch (KeyNotFoundException)
                    {
                        Log.Warn($"Ignoring blueprint for '{result.Id}', unable to find recipe for prerequisite '{prereq.Id}'..");
                        skipBlueprint = true;
                        blacklist.Add(prereq.Id);
                        blacklist.Add(result.Id);
                        break;
                    }
                }

                if (skipBlueprint)
                    continue;

                cost = cost / (decimal) result.Amount;
                SetItemValue(result.Id, cost);
            }

//            var ammo = GetUniversalItems<MyAmmoMagazineDefinition>();
//            var ore = GetUniversalItems<MyObjectBuilder_Ore>();
            
            Log.Info($"Generated prices for {_itemValues.Count} items.");
        }

        protected virtual void SetItemValue(MyDefinitionId id, decimal value)
        {
            var definition = _definitionManager.GetDefinition(id);
            if (definition.Id.TypeId.IsNull
                || string.IsNullOrEmpty(definition.Id.SubtypeName))
                return; // Skip this entry. It's useless.
            
            _definitionResolver.Register(definition.DisplayNameText, id);

            var industryTypes = new List<IndustryTypeEnum>();
            
            var subtype = id.SubtypeId.ToString();
            if (subtype.StartsWith("Industrial_", StringComparison.InvariantCultureIgnoreCase))
            {
                industryTypes.Add(IndustryTypeEnum.Military);
                industryTypes.Add(IndustryTypeEnum.Industrial);
            } else if (definition is MyAmmoMagazineDefinition)
            {
                industryTypes.Add(IndustryTypeEnum.Military);
            } else if (subtype.StartsWith("Research_", StringComparison.InvariantCultureIgnoreCase))
            {
                industryTypes.Add(IndustryTypeEnum.Research);
                industryTypes.Add(IndustryTypeEnum.Military);
            } else if (id.TypeId.ToString()
                           .Equals("MyObjectBuilder_Ore", StringComparison.InvariantCultureIgnoreCase))
            {
                industryTypes.Add(IndustryTypeEnum.Industrial);
            } else if (id.TypeId.ToString()
                .Equals("MyObjectBuilder_Ingot", StringComparison.InvariantCultureIgnoreCase))
            {
                industryTypes.Add(IndustryTypeEnum.Industrial);
                industryTypes.Add(IndustryTypeEnum.Consumer);
            } else if (definition is MyComponentDefinitionBase)
            {
                industryTypes.Add(IndustryTypeEnum.Consumer);
            }
            
            _itemValues[id] = new MarketValueItem(definition, value, industryTypes);
        }

        public MarketValueItem[] GetUniversalItems(IndustryTypeEnum industryType)
        {
            return _itemValues
                .Values
                .Where(v => v.IndustryTypes.Contains(industryType))
                .ToArray();
        }

        public MarketValueItem[] GetUniversalItems<TDefType>() where TDefType : class
        {
            return _itemValues.Values.Where(v => v.Definition is TDefType).ToArray();
        }
        
        public IEnumerable<MarketValueItem> GetUniversalItems()
        {
            return _itemValues.Values.AsEnumerable();
        }

        public decimal GetOrCalculateUniversalItemValue(MyDefinitionId id)
        {
            if (_itemValues.TryGetValue(id, out var itemValue))
            {
                return itemValue.Value;
            }
            
            var blueprintDefinition = _definitionManager.TryGetBlueprintDefinitionByResultId(id);
            if (blueprintDefinition == null)
                throw new KeyNotFoundException($"Unable to find a blueprint for item id '{id}'.");
            
            var cost = MarketConfig.EnergySecondsValue * (decimal)blueprintDefinition.BaseProductionTimeInSeconds;
            foreach (var prereq in blueprintDefinition.Prerequisites)
            {
                if (prereq.Id == id)
                    continue;
                cost += GetOrCalculateUniversalItemValue(prereq.Id) * (decimal)prereq.Amount;
            }

            cost = cost / (decimal)blueprintDefinition.Results.First().Amount;
            SetItemValue(id, cost);
            
            return cost;
        }

        public decimal GetUniversalItemValue(MyDefinitionId id)
        {
            return _itemValues[id].Value;
        }

        public decimal GetUniversalItemValue(string itemName)
        {
            return GetUniversalMarketValueItem(itemName).Value;
        }

        public MarketValueItem GetUniversalMarketValueItem(string itemName)
        {
            var itemValue = _itemValues
                .Values
                .Cast<MarketValueItem?>()
                .FirstOrDefault(k =>
                    k.Value.FriendlyName.StartsWith(itemName, StringComparison.InvariantCultureIgnoreCase));
            
            if(itemValue == null)
                throw new KeyNotFoundException("Unable to find item.");

            return itemValue.Value;
        }

        public Promise GenerateNPCOrders(NPCDataObject npc, MarketDataObject market)
        {
            return new Promise((resolve, reject) =>
            {
                decimal marginFlux = 0;
                var buyMarketItems = new List<MarketValueItem>();
                var sellMarketItems = new List<MarketValueItem>();
                
                switch (npc.IndustryType)
                {
                    case IndustryTypeEnum.Industrial:
                        // Industrial buys industrial trade goods at a high price.
                        // Industrial buys ore at a moderate price.
                        // Industrial sells ingots at a low price.
                        marginFlux = new decimal(0.02);
                        buyMarketItems.AddRange(GetUniversalItems().Where(v => v.Definition.Id.SubtypeId.String == "MyObjectBuilder_Ore"));
                        sellMarketItems.AddRange(GetUniversalItems().Where(v => v.Definition.Id.SubtypeId.String == "MyObjectBuilder_Ingot"));
                        break;
                    case IndustryTypeEnum.Consumer:
                        // Consumer buys ingots at a high price.
                        // Consumer sells components at a low price.
                        marginFlux = new decimal(0.04);
                        sellMarketItems.AddRange(GetUniversalItems<MyComponentDefinition>());
                        break;
                    case IndustryTypeEnum.Research:
                        // Research buys components at a high price.
                        // Research sells research trade goods at a low price.
                        marginFlux = new decimal(0.06);
                        buyMarketItems.AddRange(GetUniversalItems<MyComponentDefinition>());
                        break;
                    case IndustryTypeEnum.Military:
                        // Military buys research trade goods at a high price.
                        // Military sells industrial trade goods & ammo at a low price.
                        marginFlux = new decimal(0.08);
                        sellMarketItems.AddRange(GetUniversalItems<MyAmmoMagazineDefinition>());
                        break;
                }

                var orderManager = EconomyPlugin.GetManager<MarketOrderManager>();
                foreach (var item in buyMarketItems)
                {
                    var multiplier = (1 + marginFlux);
                    if (npc.IndustryType == IndustryTypeEnum.Industrial)
                        multiplier = 1; // Industrial override.
                    
                    orderManager.UpdateOrAddMarketOrder(BuyOrderType.Buy, market.Id, item.Definition.Id,
                        item.Value * multiplier, -1);
                }
                
                foreach (var item in sellMarketItems)
                {
                    orderManager.UpdateOrAddMarketOrder(BuyOrderType.Sell, market.Id, item.Definition.Id,
                        item.Value * (1 - marginFlux), -1);
                }

                resolve();
            });
        }
    }
}