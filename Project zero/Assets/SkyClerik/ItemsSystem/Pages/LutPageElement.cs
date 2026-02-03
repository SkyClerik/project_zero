using System.Linq;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
	public class LutPageElement : GridPageElementBase
	{
		private const string _craftPageTitleText = "Трофейня";
		private VisualElement _header;
		private const string _headerID = "header";
		private Label _title;
		private const string _titleID = "l_title";
		private VisualElement _body;
		private const string _bodyID = "body";
        private Button _bPickupAll;
        private const string _bPickupAllID = "b_pickup_all";

        private InventoryPageElement _inventoryPageElement;

        public LutPageElement(ItemsPage itemsPage, UIDocument document, ItemContainer itemContainer, InventoryPageElement inventoryPageElement )
	: base(itemsPage, document, itemContainer, itemContainer.RootPanelName)
		{
            _inventoryPageElement = inventoryPageElement;

            _header = _root.Q(_headerID);
			_title = _header.Q<Label>(_titleID);
			_body = _root.Q(_bodyID);
            _bPickupAll = _root.Q<Button>(_bPickupAllID);

            _title.text = _craftPageTitleText;
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

            // Очищаем текущий контейнер лута.
            // При этом ItemBaseDefinition объекты не Destroy-ятся,
            // так как они могут быть в unplacedInInventory или уже в инвентаре игрока.
            // При RemoveItem destroy = false
            foreach (var item in itemsInLut)
            {
                _itemContainer.RemoveItem(item, destroy: false);
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