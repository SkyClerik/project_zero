using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    public class InventoryPageElement : GridPageElementBase
    {
        private const string _inventoryRootID = "inventory_root";

        public InventoryPageElement(ItemsPage itemsPage, UIDocument document, ItemContainer itemContainer)
            : base(itemsPage, document, itemContainer, _inventoryRootID)
        { }
    }
}
