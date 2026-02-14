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

        public LutContainerWrapper(List<int> itemIndexes)
        {
            if (_itemsList == null)
                _itemsList = new ItemsList();

            var globalItemStorage = ServiceProvider.Get<GlobalItemStorage>();
            foreach (var index in itemIndexes)
            {
                var originalItem = globalItemStorage.GlobalItemsStorageDefinition.GetOriginalItem(index);
                if (originalItem != null)
                    _itemsList.Items.Add(originalItem);
                else
                    Debug.Log($"LutContainerWrapper не смог найти и добавить предмет под индексом {index} в свой локальный контейнер лута");
            }
        }

        public LutContainerWrapper(int itemIndexe)
        {
            if (_itemsList == null)
                _itemsList = new ItemsList();

            var globalItemStorage = ServiceProvider.Get<GlobalItemStorage>();
            var originalItem = globalItemStorage.GlobalItemsStorageDefinition.GetOriginalItem(itemIndexe);
            if (originalItem != null)
                _itemsList.Items.Add(originalItem);
            else
                Debug.Log($"LutContainerWrapper не смог найти и добавить предмет под индексом {itemIndexe} в свой локальный контейнер лута");
        }

        /// <summary>
        /// Попытаться передать указанные предметы в инвентарь игрока.
        /// </summary>
        public void TransferItemsToPlayerInventoryContainer()
        {
            var inventoryAPI = ServiceProvider.Get<InventoryAPI>();
            inventoryAPI.AddItemsToPlayerInventory(_itemsList);
        }
    }
}