using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.DataEditor;
using System;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Фасад для управления UI инвентаря. Предоставляет упрощенный доступ к основным
    /// функциям взаимодействия с инвентарем, крафтом, сундуком, лутом и экипировкой.
    /// Регистрируется в <see cref="ServiceProvider"/>.
    /// </summary>
    [RequireComponent(typeof(InventoryStorage))]
    public class InventoryAPI : MonoBehaviour
    {
        private InventoryStorage _inventoryStorage;
        private PlayerItemContainer _playerInventory;

        private void Awake()
        {
            ServiceProvider.Register(this);
            _inventoryStorage = GetComponentInChildren<InventoryStorage>(includeInactive: false);
        }

        private void Start()
        {
            _playerInventory = ServiceProvider.Get<PlayerItemContainer>();
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        internal event Action<ItemBaseDefinition> OnItemGiven;

        public void RiseItemGiveEvent(ItemBaseDefinition item)
        {
            OnItemGiven?.Invoke(item);
        }

        public void SetGivinItemTracinColor(Color newColor, int width)
        {
            _inventoryStorage.GivenItem.TracingColor = newColor;
            _inventoryStorage.GivenItem.TracingWidth = width;
        }

        /// <summary>
        /// Указывает, виден ли наш инвентарь.
        /// </summary>
        public bool IsInventoryVisible { get => _inventoryStorage.IsInventoryVisible; set => _inventoryStorage.IsInventoryVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница крафта.
        /// </summary>
        public bool IsCraftVisible { get => _inventoryStorage.IsCraftVisible; set => _inventoryStorage.IsCraftVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница сундука.
        /// </summary>
        public bool IsCheastVisible { get => _inventoryStorage.IsCheastVisible; set => _inventoryStorage.IsCheastVisible = value; }

        /// <summary>
        /// Откроет инвентарь, чтобы выбрать предмет по его ID. Если предмета нет, инвентарь не откроется.
        /// </summary>
        /// <param name="itemID">Индекс искомого предмета.</param>
        public void OpenInventoryFromGiveItem(int itemID, bool tracing) => _inventoryStorage.OpenInventoryFromGiveItem(itemID, tracing);

        /// <summary>
        /// Откроет инвентарь для выбора конкретного предмета. Если ссылка на предмет пустая, инвентарь не откроется.
        /// </summary>
        /// <param name="item">Предмет, который нужно выбрать.</param>
        public void OpenInventoryGiveItem(ItemBaseDefinition item, bool tracing) => _inventoryStorage.OpenInventoryGiveItem(item, tracing);

        /// <summary>
        /// Открывает обычный инвентарь и страницу крафта.
        /// </summary>
        public void OpenInventoryAndCraft() => _inventoryStorage.OpenInventoryAndCraft();

        /// <summary>
        /// Открывает обычный инвентарь и страницу экипировки.
        /// </summary>
        public void OpenInventoryAndEquip() => _inventoryStorage.OpenInventoryAndEquip();

        /// <summary>
        /// Открывает обычный режим отображения инвентаря.
        /// </summary>
        public void OpenInventoryNormal() => _inventoryStorage.OpenInventoryNormal();

        /// <summary>
        /// Открывает страничку сундука.
        /// </summary>
        public void OpenCheast() => _inventoryStorage.OpenCheast();

        /// <summary>
        /// Закрывает вообще все странички UI инвентаря.
        /// </summary>
        public void CloseAll() => _inventoryStorage.CloseAll();


        // --- PlayerItemContainer ---


        public void AddItemsToPlayerInventory(ItemsList itemsList) => _playerInventory.AddItems(itemsList);
    }
}
