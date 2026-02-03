using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
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
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        // -- Через вызов окна для игрока

        /// <summary>
        /// Попытаться передать указанные предметы в лут игрока.
        /// </summary>
        private void TransferItemsToPlayerLutContainer()
        {
            var playerLutContainer = ServiceProvider.Get<PlayerLutContainer>();
            playerLutContainer.AddItems(ref _items, container: this);
        }

        /// <summary>
        /// Попытаться передать указанные предметы в лут игрока.
        /// </summary>
        public void OpenLutPage()
        {
            TransferItemsToPlayerLutContainer();

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            itemsPage.OpenInventoryNormal();
            itemsPage.OpenLut();
        }

        // -- Передача всего возможного лута в инвентарь игрока

        /// <summary>
        /// Попытаться передать указанные предметы в лут игрока.
        /// </summary>
        public void TransferItemsToPlayerInventoryContainer()
        {
            var playerItemContainer = ServiceProvider.Get<PlayerItemContainer>();
            playerItemContainer.AddItems(ref _items);
        }
    }
}
