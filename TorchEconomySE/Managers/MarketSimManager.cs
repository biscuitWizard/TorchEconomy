using System.Collections.Generic;
using System.Linq;
using NLog;
using Sandbox.Definitions;
using TorchEconomySE.Data;
using VRage.Game;
using VRage.ObjectBuilders;

namespace TorchEconomySE.Managers
{
    public class MarketSimManager : BaseManager
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Managers.Market_Simulation");
        
        private readonly Dictionary<MyDefinitionId, decimal> _prices = new Dictionary<MyDefinitionId, decimal>();
        private MyDefinitionManager _definitionManager;
        
        public MarketSimManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public override void Start()
        {
            base.Start();
            
            _definitionManager = MyDefinitionManager.Static;

            _prices.Clear();
            CalculateUniversalPrices();
        }

        public void CalculateUniversalPrices()
        {
            Log.Info("Generating procedural market price data. This may take some time...");

            // How much each second is worth when making things.
            const decimal energyCost = 10;
                    
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

                _prices[oreDefinition.Id] = 10;
            }

            foreach (var scrapDefinition in _definitionManager
                .GetAllDefinitions()
                .Where(d => d.Id.SubtypeName.Contains("Scrap")))
            {
                _prices[scrapDefinition.Id] = 10;
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
                if (_prices.ContainsKey(result.Id))
                    continue; // We've already calculated this. Pass.

                decimal cost = energyCost * (decimal)blueprintDefinition.BaseProductionTimeInSeconds;
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
                        cost += GetOrCalculateItemCost(prereq.Id) * (decimal) prereq.Amount;
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
                _prices[result.Id] = cost;
            }
                    
            Log.Info($"Generated prices for {_prices.Count} items.");
        }

        public decimal GetOrCalculateItemCost(MyDefinitionId id)
        {
            if (_prices.TryGetValue(id, out var cost))
            {
                return cost;
            }
            const decimal energyCost = 10;
            var blueprintDefinition = _definitionManager.TryGetBlueprintDefinitionByResultId(id);
            if (blueprintDefinition == null)
                throw new KeyNotFoundException($"Unable to find a blueprint for item id '{id}'.");
            
            cost = energyCost * (decimal)blueprintDefinition.BaseProductionTimeInSeconds;
            foreach (var prereq in blueprintDefinition.Prerequisites)
            {
                if (prereq.Id == id)
                    continue;
                cost += GetOrCalculateItemCost(prereq.Id) * (decimal)prereq.Amount;
            }

            _prices[id] = cost;
            
            return cost;
        }
    }
}