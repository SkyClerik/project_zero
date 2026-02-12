using System.Linq;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
	public class LutPageElement : GridPageElementBase
	{
		private const string _craftPageTitleText = "Трофейня";
		private VisualElement _body;
		private const string _bodyID = "body";
        private Button _bPickupAll;
        private const string _bPickupAllID = "b_pickup_all";

        private InventoryPageElement _inventoryPageElement;

        public LutPageElement(InventoryStorage itemsPage, UIDocument document, ItemContainer itemContainer, InventoryPageElement inventoryPageElement )
	: base(itemsPage, document, itemContainer, itemContainer.RootPanelName)
		{
            _inventoryPageElement = inventoryPageElement;
			_body = _root.Q(_bodyID);
            _bPickupAll = _root.Q<Button>(_bPickupAllID);

            _bPickupAll.clicked += TakeAllLootToInventory;
        }

        // Специфичная логика для лута
        public void TakeAllLootToInventory()
        {
            var playerInventoryContainer = ServiceProvider.Get<PlayerItemContainer>();
            if (playerInventoryContainer == null)
            {
                Debug.LogError("PlayerItemContainer не найден через ServiceProvider!");
                return;
            }

            var itemsInLut = _itemContainer.GetItems().ToList();
            var unplacedInInventory = playerInventoryContainer.AddItems(itemsInLut);

            foreach (var item in itemsInLut)
            {
                Debug.Log($"TakeAllLootToInventory : {ItemContainer.ItemRemoveReason.Transfer}");
                _itemContainer.RemoveItem(item, ItemContainer.ItemRemoveReason.Transfer);
            }
            
            // Возвращаем в лут те предметы, которые не поместились в инвентарь игрока
            if (unplacedInInventory.Any())
            {
                _itemContainer.AddItems(unplacedInInventory);
            }

            // Обновляем UI обеих страниц
            _inventoryPageElement.RefreshVisuals();
            base.RefreshVisuals();
        }
    }
}