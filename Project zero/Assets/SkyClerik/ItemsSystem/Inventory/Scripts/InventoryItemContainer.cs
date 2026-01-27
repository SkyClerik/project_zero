using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using Newtonsoft.Json;

namespace Gameplay.Inventory
{
    /// <summary>
    /// Хранилище предметов игрока 
    /// </summary>
    public class InventoryItemContainer : MonoBehaviour
    {
        [SerializeField]
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        private void Awake()
        {
            Initialize(_items);
        }

        public void Initialize(List<ItemBaseDefinition> sourceItems)
        {
            if (sourceItems == null)
                return;
            
            var itemsToClone = sourceItems.ToList(); // 1. Сначала создаем копию.
            _items.Clear(); // 2. Потом очищаем основной список.

            foreach (var item in itemsToClone) // 3. Итерируемся по копии.
            {
                if (item != null)
                {
                    var copy = Object.Instantiate(item);
                    _items.Add(copy); // 4. Наполняем основной список.
                }
            }
        }

        // Добавить предмет в инвентарь как копия
        public void AddItemAsClone(ItemBaseDefinition item)
        {
            if (item != null)
            {
                var copy = Object.Instantiate(item);
                _items.Add(copy);
            }
        }

        // Удалить предмет из инвентаря (по ссылке)
        public bool RemoveItem(ItemBaseDefinition item)
        {
            if (item == null)
                return false;

            bool removed = _items.Remove(item);
            if (removed)
                Object.Destroy(item);

            return removed;
        }

        // Получить список предметов в инвентаре (только для чтения)
        public IReadOnlyList<ItemBaseDefinition> GetItems()
        {
            return _items.AsReadOnly();
        }

        // Очистить инвентарь и уничтожить все копии
        public void Clear()
        {
            // foreach (var item in _inventoryItems)
            // {
            //     Object.Destroy(item); // Удалено, чтобы избежать удаления ассетов ScriptableObject
            // }
            _items.Clear();
        }

        /// <summary>
        /// Сериализует список предметов в JSON строку.
        /// </summary>
        /// <returns>JSON строка, представляющая список предметов.</returns>
        public string SaveItemsToJson()
        {
            return JsonConvert.SerializeObject(_items, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        /// <summary>
        /// Десериализует JSON строку в список предметов.
        /// </summary>
        /// <param name="json">JSON строка для десериализации.</param>
        public void LoadItemsFromJson(string json)
        {
            Clear();

            List<ItemBaseDefinition> deserializedItemsData = JsonConvert.DeserializeObject<List<ItemBaseDefinition>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (deserializedItemsData == null)
                return;

            foreach (var itemData in deserializedItemsData)
            {
                if (itemData != null)
                {
                    ItemBaseDefinition newItemDefinition = ScriptableObject.CreateInstance(itemData.GetType()) as ItemBaseDefinition;
                    if (newItemDefinition != null)
                    {
                        JsonConvert.PopulateObject(JsonConvert.SerializeObject(itemData), newItemDefinition);
                        _items.Add(newItemDefinition);
                    }
                    else
                    {
                        Debug.LogError($"Failed to create ScriptableObject instance for type: {itemData.GetType()}");
                    }
                }
            }
        }
    }
}