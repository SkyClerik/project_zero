using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Компонент, представляющий собой контейнер для лута.
    /// Хранит список предметов, которые могут быть переданы в другой контейнер.
    /// </summary>
    public class LutContainer : MonoBehaviour
    {
        [SerializeField]
        private ItemsList _itemsList;

        // -- Через вызов окна для игрока

        /// <summary>
        /// Попытаться передать указанные предметы в лут игрока.
        /// </summary>
        private void TransferItemsToPlayerLutContainer()
        {
            var playerLutContainer = ServiceProvider.Get<PlayerLutContainer>();
            playerLutContainer.AddItems(_itemsList);
        }

        /// <summary>
        /// Открыть окно с лутом для предоставления игроку выбора
        /// </summary>
        public void OpenLutPage()
        {
            TransferItemsToPlayerLutContainer();

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            itemsPage.OpenLut();
        }

        // -- Передача всего возможного лута в инвентарь игрока

        /// <summary>
        /// Попытаться передать указанные предметы в инвентарь игрока.
        /// </summary>
        public void TransferItemsToPlayerInventoryContainer()
        {
            var playerItemContainer = ServiceProvider.Get<PlayerItemContainer>();
            playerItemContainer.AddItems(_itemsList);
        }
    }
}
