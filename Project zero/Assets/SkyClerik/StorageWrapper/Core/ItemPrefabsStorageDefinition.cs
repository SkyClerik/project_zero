using System.Collections.Generic;
using UnityEngine;

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public class ItemPrefabMapping
    {
        [SerializeField]
        [Tooltip("Уникальный ID предмета (int). Будет назначен автоматически кнопкой в инспекторе.")]
        private int _itemID = -1;
        public int ItemID => _itemID;

        [SerializeField]
        [Tooltip("Префаб объекта, который будет появляться в мире.")]
        private GameObject _worldPrefab;
        public GameObject WorldPrefab => _worldPrefab;

        /// <summary>
        /// Внутренний метод для установки ID из редактора.
        /// </summary>
        internal void SetID(int newID)
        {
            _itemID = newID;
        }
    }

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
        /// Возвращает префаб для мира по его ID (int). ID должен совпадать с индексом в списке.
        /// </summary>
        /// <param name="itemID">Уникальный ID предмета (int).</param>
        /// <returns>Префаб GameObject или null, если ID не найден или индекс вне диапазона.</param>
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
