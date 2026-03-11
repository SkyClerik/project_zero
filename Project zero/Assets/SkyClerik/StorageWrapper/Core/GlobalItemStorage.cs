using Assets.SimpleLocalization.Scripts;
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
        private ItemsDataStorageDefinition _globalItemsStorageDefinition;

        [SerializeField]
        private ItemPrefabsStorageDefinition _itemPrefabsStorageDefinition;

        private const string _localizationPrefixName = "Item.name.";
        private const string _localizationPrefixDesc = "Item.desc.";

        /// <summary>
        /// Возвращает определение глобального хранилища данных предметов.
        /// </summary>
        public ItemsDataStorageDefinition GlobalItemsStorageDefinition => _globalItemsStorageDefinition;
        /// <summary>
        /// Возвращает определение хранилища префабов предметов.
        /// </summary>
        public ItemPrefabsStorageDefinition ItemPrefabsStorageDefinition => _itemPrefabsStorageDefinition;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            LocalizationChanged();
            LocalizationManager.OnLocalizationChanged += LocalizationChanged;
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
            LocalizationManager.OnLocalizationChanged -= LocalizationChanged;
        }

        private void LocalizationChanged()
        {
            foreach (var item in _globalItemsStorageDefinition.BaseDefinitions)
            {
                item.DefinitionName = LocalizationManager.Localize($"{_localizationPrefixName}{item.ID}");
                item.Description = LocalizationManager.Localize($"{_localizationPrefixDesc}{item.ID}");
            }
        }
    }
}