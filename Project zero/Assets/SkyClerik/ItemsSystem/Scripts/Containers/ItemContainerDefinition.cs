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
    [JsonObject(MemberSerialization.Fields)]
    public class ItemContainerDefinition : ScriptableObject
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private string _containerGuid;

        public string ContainerGuid { get => _containerGuid; private set => _containerGuid = value; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        [SerializeField]
        [SerializeReference]
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();
        public List<ItemBaseDefinition> Items => _items;

        /// <summary>
        /// Копирует данные о предметах и GUID из другого <see cref="ItemContainerDefinition"/> в текущий.
        /// Используется для загрузки состояния контейнера, сохраняя ссылки на ScriptableObject.
        /// </summary>
        /// <param name="otherContainer">Контейнер-источник, данные которого будут скопированы.</param>
        public void SetDataFromOtherContainer(ItemContainerDefinition otherContainer)
        {
            if (otherContainer == null)
            {
                Debug.LogWarning("Попытка установить данные из пустого (null) контейнера.");
                return;
            }

            _containerGuid = otherContainer.ContainerGuid;

            // Получаем GlobalItemStorage для доступа к оригинальным предметам
            GlobalItemStorage globalItemStorage = ServiceProvider.Get<GlobalItemStorage>();
            if (globalItemStorage == null)
            {
                Debug.LogError("GlobalItemStorage не найден в ServiceProvider! Невозможно загрузить предметы.");
                return;
            }

            _items.Clear();

            foreach (var deserializedItem in otherContainer.Items)
            {
                if (deserializedItem == null)
                {
                    Debug.LogWarning("Десериализованный предмет является null, пропускаем.");
                    continue;
                }

                // Используем itemID для получения клона оригинального предмета из GlobalItemStorage
                ItemBaseDefinition originalClone = globalItemStorage.GlobalItemsStorageDefinition.GetClonedItem(deserializedItem.ID);

                if (originalClone != null)
                {
                    JsonScriptableObjectSerializer.CopyJsonProperties(deserializedItem, originalClone);
                    _items.Add(originalClone);
                }
                else
                {
                    Debug.LogError($"Не удалось получить клон предмета с WrapperIndex '{deserializedItem.ID}' из GlobalItemStorage. Предмет '{deserializedItem.DefinitionName}' будет отсутствовать.");
                }
            }
        }

        /// <summary>
        /// Проверяет и генерирует GUID для контейнера, если он отсутствует.
        /// </summary>
        public void ValidateGuid()
        {
            if (string.IsNullOrEmpty(_containerGuid))
            {
                _containerGuid = Guid.NewGuid().ToString();
                Debug.Log($"Сгенерирован новый GUID для контейнера: {_containerGuid}");
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.Log($"Для контейнера: {_containerGuid} GUID уже валидный");
            }
        }
    }
}
