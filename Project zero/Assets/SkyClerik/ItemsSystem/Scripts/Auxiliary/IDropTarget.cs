using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Интерфейс, определяющий контракт для объектов, которые могут принимать перетаскиваемые предметы (UI сетки инвентаря).
    /// </summary>
    public interface IDropTarget
    {
        /// <summary>
        /// Возвращает UIDocument, к которому принадлежит цель перетаскивания.
        /// </summary>
        UIDocument GetDocument { get; }
        /// <summary>
        /// Показывает целевую область для размещения перетаскиваемого предмета,
        /// а также определяет возможные конфликты размещения.
        /// </summary>
        /// <param name="itemVisual">Перетаскиваемый визуальный элемент предмета.</param>
        /// <returns>Результаты размещения, включающие информацию о конфликте и предложенной позиции.</returns>
        PlacementResults ShowPlacementTarget(ItemVisual itemVisual);
        /// <summary>
        /// Завершает операцию перетаскивания.
        /// </summary>
        void FinalizeDrag();

        /// <summary>
        /// Добавляет хранящийся предмет в сетку.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета.</param>
        /// <param name="gridPosition">Позиция в сетке.</param>
        void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition);
        /// <summary>
        /// Удаляет хранящийся предмет из сетки.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета.</param>
        void RemoveStoredItem(ItemVisual storedItem);

        /// <summary>
        /// Поднимает предмет из сетки.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета.</param>
        void PickUp(ItemVisual storedItem);
        /// <summary>
        /// Отпускает предмет в сетку.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета.</param>
        /// <param name="gridPosition">Позиция в сетке.</param>
        void Drop(ItemVisual storedItem, Vector2Int gridPosition);

        /// <summary>
        /// Добавляет визуальный элемент предмета в сетку инвентаря.
        /// </summary>
        /// <param name="item">Визуальный элемент, который нужно добавить.</param>
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

        /// <summary>
        /// Возвращает размер одной ячейки сетки в пикселях.
        /// </summary>
        Vector2 CellSize { get; }
    }
}