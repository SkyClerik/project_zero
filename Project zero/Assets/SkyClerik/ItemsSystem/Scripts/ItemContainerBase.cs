using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using Newtonsoft.Json;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Хранилище предметов игрока 
    /// </summary>
    public class ItemContainerBase : MonoBehaviour
    {
        [SerializeField]
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        protected virtual void Awake()
        {
            Initialize(_items);
        }

        public void Initialize(List<ItemBaseDefinition> sourceItems)
        {
            if (sourceItems == null)
                return;

            var itemsToClone = sourceItems.ToList();
            _items.Clear();

            foreach (var item in itemsToClone)
            {
                if (item != null)
                {
                    AddItemAsClone(item);
                }
            }
        }

        // Добавить предмет в инвентарь как копия и вернуть его
        public ItemBaseDefinition AddItemAsClone(ItemBaseDefinition item)
        {
            if (item != null)
            {
                var copy = Object.Instantiate(item);
                _items.Add(copy);
                return copy;
            }
            return null;
        }

        // Просто добавить существующий предмет в инвентарь
        public void AddItem(ItemBaseDefinition item)
        {
            if (item == null)
                return;

            if (item.Stackable)
            {
                // Ищем существующий стакуемый предмет того же типа
                ItemBaseDefinition existingStack = _items.FirstOrDefault(i => i.DefinitionName == item.DefinitionName && i.Stackable && i.Stack < i.MaxStack);

                if (existingStack != null)
                {
                    existingStack.AddStack(item.Stack, out int remainder);
                    if (remainder == 0)
                    {
                        // Весь предмет был добавлен в существующий стак, уничтожаем оригинальный предмет
                        Object.Destroy(item); 
                        return;
                    }
                    else
                    {
                        // Часть предмета осталась, обновляем его количество
                        item.Stack = remainder;
                    }
                }
            }
            _items.Add(item);
        }

        // Удалить предмет из инвентаря (по ссылке)
        public bool RemoveItem(ItemBaseDefinition item, bool destroy = true)
        {
            if (item == null)
                return false;

            Debug.Log($"[DIAGNOSTIC] ItemContainerBase.RemoveItem: Attempting to remove '{item.name}'. Parameter 'destroy' is {destroy}.");

            bool removed = _items.Remove(item);
            if (removed && destroy)
            {
                Debug.LogWarning($"[DIAGNOSTIC] ItemContainerBase.RemoveItem: Object '{item.name}' IS BEING DESTROYED.");
                Object.Destroy(item);
            }

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