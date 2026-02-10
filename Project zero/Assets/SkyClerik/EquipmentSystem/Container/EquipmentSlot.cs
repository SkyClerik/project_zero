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
        [ReadOnly]
        [Tooltip("Заполняется методом CalculateGridDimensionsFromUI. Не редактировать руками")]
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

        public ItemVisual CreateItemVisualForSlot(ItemBaseDefinition itemDefinition, ItemsPage itemsPage)
        {
            Debug.Log("CreateItemVisualForSlot");
            return new ItemVisual(
                itemsPage: itemsPage,
                ownerInventory: this, // САМ СЛОТ ЭКИПИРОВКИ является ownerInventory
                itemDefinition: itemDefinition,
                gridPosition: Vector2Int.zero, // Не используется для экипировки
                gridSize: Vector2Int.zero // Не используется для экипировки
            );
        }

        public PlacementResults ShowPlacementTarget(ItemVisual itemVisual)
        {
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
                    //Debug.Log($"[ShowPlacementTarget] _itemVisual.ItemDefinition.name: {_itemVisual.ItemDefinition.name}.");
                    conflict = ReasonConflict.SwapAvailable;
                }
            }

            _telegraph.SetPosition(_rect.position);
            _telegraph.SetPlacement(conflict, _rect.size.x, _rect.size.y);

            return new PlacementResults().Init(
                conflict: conflict,
                position: _rect.position,
                suggestedGridPosition: Vector2Int.zero, // Не используется для экипировки
                overlapItem: _itemVisual == null ? null : _itemVisual,
                targetInventory: this // Сам EquipmentSlot это сетка инвентаря
            );
        }

        public void FinalizeDrag()
        {
            _telegraph.Hide();
        }

        public void PickUp(ItemVisual storedItem)
        {
            //Debug.Log($"[EquipmentSlot][PickUp] Поднимаем предмет: {storedItem.ItemDefinition.name} из слота {_cellNameDebug}");
            _itemVisual = null;
            _equippedItem = null;
            //Debug.Log($"[ЭКИПИРОВКА][EquipmentSlot] Слот {_cellNameDebug} очищен (логика Unequip).");
            ItemsPage.CurrentDraggedItem = storedItem;
            ItemsPage.CurrentDraggedItem.SetOwnerInventory(this);
            _document.rootVisualElement.Add(storedItem);
            //Debug.Log($"[EquipmentSlot][PickUp] Установлен ItemsPage.CurrentDraggedItem: {ItemsPage.CurrentDraggedItem.ItemDefinition.name}. Его владелец: {ItemsPage.CurrentDraggedItem.OwnerInventory?.GetType().Name ?? "NULL"}.");
        }

        public void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            Equip(storedItem);
        }

        public void Equip(ItemVisual itemVisual)
        {
            //Debug.Log($"Equip '{itemVisual.ItemDefinition.name}'.");
            if (itemVisual == null)
                return;

            IDropTarget sourceOwner = itemVisual.OwnerInventory;
            if (sourceOwner is GridPageElementBase sourceGridPage)
            {
                ItemContainer sourceContainer = sourceGridPage.ItemContainer;
                if (sourceContainer != null)
                {
                    //Debug.Log($"[EquipmentSlot][Equip] Удаляю '{itemVisual.ItemDefinition.name}' из исходного инвентаря '{sourceContainer.name}'.");
                    sourceContainer.RemoveItem(itemVisual.ItemDefinition, destroy: false);
                }
            }

            _equippedItem = itemVisual.ItemDefinition;
            _itemVisual = itemVisual;
            itemVisual.SetOwnerInventory(this);

            _cell.Add(itemVisual);
            _telegraph.Hide();

            SetItemVisualPositionAndStyle(itemVisual);

            ItemsPage.CurrentDraggedItem = null;
        }

        private void SetItemVisualPositionAndStyle(ItemVisual itemVisual)
        {
            float itemWidth = itemVisual.style.width.value.value;
            float itemHeight = itemVisual.style.height.value.value;

            float centerX = (_rect.size.x - itemWidth) / 2f;
            float centerY = (_rect.size.y - itemHeight) / 2f;

            itemVisual.style.left = centerX;
            itemVisual.style.top = centerY;
            itemVisual.style.position = Position.Absolute;
        }

        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            suggestedGridPosition = Vector2Int.zero; // Не используется для экипировки
            return _itemVisual == null && CanEquip(item);
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            //Debug.Log($"[AddItemToInventoryGrid][ЗАГЛУШКА] item: {item}");
        }

        public void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)
        {
            //Debug.Log($"[AddStoredItem][ЗАГЛУШКА] storedItem: {storedItem}");
        }

        public void RemoveStoredItem(ItemVisual storedItem)
        {
            //Debug.Log($"[RemoveStoredItem][ЗАГЛУШКА] storedItem: {storedItem}");
        }

        public ItemGridData GetItemGridData(ItemVisual itemVisual)
        {
            //Debug.Log($"[GetItemGridData][ЗАГЛУШКА] itemVisual: {itemVisual}");
            return null;
        }

        public void RegisterVisual(ItemVisual itemVisual, ItemGridData gridData)
        {
            //Debug.Log($"[GetItemGridData][ЗАГЛУШКА] itemVisual: {itemVisual}");
        }
        public void UnregisterVisual(ItemVisual itemVisual)
        {
            //Debug.Log($"[GetItemGridData][ЗАГЛУШКА] ItemVisual: {itemVisual}");
        }
    }
}