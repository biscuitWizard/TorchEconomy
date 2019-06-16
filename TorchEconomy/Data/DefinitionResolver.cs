using System;
using System.Collections.Generic;
using NLog;
using Sandbox.Definitions;
using VRage.Game;

namespace TorchEconomy.Data
{
    public class DefinitionResolver
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.DefinitionResolver");
        
        private readonly Dictionary<string, MyDefinitionId> _definitionMappings 
            = new Dictionary<string, MyDefinitionId>();

        public void Clear()
        {
            _definitionMappings.Clear();
        }
        
        public void Register(string friendlyName, MyDefinitionId definition)
        {
            if (string.IsNullOrEmpty(friendlyName))
            {
                Log.Warn($"Received definition with anomalous display name. Definition: {definition.ToString()}");
                return;
            }
            
            _definitionMappings[friendlyName] = definition;
        }

        public bool TryGetDefinitionByName(string friendlyName, out MyDefinitionBase definition)
        {
            definition = null;

            foreach (var definitionMapping in _definitionMappings)
            {
                if (definitionMapping.Key.StartsWith(friendlyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    definition = MyDefinitionManager.Static.GetDefinition(definitionMapping.Value);
                    return true;
                }
            }

            return false;
        }
    }
}