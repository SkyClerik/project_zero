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

        public ItemsList ItemsList { get => _itemsList; }

        //TODO ПРОВЕРИТЬ РАБОТУ, я его вообще не проверял
        public LutContainerWrapper(List<int> wrapperItemIndexes)
        {
            if (_itemsList == null)
                _itemsList = new ItemsList();

            var globalItemStorage = ServiceProvider.Get<GlobalItemStorage>();
            foreach (var wrapperIndex in wrapperItemIndexes)
            {
                var originalItem = globalItemStorage.GlobalItemsStorageDefinition.GetOriginalItem(wrapperIndex);
                if (originalItem != null)
                    _itemsList.Items.Add(originalItem);
                else
                    Debug.Log($"LutContainerWrapper не смог найти и добавить предмет под индексом {wrapperIndex} в свой локальный контейнер лута");
            }
        }

        public LutContainerWrapper(int wrapperItemIndexe)
        {
            if (_itemsList == null)
                _itemsList = new ItemsList();

            var globalItemStorage = ServiceProvider.Get<GlobalItemStorage>();
            var originalItem = globalItemStorage.GlobalItemsStorageDefinition.GetOriginalItem(wrapperItemIndexe);
            if (originalItem != null)
                _itemsList.Items.Add(originalItem);
            else
                Debug.Log($"LutContainerWrapper не смог найти и добавить предмет под индексом {wrapperItemIndexe} в свой локальный контейнер лута");
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