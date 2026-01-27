using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;

namespace Gameplay.Inventory
{
    /// <summary>
    /// Хранилище предметов игрока 
    /// </summary>
    public class ItemContainer : MonoBehaviour
    {
        [SerializeField]
        private List<ItemDefinition> _items = new List<ItemDefinition>();

        private void Awake()
        {
            Plugin();
        }

        private void Plugin()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i] = ScriptableObject.Instantiate(_items[i]);
            }
        }

        // Инициализация инвентаря из исходного списка ScriptableObject
        public void Initialize(List<ItemDefinition> sourceItems)
        {
            Clear();

            if (sourceItems == null)
                return;

            foreach (var item in sourceItems)
            {
                if (item != null)
                {
                    var copy = Object.Instantiate(item);
                    _items.Add(copy);
                }
            }
        }

        // Добавить предмет в инвентарь (копия)
        public void AddItem(ItemDefinition item)
        {
            if (item != null)
            {
                var copy = Object.Instantiate(item);
                _items.Add(copy);
            }
        }

        // Удалить предмет из инвентаря (по ссылке)
        public bool RemoveItem(ItemDefinition item)
        {
            if (item == null)
                return false;

            bool removed = _items.Remove(item);
            if (removed)
            {
                Object.Destroy(item);
            }
            return removed;
        }

        // Получить список предметов в инвентаре (только для чтения)
        public IReadOnlyList<ItemDefinition> GetItems()
        {
            return _items.AsReadOnly();
        }

        // Очистить инвентарь и уничтожить все копии
        public void Clear()
        {
            foreach (var item in _items)
            {
                Object.Destroy(item);
            }
            _items.Clear();
        }
    }
}