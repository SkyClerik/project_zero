using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;
using SkyClerik.Inventory;
using System;

namespace SkyClerik.EquipmentSystem
{
    [Serializable]
    public class EquipmentSlot : IDropTarget
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [SerializeField]
        [SerializeReference]
        private ItemBaseDefinition _equippedItem;

        [JsonProperty]
        [SerializeField]
        [ReadOnly] // Заполняется методом CalculateGridDimensionsFromUI
        private Rect _rect;

        [SerializeField]
        private ItemVisual _itemVisual;

        [SerializeField]
        [ReadOnly]
        private string _cellNameDebug;


        private VisualElement _cell;
        private Telegraph _telegraph;
        private UIDocument _document;

        public UIDocument GetDocument => _document;
        public Vector2 CellSize => _rect.size;
        public Rect Rect => _rect;
        public bool IsEmpty => _equippedItem == null;
        public ItemBaseDefinition EquippedItem => _equippedItem;
        public ItemVisual ItemVisual => _itemVisual; 

        public VisualElement Cell
        {
            get => _cell;
            set
            {
                _cell = value;
                _cellNameDebug = _cell.name;
            }
        }

        public EquipmentSlot(Rect rect, UIDocument document)
        {
            _rect = rect;
            _document = document;
        }

        public void InitializeDocumentAndTelegraph(UIDocument document)
        {
            _document = document;
            _telegraph = new Telegraph();
            document.rootVisualElement.Add(_telegraph);
        }

        /// <summary>
        /// Проверяет, подходит ли данный предмет для экипировки в этот слот.
        /// </summary>
        /// <param name="item">Предмет для проверки.</param>
        /// <returns>True, если предмет подходит; иначе false.</returns>
        public bool CanEquip(ItemBaseDefinition item)
        {
            if (item == null)
                return false;
            // TODO заглушка на проверке экипируемого предмета, надо решить на что проверять
            return true;
        }

        /// <summary>
        /// Экипирует предмет в этот слот.
        /// </summary>
        /// <param name="item">Предмет для экипировки.</param>
        public void Equip(ItemVisual itemVisual)
        {
            if (itemVisual == null)
                return;

            IDropTarget sourceOwner = itemVisual.OwnerInventory;
            if (sourceOwner is GridPageElementBase sourceGridPage)
            {
                ItemContainer sourceContainer = sourceGridPage.ItemContainer;
                if (sourceContainer != null)
                {
                    Debug.Log($"[EquipmentSlot][Equip] Удаляю '{itemVisual.ItemDefinition.name}' из исходного инвентаря '{sourceContainer.name}'.");
                    sourceContainer.RemoveItem(itemVisual.ItemDefinition, destroy: false);
                }
            }

            _equippedItem = itemVisual.ItemDefinition;
            _itemVisual = itemVisual;

            _cell.Add(itemVisual);
            _telegraph.Hide();
            
            itemVisual.style.left = 0;
            itemVisual.style.top = 0;
            itemVisual.style.position = Position.Absolute;

            ItemsPage.CurrentDraggedItem = null;
        }

        /// <summary>
        /// Создает и экипирует ItemVisual для данного слота.
        /// </summary>
        /// <param name="itemDefinition">Определение предмета.</param>
        /// <param name="itemsPage">Ссылка на ItemsPage.</param>
        /// <returns>Созданный ItemVisual.</returns>
        public ItemVisual CreateItemVisualForSlot(ItemBaseDefinition itemDefinition, ItemsPage itemsPage)
        {
            // Здесь мы используем существующий конструктор ItemVisual.
            // Параметры ownerInventory, gridPosition и gridSize - это заглушки,
            // так как ItemVisual для слотов экипировки не управляется напрямую сеткой инвентаря.
            return new ItemVisual(
                itemsPage: itemsPage,
                ownerInventory: this, // САМ СЛОТ ЭКИПИРОВКИ является ownerInventory
                itemDefinition: itemDefinition,
                gridPosition: Vector2Int.zero, // Заглушка
                gridSize: Vector2Int.zero // Заглушка
            );
        }

