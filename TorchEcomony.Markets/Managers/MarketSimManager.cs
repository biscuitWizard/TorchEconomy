using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Sandbox.Definitions;
using TorchEconomy.Data;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Data.Models;
using VRage.Game;
using VRage.ObjectBuilders;

namespace TorchEconomy.Markets.Managers
{
    public class MarketSimManager : BaseMarketManager
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Markets.Managers.Market_Simulation");
        
        private readonly Dictionary<MyDefinitionId, MarketValueItem> _itemValues 
            = new Dictionary<MyDefinitionId, MarketValueItem>();
        
//        private readonly Dictionary<MyDefinitionId, decimal> _prices = new Dictionary<MyDefinitionId, decimal>();
        private MyDefinitionManager _definitionManager;
        
        public MarketSimManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
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

            var orePrices = new Dictionary<string, double>
            {
                { "Iron", .02 },
                { "Nickel", .04 },
                { "Silicon", .02 },
                { "Cobalt", .05 },
                { "Gold", .07 },
                { "Silver", .09 },
                { "Magnesium", .04},
                { "Stone", 0.005 },
                { "Uranium", .08 },
                { "Default", .01 },
                { "Platinum", .095 }
            };
            
            // set the ingot prices.
            var oreType = MyObjectBuilderType.Parse("MyObjectBuilder_Ore");
            foreach (var oreDefinition in _definitionManager.GetPhysicalItemDefinitions())
            {
                if (!oreDefinition.AvailableInSurvival)
                    continue;
                if (!oreDefinition.Enabled)
                    continue;
                if (oreDefinition.Id.TypeId != oreType)
                    continue;

                if (orePrices.TryGetValue(oreDefinition.Id.SubtypeId.String, out var value))
                {
                    SetItemValue(oreDefinition.Id, (decimal)value);
                    continue;
                }
                
                SetItemValue(oreDefinition.Id, (decimal)orePrices["Default"]);
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
                    catch (KeyNotFoundException e)
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
                    
            Log.Info($"Generated prices for {_itemValues.Count} items.");
        }

        protected virtual void SetItemValue(MyDefinitionId id, decimal value)
        {
            var definition = _definitionManager.GetDefinition(id);
            _itemValues[id] = new MarketValueItem(definition, value);
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
    }
}