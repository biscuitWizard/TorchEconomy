using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRage.ObjectBuilders;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMyInventory = VRage.Game.ModAPI.IMyInventory;

namespace TorchEconomy
{
    public static class Extensions
    {
        public static bool BetterAddItems(this IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var content = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject(definitionId);

            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem
                {Amount = amount, PhysicalContent = content};

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, definitionId))
            {
                inventory.AddItems(inventoryItem.Amount, (MyObjectBuilder_PhysicalObject) inventoryItem.PhysicalContent,
                    -1);
                return true;
            }

            // Inventory full. Could not add the item.
            return false;
        }
        
        /// <summary>
        /// Find all grids attached to the specified grid, either by piston, rotor, connector or landing gear.
        /// This will iterate through all attached grids, until all are found.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type">Specifies if all attached grids will be found or only grids that are attached either by piston or rotor.</param>
        /// <returns>A list of all attached grids, including the original.</returns>
        public static List<IMyCubeGrid> GetAttachedGrids(this IMyEntity entity, AttachedGrids type = AttachedGrids.All)
        {
            var cubeGrid = entity as IMyCubeGrid;

            if (cubeGrid == null)
                return new List<IMyCubeGrid>();

            switch (type)
            {
                case AttachedGrids.Static:
                    // Should include connections via: Rotors, Pistons, Suspension.
                    return MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Mechanical);
                case AttachedGrids.All:
                default:
                    // Should include connections via: Landing Gear, Connectors, Rotors, Pistons, Suspension.
                    return MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Physical);
            }
        }
    }
}