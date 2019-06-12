using Sandbox.Common.ObjectBuilders.Definitions;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace TorchEconomySE
{
    public static class Support
    {
        public static bool InventoryAdd(IMyInventory inventory, MyFixedPoint amount, MyDefinitionId definitionId)
        {
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);

            var gasContainer = content as MyObjectBuilder_GasContainerObject;
            if (gasContainer != null)
                gasContainer.GasLevel = 1f;

            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amount, PhysicalContent = content };

            if (inventory.CanItemsBeAdded(inventoryItem.Amount, definitionId))
            {
                inventory.AddItems(inventoryItem.Amount, (MyObjectBuilder_PhysicalObject)inventoryItem.PhysicalContent, -1);
                return true;
            }

            // Inventory full. Could not add the item.
            return false;
        }
    }
}