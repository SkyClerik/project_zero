using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using Newtonsoft.Json;

namespace Gameplay.Inventory
{
    /// <summary>
    /// Крафт предметов игрока 
    /// </summary>
    public class CraftItemsContainer : MonoBehaviour
    {
        [SerializeField]
        private List<ItemDefinition> _items = new List<ItemDefinition>();

        private void Awake()
        {
            Initialize(_items);
        }

        private void Initialize(List<ItemDefinition> sourceItems)
        {
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

        // Добавить предмет в инвентарь как копия
        public void AddItemAsClone(ItemDefinition item)
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
                Object.Destroy(item);

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
                Object.Destroy(item); // Удалено, чтобы избежать удаления ассетов ScriptableObject
            }
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

            List<ItemDefinition> deserializedItemsData = JsonConvert.DeserializeObject<List<ItemDefinition>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (deserializedItemsData == null)
                return;

            foreach (var itemData in deserializedItemsData)
            {
                if (itemData != null)
                {
                    ItemDefinition newItemDefinition = ScriptableObject.CreateInstance(itemData.GetType()) as ItemDefinition;
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