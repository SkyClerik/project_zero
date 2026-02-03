using UnityEngine;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Структура, представляющая результат попытки размещения предмета в сетке.
    /// Содержит информацию о конфликте, предложенной позиции и пересекающемся предмете.
    /// </summary>
    public struct PlacementResults
    {
        /// <summary>
        /// Тип конфликта размещения.
        /// </summary>
        public ReasonConflict Conflict;
        /// <summary>
        /// Позиция в пикселях, соответствующая предложенной позиции в сетке.
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// Предложенная позиция в ячейках сетки.
        /// </summary>
        public Vector2Int SuggestedGridPosition;
        /// <summary>
        /// Визуальный элемент предмета, с которым пересекается перетаскиваемый предмет (если есть).
        /// </summary>
        public ItemVisual OverlapItem;
        /// <summary>
        /// Целевой инвентарь (страница), на который происходит размещение.
        /// </summary>
        public IDropTarget TargetInventory;

        /// <summary>
        /// Инициализирует результат размещения предмета.
        /// </summary>
        /// <param name="conflict">Тип конфликта размещения.</param>
        /// <param name="position">Позиция в пикселях.</param>
        /// <param name="suggestedGridPosition">Предложенная позиция в сетке.</param>
        /// <param name="overlapItem">Пересекающийся предмет.</param>
        /// <param name="targetInventory">Целевой инвентарь.</param>
        /// <returns>Инициализированная структура <see cref="PlacementResults"/>.</returns>
        public PlacementResults Init(ReasonConflict conflict, Vector2 position, Vector2Int suggestedGridPosition, ItemVisual overlapItem, IDropTarget targetInventory)
        {
            Conflict = conflict;
            Position = position;
            SuggestedGridPosition = suggestedGridPosition;
            OverlapItem = overlapItem;
            TargetInventory = targetInventory;

            return this;
        }

        /// <summary>
        /// Инициализирует результат размещения предмета (перегрузка без указания позиции в пикселях).
        /// </summary>
        /// <param name="conflict">Тип конфликта размещения.</param>
        /// <param name="suggestedGridPosition">Предложенная позиция в сетке.</param>
        /// <param name="overlapItem">Пересекающийся предмет.</param>
        /// <param name="targetInventory">Целевой инвентарь.</param>
        /// <returns>Инициализированная структура <see cref="PlacementResults"/>.</returns>
        public PlacementResults Init(ReasonConflict conflict, Vector2Int suggestedGridPosition, ItemVisual overlapItem, IDropTarget targetInventory)
        {
            Conflict = conflict;
            Position = Vector2.zero;
            SuggestedGridPosition = suggestedGridPosition;
            OverlapItem = overlapItem;
            TargetInventory = targetInventory;

            return this;
        }
    }
}