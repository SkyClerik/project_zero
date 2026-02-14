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
