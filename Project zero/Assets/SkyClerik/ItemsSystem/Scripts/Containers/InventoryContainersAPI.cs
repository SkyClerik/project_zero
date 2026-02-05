using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.DataEditor;

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
        //private PlayerCraftContainer _craftContainer;
        //private PlayerChestContainer _chestContainer;
        private PlayerLutContainer _lutContainer;

        /// <summary>
        /// Возвращает экземпляр PlayerItemContainer.
        /// </summary>
        //public PlayerItemContainer PlayerInventory => _playerInventory;
        /// <summary>
        /// Возвращает экземпляр PlayerCraftContainer.
        /// </summary>
        //public PlayerCraftContainer CraftContainer => _craftContainer;
        /// <summary>
        /// Возвращает экземпляр PlayerChestContainer.
        /// </summary>
        //public PlayerChestContainer ChestContainer => _chestContainer;
        /// <summary>
        /// Возвращает экземпляр PlayerLutContainer.
        /// </summary>
        //public PlayerLutContainer LutContainer => _lutContainer;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            // Получаем экземпляры контейнеров из ServiceProvider.
            // Предполагается, что эти контейнеры уже зарегистрированы (они являются MonoBehaviour и делают это в своем Awake).
            _playerInventory = ServiceProvider.Get<PlayerItemContainer>();
            //_craftContainer = ServiceProvider.Get<PlayerCraftContainer>();
            //_chestContainer = ServiceProvider.Get<PlayerChestContainer>();
            _lutContainer = ServiceProvider.Get<PlayerLutContainer>();
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        // --- Методы для PlayerInventory ---
        public void AddItemsToPlayerInventory(ItemsList itemsList) => _playerInventory.AddItems(itemsList);
        //public List<ItemBaseDefinition> AddClonedItemsToPlayerInventory(List<ItemBaseDefinition> itemTemplates) => _playerInventory.AddClonedItems(itemTemplates);
        //public bool RemoveItemFromPlayerInventory(ItemBaseDefinition item, bool destroy = true) => _playerInventory.RemoveItem(item, destroy);
        //public void ClearPlayerInventory() => _playerInventory.Clear();
        //public IReadOnlyList<ItemBaseDefinition> GetPlayerInventoryItems() => _playerInventory.GetItems();
        //public ItemBaseDefinition GetPlayerInventoryItemByItemID(int wrapperIndex) => _playerInventory.GetItemByItemID(wrapperIndex);
        //public bool TryAddItemToPlayerInventoryAtPosition(ItemBaseDefinition item, Vector2Int gridPosition) => _playerInventory.TryAddItemAtPosition(item, gridPosition);
        //public void MovePlayerInventoryItem(ItemBaseDefinition item, Vector2Int newPosition) => _playerInventory.MoveItem(item, newPosition);

        // --- Методы для CraftContainer ---
        //public List<ItemBaseDefinition> AddItemsToCraftContainer(List<ItemBaseDefinition> itemsToAdd) => _craftContainer.AddItems(itemsToAdd);
        //public List<ItemBaseDefinition> AddClonedItemsToCraftContainer(List<ItemBaseDefinition> itemTemplates) => _craftContainer.AddClonedItems(itemTemplates);
        //public bool RemoveItemFromCraftContainer(ItemBaseDefinition item, bool destroy = true) => _craftContainer.RemoveItem(item, destroy);
        //public void ClearCraftContainer() => _craftContainer.Clear();
        //public IReadOnlyList<ItemBaseDefinition> GetCraftContainerItems() => _craftContainer.GetItems();
        //public ItemBaseDefinition GetCraftContainerItemByItemID(int wrapperIndex) => _craftContainer.GetItemByItemID(wrapperIndex);
        //public bool TryAddItemToCraftContainerAtPosition(ItemBaseDefinition item, Vector2Int gridPosition) => _craftContainer.TryAddItemAtPosition(item, gridPosition);
        //public void MoveCraftContainerItem(ItemBaseDefinition item, Vector2Int newPosition) => _craftContainer.MoveItem(item, newPosition);

        // --- Методы для ChestContainer ---
        //public List<ItemBaseDefinition> AddItemsToChestContainer(List<ItemBaseDefinition> itemsToAdd) => _chestContainer.AddItems(itemsToAdd);
        //public List<ItemBaseDefinition> AddClonedItemsToChestContainer(List<ItemBaseDefinition> itemTemplates) => _chestContainer.AddClonedItems(itemTemplates);
        //public bool RemoveItemFromChestContainer(ItemBaseDefinition item, bool destroy = true) => _chestContainer.RemoveItem(item, destroy);
        //public void ClearChestContainer() => _chestContainer.Clear();
        //public IReadOnlyList<ItemBaseDefinition> GetChestContainerItems() => _chestContainer.GetItems();
        //public ItemBaseDefinition GetChestContainerItemByItemID(int wrapperIndex) => _chestContainer.GetItemByItemID(wrapperIndex);
        //public bool TryAddItemToChestContainerAtPosition(ItemBaseDefinition item, Vector2Int gridPosition) => _chestContainer.TryAddItemAtPosition(item, gridPosition);
        //public void MoveChestContainerItem(ItemBaseDefinition item, Vector2Int newPosition) => _chestContainer.MoveItem(item, newPosition);

        // --- Методы для LutContainer ---
        public void AddItemsToLutContainer(ItemsList itemsList) => _lutContainer.AddItems(itemsList);
        //public List<ItemBaseDefinition> AddClonedItemsToLutContainer(List<ItemBaseDefinition> itemTemplates) => _lutContainer.AddClonedItems(itemTemplates);
        //public bool RemoveItemFromLutContainer(ItemBaseDefinition item, bool destroy = true) => _lutContainer.RemoveItem(item, destroy);
        //public void ClearLutContainer() => _lutContainer.Clear();
        //public IReadOnlyList<ItemBaseDefinition> GetLutContainerItems() => _lutContainer.GetItems();
        //public ItemBaseDefinition GetLutContainerItemByItemID(int wrapperIndex) => _lutContainer.GetItemByItemID(wrapperIndex);
        //public bool TryAddItemToLutContainerAtPosition(ItemBaseDefinition item, Vector2Int gridPosition) => _lutContainer.TryAddItemAtPosition(item, gridPosition);
        //public void MoveLutContainerItem(ItemBaseDefinition item, Vector2Int newPosition) => _lutContainer.MoveItem(item, newPosition);
    }
}
