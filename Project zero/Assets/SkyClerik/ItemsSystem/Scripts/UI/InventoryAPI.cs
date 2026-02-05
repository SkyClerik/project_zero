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
    [RequireComponent(typeof(ItemsPage))]
    public class InventoryAPI : MonoBehaviour
    {
        private ItemsPage _itemsPage;

        private void Awake()
        {
            ServiceProvider.Register(this);
            _itemsPage = GetComponentInChildren<ItemsPage>(includeInactive: false);
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

        /// <summary>
        /// Указывает, виден ли наш инвентарь.
        /// </summary>
        public bool IsInventoryVisible { get => _itemsPage.IsInventoryVisible; set => _itemsPage.IsInventoryVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница крафта.
        /// </summary>
        public bool IsCraftVisible { get => _itemsPage.IsCraftVisible; set => _itemsPage.IsCraftVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница сундука.
        /// </summary>
        public bool IsCheastVisible { get => _itemsPage.IsCheastVisible; set => _itemsPage.IsCheastVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница лута.
        /// </summary>
        public bool IsLutVisible { get => _itemsPage.IsLutVisible; set => _itemsPage.IsLutVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница экипировки.
        /// </summary>
        public bool IsEquipVisible { get => _itemsPage.IsEquipVisible; set => _itemsPage.IsEquipVisible = value; }

        /// <summary>
        /// Откроет инвентарь, чтобы выбрать предмет по его ID. Если предмета нет, инвентарь не откроется.
        /// </summary>
        /// <param name="itemID">Индекс искомого предмета.</param>
        public void OpenInventoryFromGiveItem(int itemID) => _itemsPage.OpenInventoryFromGiveItem(itemID);

        /// <summary>
        /// Откроет инвентарь для выбора конкретного предмета. Если ссылка на предмет пустая, инвентарь не откроется.
        /// </summary>
        /// <param name="item">Предмет, который нужно выбрать.</param>
        public void OpenInventoryGiveItem(ItemBaseDefinition item) => _itemsPage.OpenInventoryGiveItem(item);

        /// <summary>
        /// Открывает обычный инвентарь и страницу крафта.
        /// </summary>
        public void OpenInventoryAndCraft() => _itemsPage.OpenInventoryAndCraft();

        /// <summary>
        /// Открывает обычный режим отображения инвентаря.
        /// </summary>
        public void OpenInventoryNormal() => _itemsPage.OpenInventoryNormal();

        /// <summary>
        /// Открывает страничку сундука.
        /// </summary>
        public void OpenCheast() => _itemsPage.OpenCheast();

        /// <summary>
        /// Открывает страничку лута.
        /// </summary>
        public void OpenLut() => _itemsPage.OpenLut();

        /// <summary>
        /// Открывает страничку экипировки.
        /// </summary>
        public void OpenEquip() => _itemsPage.OpenEquip();

        /// <summary>
        /// Закрывает вообще все странички UI инвентаря.
        /// </summary>
        public void CloseAll() => _itemsPage.CloseAll();
    }
}
