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
            var inventoryContainersAPI = ServiceProvider.Get<InventoryContainersAPI>();
            inventoryContainersAPI.AddItemsToLutContainer(_itemsList);
        }

        /// <summary>
        /// Открыть окно с лутом для предоставления игроку выбора
        /// </summary>
        public void OpenLutPage()
        {
            //TODO Обязательно не забыть что лут нужно возвращать при закрытии окна
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
