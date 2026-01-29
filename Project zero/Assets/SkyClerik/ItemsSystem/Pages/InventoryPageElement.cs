using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    public class InventoryPageElement : GridPageElementBase
    {
        private const string _inventoryRootID = "inventory_root";

        public InventoryPageElement(
            ItemsPage itemsPage,
            UIDocument document,
            ItemContainerBase itemContainer,
            Vector2 cellSize,
            Vector2Int inventoryGridSize)
            : base(
                  itemsPage,
                  document,
                  itemContainer,
                  cellSize,
                  inventoryGridSize,
                  _inventoryRootID)
        { }

        protected override void CalculateGridRect()
        {
            _gridRect = _inventoryGrid.worldBound; // Пока оставляем worldBound для _gridRect, если нет другого способа определить общий размер сетки.
            _gridRect.width = (_cellSize.width * _inventoryDimensions.width) + (_cellSize.width / 2);
            _gridRect.height = (_cellSize.height * _inventoryDimensions.height) + (_cellSize.height / 2);
            _gridRect.x -= (_cellSize.width / 4);
            _gridRect.y -= (_cellSize.height / 4);
            Debug.Log($"[{GetType().Name}] Calculated Grid Rect: _cellSize={_cellSize}, _gridRect={_gridRect}");
        }
    }
}
