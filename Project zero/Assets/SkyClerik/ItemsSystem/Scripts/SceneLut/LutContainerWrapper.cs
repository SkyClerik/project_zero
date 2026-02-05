using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Объект, представляющий собой контейнер для лута.
    /// Хранит список предметов, которые могут быть переданы в другой контейнер.
    /// </summary>
    public class LutContainerWrapper
    {
        private ItemsList _itemsList;
        //TODO ПРОВЕРИТЬ РАБОТУ, я его вообще не проверял
        public LutContainerWrapper(List<int> wrapperItemIndexes)
        {
            var playerItemContainer = ServiceProvider.Get<PlayerItemContainer>();

            foreach (var wrapperIndex in wrapperItemIndexes)
            {
                var result = playerItemContainer.GetItemByItemID(wrapperIndex);
                if (result != null)
                    _itemsList.Items.Add(result);
                else
                    Debug.Log($"LutContainerWrapper не смог найти и добавить предмет под индексом {wrapperIndex} в свой локальный контейнер лута");
            }

        }

        // -- Через вызов окна для игрока

        /// <summary>
        /// Попытаться передать указанные предметы в лут игрока.
        /// </summary>
        private void TransferItemsToPlayerLutContainer()
        {
            var inventoryContainersAPI = ServiceProvider.Get<InventoryContainersAPI>();
            inventoryContainersAPI.AddItemsToLutContainer(_itemsList);
        }

        /// <summary>
        /// Открыть окно с лутом для предоставления игроку выбора
        /// </summary>
        public void OpenLutPage()
        {
            TransferItemsToPlayerLutContainer();
            var inventoryAPI = ServiceProvider.Get<InventoryAPI>();
            inventoryAPI.OpenLut();
        }

        // -- Передача всего возможного лута в инвентарь игрока

        /// <summary>
        /// Попытаться передать указанные предметы в инвентарь игрока.
        /// </summary>
        public void TransferItemsToPlayerInventoryContainer()
        {
            var inventoryContainersAPI = ServiceProvider.Get<InventoryContainersAPI>();
            inventoryContainersAPI.AddItemsToPlayerInventory(_itemsList);
        }
    }
}