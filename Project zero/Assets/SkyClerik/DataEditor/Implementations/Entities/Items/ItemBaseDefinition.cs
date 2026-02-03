using Newtonsoft.Json;
using System;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// Абстрактный базовый класс для всех предметов.
    /// </summary>
    [System.Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class ItemBaseDefinition : BaseDefinition
    {
        [JsonProperty]
        [SerializeField]
        [Tooltip("Цена предмета в магазинах.")]
        protected int _price;
        public int Price { get => _price; set => _price = value; }

        [JsonProperty]
        [SerializeField]
        [Tooltip("Текущее кол-во")]
        protected int _curStack;
        public int Stack { get => _curStack; set => _curStack = value; }

        [JsonProperty]
        [SerializeField]
        [Tooltip("Максимальное кол-во")]
        protected int _maxStack;
        public int MaxStack { get => _maxStack; set => _maxStack = value; }

        [JsonProperty]
        [SerializeField]
        protected bool _stackable = true;
        public bool Stackable { get => _stackable; set => _stackable = value; }

        [JsonProperty]
        [SerializeField]
        protected bool _viewStackable = true;
        public bool ViewStackable { get => _viewStackable; set => _viewStackable = value; }

        [JsonProperty]
        [SerializeField]
        [Tooltip("Размер и поворот предмета")]
        protected ItemDimensions _dimensions;
        public ItemDimensions Dimensions { get => _dimensions; set => _dimensions = value; }

        [JsonProperty]
        [SerializeField]
        protected Vector2Int _gridPosition;
        public Vector2Int GridPosition { get => _gridPosition; set => _gridPosition = value; }

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
    }

    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class ItemDimensions
    {
        [JsonProperty]
        [SerializeField]
        private int width = 1;
        [JsonProperty]
        [SerializeField]
        private int height = 1;
        [JsonProperty]
        [SerializeField]
        private float angle = 0;

        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public float Angle { get => angle; set => angle = value; }

        public ItemDimensions()
        {
            width = 1;
            height = 1;
            angle = 0;
        }

        public void Swap()
        {
            (width, height) = (height, width);
        }
    }
}
