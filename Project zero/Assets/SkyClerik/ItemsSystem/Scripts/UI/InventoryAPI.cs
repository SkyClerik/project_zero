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
    [RequireComponent(typeof(InventoryStorage))]
    public class InventoryAPI : MonoBehaviour
    {
        private InventoryStorage _inventoryStorage;
        private PlayerItemContainer _playerInventory;

        private void Awake()
        {
            ServiceProvider.Register(this);
            _inventoryStorage = GetComponentInChildren<InventoryStorage>(includeInactive: false);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            SubscribeToEventsForLogging();
#endif
        }

        private void Start()
        {
            _playerInventory = ServiceProvider.Get<PlayerItemContainer>();
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        #region События инвентаря
        /// <summary>
        /// Вызывается, когда предмет успешно выбран в инвентаре.
        /// </summary>
        internal event Action<ItemBaseDefinition> OnItemGiven;

        /// <summary>
        /// Вызывается, когда предмет успешно добавлен в инвентарь игрока.
        /// </summary>
        public event Action<ItemBaseDefinition> OnPlayerItemAdded;

        /// <summary>
        /// Вызывается, когда предмет удален из инвентаря игрока.
        /// </summary>
        public event Action<ItemBaseDefinition> OnPlayerItemRemoved;

        /// <summary>
        /// Вызывается при неудачной попытке добавить предмет (например, нет места).
        /// Передает ItemBaseDefinition, так как полноценный ItemData еще не создан.
        /// </summary>
        public event Action<ItemBaseDefinition> OnPlayerAddItemFailed;

        /// <summary>
        /// Вызывается, когда предмет перемещается между контейнерами.
        /// </summary>
        public event Action<ItemMovedEventArgs> OnItemMoved;

        /// <summary>
        /// Вызывается, когда игрок использует предмет.
        /// </summary>
        public event Action<ItemBaseDefinition> OnItemUsed;

        /// <summary>
        /// Вызывается, когда предмет не найден.
        /// </summary>
        public event Action<int, Type> OnItemFindFall;

        /// <summary>
        /// Вызывается, когда предмет подняли.
        /// </summary>
        public event Action<ItemVisual, GridPageElementBase> OnItemPickUp;

        /// <summary>
        /// Вызывается, когда предмет положили.
        /// </summary>
        public event Action<ItemVisual, GridPageElementBase> OnItemDrop;

        #endregion

        #region Внутренние методы вызова событий

        /// <summary>
        /// Внутренний метод для вызова события OnItemGiven.
        /// </summary>
        public void RiseItemGiveEvent(ItemBaseDefinition item) => OnItemGiven?.Invoke(item);

        /// <summary>
        /// Внутренний метод для вызова события OnPlayerItemAdded.
        /// </summary>
        internal void RaisePlayerItemAdded(ItemBaseDefinition item) => OnPlayerItemAdded?.Invoke(item);

        /// <summary>
        /// Внутренний метод для вызова события OnPlayerItemRemoved.
        /// </summary>
        internal void RaisePlayerItemRemoved(ItemBaseDefinition item) => OnPlayerItemRemoved?.Invoke(item);

        /// <summary>
        /// Внутренний метод для вызова события OnPlayerAddItemFailed.
        /// </summary>
        internal void RaisePlayerAddItemFailed(ItemBaseDefinition itemDef) => OnPlayerAddItemFailed?.Invoke(itemDef);

        /// <summary>
        /// Внутренний метод для вызова события OnItemMoved.
        /// </summary>
        internal void RaiseItemMoved(ItemMovedEventArgs eventArgs) => OnItemMoved?.Invoke(eventArgs);

        /// <summary>
        /// Внутренний метод для вызова события OnItemUsed.
        /// </summary>
        internal void RaiseItemUsed(ItemBaseDefinition item) => OnItemUsed?.Invoke(item);

        /// <summary>
        /// Внутренний метод для вызова события OnItemFindFall.
        /// </summary>
        internal void RiseItemFindFall(int id, Type type) => OnItemFindFall?.Invoke(id, type);

        /// <summary>
        /// Внутренний метод для вызова события OnItemUsed.
        /// </summary>
        internal void RaiseItemPickUp(ItemVisual itemVisual, GridPageElementBase gridPage) => OnItemPickUp?.Invoke(itemVisual, gridPage);

        /// <summary>
        /// Внутренний метод для вызова события OnItemFindFall.
        /// </summary>
        internal void RiseItemDrop(ItemVisual itemVisual, GridPageElementBase gridPage) => OnItemDrop?.Invoke(itemVisual, gridPage);

        #endregion

        /// <summary>
        /// Метод для определения цвета и толщины обводки искомого предмета
        /// </summary>
        public void SetGivinItemTracinColor(Color newColor, int width)
        {
            _inventoryStorage.GivenItem.TracingColor = newColor;
            _inventoryStorage.GivenItem.TracingWidth = width;
        }

        /// <summary>
        /// Указывает, виден ли наш инвентарь.
        /// </summary>
        public bool IsInventoryVisible { get => _inventoryStorage.IsInventoryVisible; set => _inventoryStorage.IsInventoryVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница крафта.
        /// </summary>
        public bool IsCraftVisible { get => _inventoryStorage.IsCraftVisible; set => _inventoryStorage.IsCraftVisible = value; }

        /// <summary>
        /// Указывает, видна ли страница сундука.
        /// </summary>
        public bool IsCheastVisible { get => _inventoryStorage.IsCheastVisible; set => _inventoryStorage.IsCheastVisible = value; }

        /// <summary>
        /// Откроет инвентарь, чтобы выбрать предмет по его ID. Если предмета нет, инвентарь не откроется.
        /// </summary>
        /// <param name="itemID">Индекс искомого предмета.</param>
        public void OpenInventoryFromGiveItem(int itemID, bool tracing) => _inventoryStorage.OpenInventoryFromGiveItem(itemID, tracing);

        /// <summary>
        /// Откроет инвентарь для выбора конкретного предмета. Если ссылка на предмет пустая, инвентарь не откроется.
        /// </summary>
        /// <param name="item">Предмет, который нужно выбрать.</param>
        public void OpenInventoryGiveItem(ItemBaseDefinition item, bool tracing) => _inventoryStorage.OpenInventoryGiveItem(item, tracing);

        /// <summary>
        /// Открывает обычный инвентарь и страницу крафта.
        /// </summary>
        public void OpenInventoryAndCraft() => _inventoryStorage.OpenInventoryAndCraft();

        /// <summary>
        /// Открывает обычный инвентарь и страницу экипировки.
        /// </summary>
        public void OpenInventoryAndEquip() => _inventoryStorage.OpenInventoryAndEquip();

        /// <summary>
        /// Открывает обычный режим отображения инвентаря.
        /// </summary>
        public void OpenInventoryNormal() => _inventoryStorage.OpenInventoryNormal();

        /// <summary>
        /// Открывает страничку сундука.
        /// </summary>
        public void OpenCheast() => _inventoryStorage.OpenCheast();

        /// <summary>
        /// Закрывает вообще все странички UI инвентаря.
        /// </summary>
        public void CloseAll() => _inventoryStorage.CloseAll();


        /// <summary>
        /// Уведомляет систему о том, что предмет был перемещен между контейнерами.
        /// Предназначен для вызова из UI-логики (например, после drag-n-drop).
        /// </summary>
        /// <param name="item">Перемещенный предмет.</param>
        /// <param name="from">Контейнер-источник.</param>
        /// <param name="to">Контейнер-получатель.</param>
        public void NotifyItemMoved(ItemBaseDefinition item, ItemContainer from, ItemContainer to)
        {
            RaiseItemMoved(new ItemMovedEventArgs(item, from, to));
        }

        /// <summary>
        /// Пытается использовать указанный предмет.
        /// Если предмет реализует интерфейс IUsable, выполняет его логику и вызывает событие OnItemUsed.
        /// </summary>
        /// <param name="item">Предмет для использования.</param>
        public void UseItem(ItemBaseDefinition item)
        {
            if (item is IUsable usableItem)
            {
                // Тут может быть дополнительная логика, например, проверка условий использования.
                usableItem.Use();
                RaiseItemUsed(item);
            }
            else
            {
                Debug.LogWarning($"Предмет '{item.DefinitionName}' не является используемым (не реализует IUsable).");
            }
        }


        // --- PlayerItemContainer ---


        public void AddItemsToPlayerInventory(ItemsList itemsList) => _playerInventory.AddItems(itemsList);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// [Только для разработки] Подписывается на все события API и выводит их в консоль для отладки.
        /// </summary>
        private void SubscribeToEventsForLogging()
        {
            OnPlayerItemAdded += (item) => Debug.Log($"<color=cyan>[InventoryAPI]</color> Предмет добавлен: <b>{item.DefinitionName}</b> (ID: {item.ID})");
            OnPlayerItemRemoved += (item) => Debug.Log($"<color=orange>[InventoryAPI]</color> Предмет удален: <b>{item.DefinitionName}</b> (ID: {item.ID})");
            OnPlayerAddItemFailed += (itemDef) => Debug.Log($"<color=red>[InventoryAPI]</color> Не удалось добавить предмет: <b>{itemDef.DefinitionName}</b>");
            OnItemMoved += (args) => Debug.Log($"<color=yellow>[InventoryAPI]</color> Предмет перемещен: <b>{args.Item.DefinitionName}</b> из <i>{args.SourceContainer.name}</i> в <i>{args.DestinationContainer.name}</i>");
            OnItemUsed += (item) => Debug.Log($"<color=green>[InventoryAPI]</color> Предмет использован: <b>{item.DefinitionName}</b> (ID: {item.ID})");
            OnItemFindFall += (id, type) => Debug.Log($"<color=red>[InventoryAPI]</color> Не удалось найти предмет с id: <b>{id}</b> в объекте типа: {type.FullName}");

            Debug.Log("<color=lime>[InventoryAPI]</color> Отладочное логирование событий инвентаря включено.");
        }
#endif
    }
}
