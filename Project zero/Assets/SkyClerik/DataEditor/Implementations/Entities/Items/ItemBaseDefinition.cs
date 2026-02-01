using System;
using Newtonsoft.Json;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// Абстрактный базовый класс для всех предметов.
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    public abstract class ItemBaseDefinition : BaseDefinition
    {
        [JsonProperty]
        [SerializeField]
        [Tooltip("Уникальный идентификатор. Индекс полученный от общей базы предметов")]
        private int _wrapperIndex;
        public int WrapperIndex { get => _wrapperIndex; set => _wrapperIndex = value; }

        [JsonProperty]
        [SerializeField]
        [Tooltip("Цена предмета в магазинах.")]
        private int _price;
        public int Price => _price;

        [JsonProperty]
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

        [JsonProperty]
        [SerializeField]
        [Tooltip("Максимальное кол-во")]
        private int _maxStack;
        public int MaxStack => _maxStack;

        [JsonProperty]
        [SerializeField]
        private bool _stackable = true;
        public bool Stackable => _stackable;

        [JsonProperty]
        [SerializeField]
        private bool _viewStackable = true;
        public bool ViewStackable => _viewStackable;

        [JsonProperty]
        [SerializeField]
        [Tooltip("Размер и поворот предмета")]
        private ItemDimensions _dimensions;
        public ItemDimensions Dimensions { get => _dimensions; set => _dimensions = value; }

        [JsonProperty]
        [SerializeField]
        private Vector2Int _gridPosition;
        public Vector2Int GridPosition { get => _gridPosition; set => _gridPosition = value; }
    }

    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class ItemDimensions
    {
        [JsonProperty]
        [SerializeField]
        private int defaultWidth = 1;
        [JsonProperty]
        [SerializeField]
        private int defaultHeight = 1;
        [JsonProperty]
        //[SerializeField]
        private float defaultAngle = 0;
        [JsonProperty]
        //[SerializeField]
        private int currentHeight = 0;
        [JsonProperty]
        //[SerializeField]
        private int currentWidth = 0;
        [JsonProperty]
        //[SerializeField]
        private float currentAngle = 0;

        public int DefaultWidth { get => defaultWidth; set => defaultWidth = value; }
        public int DefaultHeight { get => defaultHeight; set => defaultHeight = value; }
        public float DefaultAngle { get => defaultAngle; set => defaultAngle = value; }
        public int CurrentHeight { get => currentHeight; set => currentHeight = value; }
        public int CurrentWidth { get => currentWidth; set => currentWidth = value; }
        public float CurrentAngle { get => currentAngle; set => currentAngle = value; }

        public ItemDimensions()
        {
            currentWidth = defaultWidth;
            currentHeight = defaultHeight;
            currentAngle = defaultAngle;
        }

        public void Swap()
        {
            (currentWidth, currentHeight) = (currentHeight, currentWidth);
        }
    }
}
