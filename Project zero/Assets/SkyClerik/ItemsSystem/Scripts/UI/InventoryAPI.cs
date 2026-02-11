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
    [RequireComponent(typeof(InventoryContainer))]
    public class InventoryAPI : MonoBehaviour
    {
        private InventoryContainer _itemsPage;

        private void Awake()
        {
            ServiceProvider.Register(this);
            _itemsPage = GetComponentInChildren<InventoryContainer>(includeInactive: false);
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
            _itemsPage.GivenItem.TracingColor = newColor;
            _itemsPage.GivenItem.TracingWidth = width;
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
        /// Откроет инвентарь, чтобы выбрать предмет по его ID. Если предмета нет, инвентарь не откроется.
        /// </summary>
        /// <param name="itemID">Индекс искомого предмета.</param>
        public void OpenInventoryFromGiveItem(int itemID, bool tracing) => _itemsPage.OpenInventoryFromGiveItem(itemID, tracing);

        /// <summary>
        /// Откроет инвентарь для выбора конкретного предмета. Если ссылка на предмет пустая, инвентарь не откроется.
        /// </summary>
        /// <param name="item">Предмет, который нужно выбрать.</param>
        public void OpenInventoryGiveItem(ItemBaseDefinition item, bool tracing) => _itemsPage.OpenInventoryGiveItem(item, tracing);

        /// <summary>
        /// Открывает обычный инвентарь и страницу крафта.
        /// </summary>
        public void OpenInventoryAndCraft() => _itemsPage.OpenInventoryAndCraft();

        /// <summary>
        /// Открывает обычный инвентарь и страницу экипировки.
        /// </summary>
        public void OpenInventoryAndEquip() => _itemsPage.OpenInventoryAndEquip();

        /// <summary>
        /// Открывает обычный режим отображения инвентаря.
        /// </summary>
        public void OpenInventoryNormal() => _itemsPage.OpenInventoryNormal();

        /// <summary>
        /// Открывает страничку сундука.
        /// </summary>
        public void OpenCheast() => _itemsPage.OpenCheast();

        /// <summary>
        /// Закрывает вообще все странички UI инвентаря.
        /// </summary>
        public void CloseAll() => _itemsPage.CloseAll();
    }
}
