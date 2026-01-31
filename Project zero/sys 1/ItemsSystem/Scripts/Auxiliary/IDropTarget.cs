using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor; // Для ItemBaseDefinition

namespace SkyClerik.Inventory
{
    public interface IDropTarget
    {
        UIDocument GetDocument { get; }
        PlacementResults ShowPlacementTarget(ItemVisual itemVisual);
        void FinalizeDrag();

                void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition);
        void RemoveStoredItem(ItemVisual storedItem);

        void PickUp(ItemVisual storedItem);
        void Drop(ItemVisual storedItem, Vector2Int gridPosition);

        void AddItemToInventoryGrid(VisualElement item);

        /// <summary>
        /// Синхронно проверяет, может ли предмет быть размещен в сетке, и возвращает предложенную позицию.
        /// Работает только с логикой сетки (ячейками), без учета пикселей UI.
        /// </summary>
        /// <param name="item">Предмет для размещения.</param>
        /// <param name="suggestedGridPosition">Предложенная позиция в ячейках, если место найдено.</param>
        /// <returns>True, если предмет может быть размещен; иначе False.</returns>
        bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition);

        /// <summary>
        /// Возвращает ItemGridData для конкретного предмета.
        /// </summary>
        /// <param name="itemVisual">Визуальный элемент предмета, для которого нужно получить данные.</param>
        /// <returns>ItemGridData, если предмет найден, иначе null.</returns>
        ItemGridData GetItemGridData(ItemVisual itemVisual);

        /// <summary>
        /// Регистрирует визуальный элемент предмета с его логическими данными в сетке.
        /// </summary>
        /// <param name="visual">Визуальный элемент предмета.</param>
        /// <param name="gridData">Логические данные о предмете и его позиции в сетке.</param>
        void RegisterVisual(ItemVisual visual, ItemGridData gridData);

        /// <summary>
        /// Отменяет регистрацию визуального элемента предмета из сетки.
        /// </summary>
        /// <param name="visual">Визуальный элемент предмета.</param>
        void UnregisterVisual(ItemVisual visual);

        Vector2 CellSize { get; }
    }
}