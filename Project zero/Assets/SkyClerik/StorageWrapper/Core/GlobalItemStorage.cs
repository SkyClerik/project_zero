using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Глобальное хранилище данных и префабов всех предметов в игре.
    /// Предоставляет доступ к определениям предметов и их визуальным представлениям.
    /// Регистрируется в <see cref="ServiceProvider"/>.
    /// </summary>
    public class GlobalItemStorage : MonoBehaviour
    {
        [SerializeField]
        private ItemsDataStorageDefinition globalItemsStorageDefinition;
        [SerializeField]
        private ItemPrefabsStorageDefinition itemPrefabsStorageDefinition;

        /// <summary>
        /// Возвращает определение глобального хранилища данных предметов.
        /// </summary>
        public ItemsDataStorageDefinition GlobalItemsStorageDefinition => globalItemsStorageDefinition;
        /// <summary>
        /// Возвращает определение хранилища префабов предметов.
        /// </summary>
        public ItemPrefabsStorageDefinition ItemPrefabsStorageDefinition => itemPrefabsStorageDefinition;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }
    }
}