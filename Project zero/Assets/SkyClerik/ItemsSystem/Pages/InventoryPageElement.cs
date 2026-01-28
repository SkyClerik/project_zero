using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    // InventoryPageElement теперь наследуется от GridPageElementBase
    public class InventoryPageElement : GridPageElementBase
    {
        private const string _inventoryRootID = "inventory_root";

        public InventoryPageElement(ItemsPage itemsPage, UIDocument document, ItemContainerBase itemContainer, Vector2 cellSize, Vector2Int inventoryGridSize)
            : base(itemsPage, document, itemContainer, cellSize, inventoryGridSize, _inventoryRootID) // Вызываем конструктор базового класса
        {
            // Здесь может быть дополнительная специфичная инициализация для инвентаря, если она есть
            // Например, установка конкретных кнопок инвентаря, если они есть.
        }

        // Если есть методы, которые должны быть переопределены или являются специфичными для инвентаря, они будут здесь
        // В данном случае, большинство методов перенесены в базовый класс, так что здесь очень мало кода.
        // Переопределим ConfigureInventoryDimensions, так как там был специфичный Debug.Log
        protected override void ConfigureInventoryDimensions()
        {
            // Размеры инвентаря (_inventoryDimensions) и массив занятости сетки (_gridOccupancy)
            // теперь инициализируются в конструкторе базового класса.
            Debug.Log($"[{GetType().Name}] Configured Inventory Dimensions: Width={_inventoryDimensions.width}, Height={_inventoryDimensions.height}");
        }

        // Переопределим CalculateGridRect, так как там был специфичный Debug.Log
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
