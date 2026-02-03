using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой структуру данных для хранения информации о предмете в сетке,
    /// включая его определение и позицию.
    /// </summary>
    [System.Serializable]
    public class ItemGridData
    {
        [SerializeField]
        private ItemBaseDefinition _itemDefinition;
        [SerializeField]
        private Vector2Int _gridPosition;

        /// <summary>
        /// Определение предмета (<see cref="ItemBaseDefinition"/>).
        /// </summary>
        public ItemBaseDefinition ItemDefinition => _itemDefinition;
        /// <summary>
        /// Позиция предмета в сетке.
        /// </summary>
        public Vector2Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        /// <summary>
        /// Размер предмета в ячейках сетки, вычисляемый на основе его определения.
        /// </summary>
        public Vector2Int GridSize => new Vector2Int(ItemDefinition.Dimensions.Width, ItemDefinition.Dimensions.Height);

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemGridData"/>.
        /// </summary>
        /// <param name="item">Определение предмета.</param>
        /// <param name="position">Позиция предмета в сетке.</param>
        public ItemGridData(ItemBaseDefinition item, Vector2Int position)
        {
            _itemDefinition = item;
            _gridPosition = position;
        }
    }
}