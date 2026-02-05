using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Фасад (API) для управления всеми контейнерами инвентаря игрока:
    /// основного инвентаря, контейнера крафта, сундука и лута.
    /// Предоставляет централизованный доступ к основным операциям с предметами
    /// для каждого типа контейнера. Регистрируется в <see cref="ServiceProvider"/>.
    /// </summary>
    public class InventoryContainersAPI : MonoBehaviour
    {
        private PlayerItemContainer _playerInventory;
        private PlayerLutContainer _lutContainer;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            _playerInventory = ServiceProvider.Get<PlayerItemContainer>();
            _lutContainer = ServiceProvider.Get<PlayerLutContainer>();
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        public void AddItemsToPlayerInventory(ItemsList itemsList) => _playerInventory.AddItems(itemsList);
        public void AddItemsToLutContainer(ItemsList itemsList) => _lutContainer.AddItems(itemsList);
    }
}
