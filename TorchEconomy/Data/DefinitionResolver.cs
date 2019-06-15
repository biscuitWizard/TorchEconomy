using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;

namespace TorchEconomy.Data
{
    public class DefinitionResolver
    {
        private readonly Dictionary<string, MyDefinitionId> _definitionMappings 
            = new Dictionary<string, MyDefinitionId>();

        public void Clear()
        {
            _definitionMappings.Clear();
        }
        
        public void Register(string friendlyName, MyDefinitionId definition)
        {
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