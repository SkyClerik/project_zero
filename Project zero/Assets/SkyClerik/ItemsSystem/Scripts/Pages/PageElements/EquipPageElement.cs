using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Элемент страницы, представляющий собой слот экипировки.
    /// Наследует функциональность GridPageElementBase для управления визуалом одного слота.
    /// </summary>
    public class EquipPageElement : GridPageElementBase
    {
        private const string _titleText = "Хранилище предметов";
        private VisualElement _rootElement;
        private List<VisualElement> _styles;
        private const string _styleID = "style";

        /// <summary>
        /// Инициализирует новый экземпляр класса EquipPageElement.
        /// </summary>
        /// <param name="inventoryStorage">Главный контроллер инвентаря.</param>
        /// <param name="document">UIDocument, к которому принадлежит страница.</param>
        /// <param name="itemContainer">Логический контейнер предметов, связанный с этим слотом экипировки.</param>
        /// <param name="rootID">ID корневого визуального элемента страницы/слота в UIDocument.</param>
        public EquipPageElement(InventoryStorage inventoryStorage, UIDocument document, ItemContainer itemContainer, string rootID)
            : base(inventoryStorage, document, itemContainer, rootID)
        {
            _rootElement = _root.Q(rootID);
            _styles = _rootElement.Query<VisualElement>(name: _styleID).ToList();

            foreach (var style in _styles)
            {
                Debug.Log($"_style : {style.name}");
            }

            ServiceProvider.Get<InventoryAPI>().OnItemPickUp += EquipPageElement_OnItemPickUp;
            ServiceProvider.Get<InventoryAPI>().OnItemDrop += EquipPageElement_OnItemDrop;
        }

        private void EquipPageElement_OnItemPickUp(ItemVisual item, GridPageElementBase gridPage)
        {
            if (gridPage == this)
                return;

            var equipContainer = _itemContainer as PlayerEquipContainer;
            if (equipContainer == null)
                return;

            bool typeMatch = (equipContainer.AllowedItemType == UnityEngine.DataEditor.ItemType.Any) || (equipContainer.AllowedItemType == item.ItemDefinition.ItemType);

            Color borderColor;
            if (typeMatch)
            {
                if (_itemContainer.GetItems().Count > 0)
                    borderColor = Color.yellow;
                else
                    borderColor = Color.green;
            }
            else
            {
                borderColor = Color.red;
            }

            foreach (var style in _styles)
            {
                style.SetBorderWidth(3);
                style.SetBorderRadius(3);
                style.SetBorderColor(borderColor);
            }
        }

        private void EquipPageElement_OnItemDrop(ItemVisual item, GridPageElementBase gridPage)
        {
            foreach (var style in _styles)
            {
                style.SetBorderWidth(0);
                style.SetBorderRadius(0);
                style.SetBorderColor(Color.red);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            var inventoryAPI = ServiceProvider.Get<InventoryAPI>();
            if (inventoryAPI != null)
            {
                inventoryAPI.OnItemPickUp -= EquipPageElement_OnItemPickUp;
                inventoryAPI.OnItemDrop -= EquipPageElement_OnItemDrop;
            }
        }

        public override void FinalizeDrag()
        {
            base.FinalizeDrag();
        }

        public override PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            if (!_root.enabledSelf || !_root.visible)
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);

            var equipContainer = _itemContainer as PlayerEquipContainer;
            if (equipContainer != null &&
                equipContainer.AllowedItemType != UnityEngine.DataEditor.ItemType.Any &&
                draggedItem.ItemDefinition.ItemType != equipContainer.AllowedItemType)
            {
                _placementResults = new PlacementResults().Init(ReasonConflict.invalidSlotType, Vector2.zero, Vector2Int.zero, null, this);
                InventoryStorage.MainTelegraph.Hide();
                return _placementResults;
            }

            if (_itemContainer.GetItems().Count == 0)
            {
                return base.ShowPlacementTarget(draggedItem);
            }
            else if (_itemContainer.GetItems().Count == 1)
            {
                _placementResults = new PlacementResults().Init(
                    conflict: ReasonConflict.SwapAvailable,
                    position: GetGlobalCellPosition(Vector2Int.zero),
                    suggestedGridPosition: Vector2Int.zero,
                    overlapItem: GetItemVisual(_itemContainer.GetItems()[0]),
                    targetInventory: this);

                InventoryStorage.MainTelegraph.SetPosition(GetGlobalCellPosition(Vector2Int.zero));
                InventoryStorage.MainTelegraph.SetPlacement(ReasonConflict.SwapAvailable, _itemContainer.CellSize.x, _itemContainer.CellSize.y);
            }
            else
            {
                _placementResults = new PlacementResults().Init(ReasonConflict.intersectsObjects, Vector2.zero, Vector2Int.zero, null, this);
                InventoryStorage.MainTelegraph.Hide();
            }

            return _placementResults;
        }
    }
}
