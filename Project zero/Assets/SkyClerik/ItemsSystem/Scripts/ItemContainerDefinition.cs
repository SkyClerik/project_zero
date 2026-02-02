using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// ScriptableObject для хранения списка ItemBaseDefinition и GUID контейнера.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemContainerDefinition", menuName = "SkyClerik/Inventory/Item Container Definition")]
    [System.Serializable]
    public class ItemContainerDefinition : ScriptableObject
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private string _containerGuid;
        public string ContainerGuid { get => _containerGuid; private set => _containerGuid = value; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)] // Явно указываем, как обрабатывать типы элементов в списке
        [SerializeField]
        [SerializeReference] // Указываем Unity, что это полиморфный список
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        [JsonIgnore] // Добавлено для игнорирования публичного свойства при сериализации Newtonsoft.Json
        public List<ItemBaseDefinition> Items => _items;

        public void SetDataFromOtherContainer(ItemContainerDefinition otherContainer)
        {
            if (otherContainer == null)
            {
                Debug.LogWarning("Попытка установить данные из пустого (null) контейнера.");
                return;
            }

            _containerGuid = otherContainer.ContainerGuid;

            _items.Clear();
            _items.AddRange(otherContainer.Items);
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
    }
}
