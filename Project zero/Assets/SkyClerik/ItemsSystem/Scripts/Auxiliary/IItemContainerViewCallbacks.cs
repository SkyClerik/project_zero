using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Интерфейс для обратных вызовов из ItemContainer в его визуальное представление.
    /// </summary>
    public interface IItemContainerViewCallbacks
    {
        /// <summary>
        /// Вызывается при добавлении предмета в контейнер.
        /// </summary>
        /// <param name="item">Добавленный предмет.</param>
        void OnItemAddedCallback(ItemBaseDefinition item);
        /// <summary>
        /// Вызывается при удалении предмета из контейнера.
        /// </summary>
        /// <param name="item">Удаленный предмет.</param>
        void OnItemRemovedCallback(ItemBaseDefinition item);
        /// <summary>
        /// Вызывается при полной очистке контейнера.
        /// </summary>
        void OnClearedCallback();
        /// <summary>
        /// Вызывается при изменении занятости ячеек сетки.
        /// </summary>
        void OnGridOccupancyChangedCallback();
        /// <summary>
        /// Вызывается при перемещении предмета в контейнере.
        /// </summary>
        /// <param name="item">Перемещенный предмет.</param>
        /// <param name="oldPosition">Старая позиция предмета в сетке.</param>
        void OnItemMovedCallback(ItemBaseDefinition item, Vector2Int oldPosition);
    }
}
