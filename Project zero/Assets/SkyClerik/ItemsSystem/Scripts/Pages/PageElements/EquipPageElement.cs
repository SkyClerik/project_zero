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
        private VisualElement _gridElement;
        private const string _gridElementID = "grid";
        private List<VisualElement> _styles;
        private const string _styleID = "style";

        /// <summary>
        /// Инициализирует новый экземпляр класса EquipPageElement.
        /// </summary>
        public EquipPageElement(InventoryStorage inventoryStorage, UIDocument document, ItemContainer itemContainer, string rootID)
            : base(inventoryStorage, document, itemContainer, rootID)
        {
            _rootElement = _root.Q(rootID);
            _gridElement = _rootElement.Q(_gridElementID);
            _styles = _gridElement.Query<VisualElement>(name: _styleID).ToList();

            ServiceProvider.Get<InventoryAPI>().OnItemPickUp += EquipPageElement_OnItemPickUp;
            ServiceProvider.Get<InventoryAPI>().OnItemDrop += EquipPageElement_OnItemDrop;
        }

        private void EquipPageElement_OnItemPickUp(ItemVisual item, GridPageElementBase gridPage)
        {
            if (gridPage == this) return;

            var equipContainer = _itemContainer as PlayerEquipContainer;
            if (equipContainer == null) return;

            bool typeMatch = (equipContainer.AllowedItemType == UnityEngine.DataEditor.ItemType.Any) ||
                             (equipContainer.AllowedItemType == item.ItemDefinition.ItemType);

            Color borderColor;
            if (typeMatch)
            {
                borderColor = (_itemContainer.GetItems().Count > 0) ? Color.yellow : Color.green;
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

        public override void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            while (storedItem.ItemDefinition.Dimensions.Angle != 0)
            {
                storedItem.Rotate();
            }

            base.Drop(storedItem, gridPosition);
        }

        public override void FinalizeDrag()
        {
            base.FinalizeDrag();
        }

        public override PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            if (currentHoverGridPosition.x < 0)
            {
                InventoryStorage.MainTelegraph.Hide();
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            if (!_root.enabledSelf || !_root.visible)
            {
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

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
                var equippedItemVisual = GetItemVisual(_itemContainer.GetItems()[0]);

                if (equippedItemVisual == draggedItem)
                {
                    _placementResults = new PlacementResults().Init(
                        conflict: ReasonConflict.None,
                        position: GetGlobalCellPosition(Vector2Int.zero),
                        suggestedGridPosition: Vector2Int.zero,
                        overlapItem: null,
                        targetInventory: this);
                    InventoryStorage.MainTelegraph.SetPosition(GetGlobalCellPosition(Vector2Int.zero));
                    InventoryStorage.MainTelegraph.SetPlacement(ReasonConflict.None, _gridElement.resolvedStyle.width, _gridElement.resolvedStyle.height);
                }
                else
                {
                    _placementResults = new PlacementResults().Init(
                        conflict: ReasonConflict.SwapAvailable,
                        position: GetGlobalCellPosition(Vector2Int.zero),
                        suggestedGridPosition: Vector2Int.zero,
                        overlapItem: equippedItemVisual,
                        targetInventory: this);

                    InventoryStorage.MainTelegraph.SetPosition(GetGlobalCellPosition(Vector2Int.zero));
                    InventoryStorage.MainTelegraph.SetPlacement(ReasonConflict.SwapAvailable, _gridElement.resolvedStyle.width, _gridElement.resolvedStyle.height);
                }
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