using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// ScriptableObject для хранения списка ItemBaseDefinition и GUID контейнера.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemDataStorage", menuName = "SkyClerik/Inventory/Item Data Storage")]
    public class ItemDataStorageSO : ScriptableObject
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private string _containerGuid;
        public string ContainerGuid { get => _containerGuid; private set => _containerGuid = value; }

        [SerializeField]
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        public List<ItemBaseDefinition> Items => _items;

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

        public string SaveItemsToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new SpriteJsonConverter() }
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public void LoadItemsFromJson(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new SpriteJsonConverter() }
            };

            // Deserialize into a temporary, anonymous type to get GUID and item data separately
            var tempAnonObject = JsonConvert.DeserializeAnonymousType(json, new { ContainerGuid = "", Items = new List<object>() }, settings);

            if (tempAnonObject == null) return;

            _items.Clear();
            foreach (var itemJson in tempAnonObject.Items)
            {
                // Serialize itemJson back to string to deserialize into a new instance
                // This ensures we get a fresh object, not a reference to an existing asset
                string individualItemJson = JsonConvert.SerializeObject(itemJson, settings);

                // Use TypeNameHandling.Auto for deserializing individual items to correctly create instances of derived types
                ItemBaseDefinition newItemDefinition = JsonConvert.DeserializeObject<ItemBaseDefinition>(individualItemJson, settings);

                if (newItemDefinition != null)
                {
                    _items.Add(newItemDefinition);
                }
                else
                {
                    Debug.LogError($"Failed to deserialize ItemBaseDefinition from JSON: {individualItemJson}");
                }
            }
            _containerGuid = tempAnonObject.ContainerGuid;
        }
    }
}
