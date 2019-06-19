using Sandbox.Common.ObjectBuilders.Definitions;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace TorchEconomy
{
    public static class Support
    {
        public static bool InventoryAdd(IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);

            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amount, PhysicalContent = content };

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, definitionId))
            {
                inventory.AddItems(inventoryItem.Amount, inventoryItem.PhysicalContent, -1);
                return true;
            }

            // Inventory full. Could not add the item.
            return false;
        }
    }
}