using System;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// Абстрактный базовый класс для всех предметов.
    /// </summary>
    public abstract class ItemBaseDefinition : BaseDefinition
    {
        [SerializeField]
        [Tooltip("Цена предмета в магазинах.")]
        private int _price;
        public int Price => _price;

        [SerializeField]
        [Tooltip("Текущее кол-во")]
        private int _curStack;
        public int Stack { get => _curStack; set => _curStack = value; }

        /// <summary>
        /// Добавляет предметы в стак.
        /// </summary>
        /// <param name="amount">Количество для добавления.</param>
        /// <param name="remainder">Излишек, если стак переполнен.</param>
        public void AddStack(int amount, out int remainder)
        {
            if (!_stackable)
            {
                remainder = amount;
                return;
            }

            var newStack = _curStack + amount;
            if (newStack > _maxStack)
            {
                _curStack = _maxStack;
                remainder = newStack - _maxStack;
                return;
            }

            _curStack = newStack;
            remainder = 0;
        }

        /// <summary>
        /// Удаляет предметы из стака.
        /// </summary>
        /// <param name="amount">Количество для удаления.</param>
        /// <returns>Возвращает количество, которое было фактически удалено.</returns>
        public int RemoveStack(int amount)
        {
            if (amount > _curStack)
            {
                var removedAmount = _curStack;
                _curStack = 0;
                return removedAmount;
            }

            _curStack -= amount;
            return amount;
        }

        [SerializeField]
        [Tooltip("Максимальное кол-во")]
        private int _maxStack;
        public int MaxStack => _maxStack;

        [SerializeField]
        private bool _stackable = true;
        public bool Stackable => _stackable;

        [SerializeField]
        [Tooltip("Размер и поворот предмета")]
        private ItemDimensions _dimensions;
        public ItemDimensions Dimensions { get => _dimensions; set => _dimensions = value; }
    }

    [Serializable]
    public class ItemDimensions
    {
        [SerializeField]
        private int defaultWidth = 1;
        [SerializeField]
        private int defaultHeight = 1;
        [SerializeField]
        private float defaultAngle = 0;
        [SerializeField]
        private int currentHeight = 1;
        [SerializeField]
        private int currentWidth = 1;
        [SerializeField]
        private float currentAngle = 0;

        public int DefaultWidth { get => defaultWidth; set => defaultWidth = value; }
        public int DefaultHeight { get => defaultHeight; set => defaultHeight = value; }
        public float DefaultAngle { get => defaultAngle; set => defaultAngle = value; }
        public int CurrentHeight { get => currentHeight; set => currentHeight = value; }
        public int CurrentWidth { get => currentWidth; set => currentWidth = value; }
        public float CurrentAngle { get => currentAngle; set => currentAngle = value; }

        public void Swap()
        {
            (currentWidth, currentHeight) = (currentHeight, currentWidth);
        }
    }
}
