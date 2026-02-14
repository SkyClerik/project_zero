using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой связку между уникальным ID предмета и его префабом для размещения в игровом мире. Может иметь ссылку на данные.
    /// </summary>
    [System.Serializable]
    public class ItemPrefabMapping
    {
        [SerializeField]
        [Tooltip("Уникальный ID предмета (int). Будет назначен автоматически кнопкой в инспекторе.")]
        private int _itemID = -1;
        /// <summary>
        /// Возвращает уникальный ID предмета.
        /// </summary>
        public int ItemID => _itemID;

        [SerializeField]
        [Tooltip("Префаб объекта, который будет появляться в мире.")]
        private GameObject _worldPrefab;
        /// <summary>
        /// Возвращает префаб GameObject, который будет появляться в мире.
        /// </summary>
        public GameObject WorldPrefab => _worldPrefab;

        [SerializeField]
        [Tooltip("Данные для этого объекта. (Не обязательно имеют ID этого предмета)")]
        private ItemBaseDefinition _definition;
        public ItemBaseDefinition Definition { get => _definition; set => _definition = value; }

        /// <summary>
        /// Внутренний метод для установки ID из редактора.
        /// </summary>
        internal void SetID(int newID)
        {
            _itemID = newID;
        }
    }

    /// <summary>
    /// ScriptableObject для хранения сопоставлений ID предметов с их префабами для игрового мира.
    /// Позволяет получить префаб по ID предмета.
    /// </summary>
    [CreateAssetMenu(fileName = "Item Prefabs Storage Definition", menuName = "SkyClerik/Inventory/Item Prefabs Storage Definition")]
    public class ItemPrefabsStorageDefinition : ScriptableObject
    {
        [SerializeField]
        private List<ItemPrefabMapping> _prefabMappings = new List<ItemPrefabMapping>();

        /// <summary>
        /// Присваивает каждому элементу в списке ID, равный его индексу. Вызывается из редактора.
        /// </summary>
        public void AssignIndexesToIDs()
        {
            for (int i = 0; i < _prefabMappings.Count; i++)
            {
                if (_prefabMappings[i] != null)
                {
                    _prefabMappings[i].SetID(i);
                }
            }
            Debug.Log("[ItemPrefabsStorageDefinition] ID префабов были успешно обновлены по их индексам.");
        }

        /// <summary>
        /// Возвращает префаб для мира по его itemID (int). itemID должен совпадать с индексом в списке.
        /// </summary>
        /// <param name="itemID">Уникальный itemID предмета (int).</param>
        /// <returns>Префаб GameObject или null, если itemID не найден или индекс вне диапазона.</returns>
        public GameObject GetPrefab(int itemID)
        {
            // Проверяем, что itemID является валидным индексом для списка.
            if (itemID >= 0 && itemID < _prefabMappings.Count)
            {
                var mapping = _prefabMappings[itemID];
                if (mapping != null && mapping.WorldPrefab != null)
                {
                    return mapping.WorldPrefab;
                }
                Debug.LogWarning($"[ItemPrefabsStorageDefinition] Предмет по ID '{itemID}' найден, но его префаб или сам мэппинг равен null.");
                return null;
            }

            Debug.LogWarning($"[ItemPrefabsStorageDefinition] Префаб с ID '{itemID}' не найден (индекс вне диапазона).");
            return null;
        }
    }
}
