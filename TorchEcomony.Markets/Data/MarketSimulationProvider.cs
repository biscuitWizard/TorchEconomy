using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Sandbox.Definitions;
using TorchEconomy.Data;
using TorchEconomy.Data.Types;
using TorchEconomy.Markets.Data.Models;
using VRage.Game;

namespace TorchEconomy.Markets.Data
{
	public class MarketSimulationProvider : IDataProvider
	{
        private static readonly Logger Log = LogManager.GetLogger("Economy.Markets.Data.Market_Simulation");
		
        public EconomyMarketConfig MarketConfig { get; }
        
        private readonly DefinitionResolver _definitionResolver;
		private MyDefinitionManager _definitionManager;
		private readonly Dictionary<MyDefinitionId, MarketValueItem> _itemValues 
			= new Dictionary<MyDefinitionId, MarketValueItem>();

        public MarketSimulationProvider(DefinitionResolver definitionResolver,
            EconomyMarketConfig marketConfig)
        {
            _definitionResolver = definitionResolver;
            MarketConfig = marketConfig;
        }
		
		public void OnStart()
		{
			_definitionManager = MyDefinitionManager.Static;

			_itemValues.Clear();
			CalculateUniversalPrices();
		}

		public void OnSessionLoaded()
		{
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
		
		public MarketValueItem[] GetUniversalItems<TDefType>() where TDefType : class
		{
			return _itemValues.Values.Where(v => v.Definition is TDefType).ToArray();
		}
        
		public IEnumerable<MarketValueItem> GetUniversalItems()
		{
			return _itemValues.Values.AsEnumerable();
		}
		
		public MarketValueItem[] GetUniversalItems(IndustryTypeEnum industryType)
        {
            return _itemValues
                .Values
                .Where(v => Enumerable.Contains(v.IndustryTypes, industryType))
                .ToArray();
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
	}
}