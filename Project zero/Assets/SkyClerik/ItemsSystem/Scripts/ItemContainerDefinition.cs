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

                // Используем WrapperIndex для получения клона оригинального предмета из GlobalItemStorage
                ItemBaseDefinition originalClone = globalItemStorage.GlobalItemsStorageDefinition.GetClonedItemByIndex(deserializedItem.WrapperIndex);

                if (originalClone != null)
                {
                    JsonScriptableObjectSerializer.CopyJsonProperties(deserializedItem, originalClone);

                    // Копируем данные, которые должны быть специфичными для этого экземпляра предмета
                    //originalClone.WrapperIndex = deserializedItem.WrapperIndex;
                    //originalClone.Stack = deserializedItem.Stack;
                    //originalClone.GridPosition = deserializedItem.GridPosition;
                    // TODO: Добавьте сюда копирование других специфичных для экземпляра полей, если они есть

                    _items.Add(originalClone);
                }
                else
                {
                    Debug.LogError($"Не удалось получить клон предмета с WrapperIndex '{deserializedItem.WrapperIndex}' из GlobalItemStorage. Предмет '{deserializedItem.DefinitionName}' будет отсутствовать.");
                }
            }
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