        /// <summary>
        /// Снимает предмет из этого слота.
        /// </summary>
        /// <returns>Снятый ItemBaseDefinition, или null, если слот был пуст.</returns>
        public ItemBaseDefinition Unequip()
        {
            if (_itemVisual != null)
            {
                _itemVisual.RemoveFromHierarchy();
                _itemVisual = null;
            }

            _equippedItem = null;
            Debug.Log($"[ЭКИПИРОВКА][EquipmentSlot] _equippedItem обнулен для слота: {_cellNameDebug}.");

            return null;
        }

        // --- IDropTarget реализация ---

        public PlacementResults ShowPlacementTarget(ItemVisual itemVisual)
        {
            // Здесь мы должны проверить, может ли itemVisual быть экипирован в этот слот.
            // Используем существующий метод CanEquip.
            bool canEquip = CanEquip(itemVisual.ItemDefinition);
            ReasonConflict conflict = ReasonConflict.invalidSlotType;

            if (canEquip)
            {
                if (_itemVisual == null)
                {
                    conflict = ReasonConflict.None;
                }
                else
                {
                    Debug.Log($"[ShowPlacementTarget] _itemVisual.ItemDefinition.name: {_itemVisual.ItemDefinition.name}.");
                    conflict = ReasonConflict.SwapAvailable;
                }
            }

            // Отображаем телеграф на позиции и размере слота
            // _rect.position уже содержит корректную позицию Rect слота в мировых координатах.
            // Телеграф является дочерним элементом rootVisualElement, поэтому его позиция style.left/top
            // должна быть равна _rect.position, чтобы он появился над слотом.
            _telegraph.SetPosition(_rect.position); // Устанавливаем позицию телеграфа напрямую в мировые координаты
            _telegraph.SetPlacement(conflict, _rect.size.x, _rect.size.y);

            // Инициализируем PlacementResults с данными слота
            return new PlacementResults().Init(
                conflict: conflict,
                position: _rect.position, // Позиция Rect слота
                suggestedGridPosition: Vector2Int.zero, // Не используется для экипировки
                overlapItem: _itemVisual == null ? null : _itemVisual, // Возвращаем ItemVisual, если слот занят
                targetInventory: this // Сам EquipmentSlot является целью перетаскивания
            );
        }

        public void FinalizeDrag()
        {
            _telegraph.Hide();
        }

        public void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)
        {
            //Equip(storedItem); // Просто вызываем Equip
        }

        public void RemoveStoredItem(ItemVisual storedItem)
        {
            Unequip(); // Просто вызываем Unequip
        }

        public void PickUp(ItemVisual storedItem)
        {
            Debug.Log($"[ItemVisual][PickUp] storedItem: {storedItem.ItemDefinition.name}");
            Unequip();
            ItemsPage.CurrentDraggedItem = storedItem;
            _document.rootVisualElement.Add(storedItem);
            ItemsPage.CurrentDraggedItem.SetOwnerInventory(this);
            Debug.Log($"[EquipmentSlot][PickUp] Установлен ItemsPage.CurrentDraggedItem: {ItemsPage.CurrentDraggedItem.ItemDefinition.name}.");
        }

        public void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            Equip(storedItem); // Просто вызываем Equip
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            // ItemVisual уже добавляется к _cell в методе Equip
            // Этот метод может быть пустым
        }

        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            suggestedGridPosition = Vector2Int.zero; // Не используется для слотов экипировки

            return _itemVisual == null && CanEquip(item);
        }

        public ItemGridData GetItemGridData(ItemVisual itemVisual)
        {
            // EquipSlot не работает с ItemGridData, возвращаем null
            return null;
        }

        public void RegisterVisual(ItemVisual visual, ItemGridData gridData) { }
        public void UnregisterVisual(ItemVisual visual) { }
    }
}