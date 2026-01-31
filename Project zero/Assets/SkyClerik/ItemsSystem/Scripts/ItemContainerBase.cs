using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using Newtonsoft.Json;
using System;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Хранилище предметов игрока 
    /// </summary>
    public class ItemContainerBase : MonoBehaviour
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private string _containerGuid;
        public string ContainerGuid { get => _containerGuid; private set => _containerGuid = value; }

        [SerializeField]
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        protected virtual void Awake()
        {
            ValidateGuid();
            Initialize(_items);
        }

        /// <summary>
        /// Проверяет и генерирует GUID для контейнера, если он отсутствует.
        /// Может быть вызван вручную через контекстное меню.
        /// </summary>
        [ContextMenu("Validate GUID")]
        public void ValidateGuid()
        {
            if (string.IsNullOrEmpty(_containerGuid))
            {
                _containerGuid = Guid.NewGuid().ToString();
                Debug.Log($"Сгенерирован новый GUID для контейнера: {_containerGuid} (из ValidateGuid)");
            }
            else
            {
                Debug.Log($"GUID контейнера уже существует: {_containerGuid}");
            }
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

        public ItemBaseDefinition AddItemAsClone(ItemBaseDefinition item)
        {
            if (item != null)
            {
                var copy = UnityEngine.Object.Instantiate(item);
                _items.Add(copy);
                return copy;
            }
            return null;
        }

        public void AddItem(ItemBaseDefinition item)
        {
            if (item == null)
                return;

            if (item.Stackable)
            {
                ItemBaseDefinition existingStack = _items.FirstOrDefault(i => i.DefinitionName == item.DefinitionName && i.Stackable && i.Stack < i.MaxStack);

                if (existingStack != null)
                {
                    existingStack.AddStack(item.Stack, out int remainder);
                    if (remainder == 0)
                    {
                        UnityEngine.Object.Destroy(item); 
                        return;
                    }
                    else
                    {
                        item.Stack = remainder;
                    }
                }
            }
            _items.Add(item);
        }

        public void AddItemReference(ItemBaseDefinition item)
        {
            if (item != null)
            {
                _items.Add(item);
            }
        }

        public bool RemoveItem(ItemBaseDefinition item, bool destroy = true)
        {
            if (item == null)
                return false;

            bool removed = _items.Remove(item);
            if (removed && destroy)
            {
                UnityEngine.Object.Destroy(item);
            }

            return removed;
        }

        public IReadOnlyList<ItemBaseDefinition> GetItems()
        {
            return _items.AsReadOnly();
        }

        public void Clear()
        {
            _items.Clear();
        }

        public string SaveItemsToJson()
        {
            return JsonConvert.SerializeObject(_items, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

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