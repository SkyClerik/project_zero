using SkyClerik.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.UIElements;

namespace SkyClerik.EquipmentSystem
{
    /// <summary>
    /// Конкретная реализация UI-страницы для отображения слотов экипировки игрока.
    /// </summary>
    public class EquipmentPageElement : IDropTarget, IDisposable
    {
        private const string _titleText = "Окно Экипировки";
        private VisualElement _header;
        private const string _headerID = "header";
        private Label _title;
        private const string _titleID = "l_title";

        private List<ItemVisual> _itemVisuals = new List<ItemVisual>();

        // Зависимости
        protected ItemsPage _itemsPage;
        protected UIDocument _document;
        protected MonoBehaviour _coroutineRunner;
        protected EquipmentContainer _equipmentContainer;

        // UI-элементы
        protected VisualElement _root;
        protected Telegraph _telegraph;
        protected PlacementResults _placementResults;

        public UIDocument GetDocument => _document;
        public VisualElement Root => _root;
        public Telegraph Telegraph => _telegraph;

        public Vector2 CellSize => throw new NotImplementedException();

        /// <summary>
        /// Конструктор для страницы экипировки.
        /// </summary>
        /// <param name="itemsPage">Ссылка на главную страницу предметов.</param>
        /// <param name="document">UIDocument, к которому принадлежит эта страница.</param>
        /// <param name="equipmentContainerDefinition">Контейнер экипировки, связанный с этой страницей.</param>
        /// <param name="rootID">Идентификатор корневого визуального элемента страницы в UIDocument.</param>
        public EquipmentPageElement(ItemsPage itemsPage, UIDocument document, EquipmentContainer equipmentContainer)
        {
            _itemsPage = itemsPage;
            _document = document;
            _equipmentContainer = equipmentContainer;

            _root = _document.rootVisualElement.Q<VisualElement>(equipmentContainer.RootPanelName);
            _header = _root.Q(_headerID);
            _title = _root.Q<Label>(_titleID);

            _title.text = _titleText;



            SubscribeToContainerEvents();

            _coroutineRunner = itemsPage;
            _coroutineRunner.StartCoroutine(Initialize());
        }

        // --- Инициализация и подписка на события ---
        protected IEnumerator Initialize()
        {
            Configure();
            yield return new WaitForEndOfFrame();
            LoadInitialVisuals();
        }

        protected void SubscribeToContainerEvents()
        {
            //if (_itemContainer == null) return;
            //_itemContainer.OnItemAdded += HandleItemAdded;
            //_itemContainer.OnItemRemoved += HandleItemRemoved;
            //_itemContainer.OnCleared += HandleContainerCleared;

            //_equipmentContainerDefinition.OnItemEquipped += HandleItemEquipped;
            //_equipmentContainerDefinition.OnItemUnequipped += HandleItemUnequipped;
        }

        protected void UnsubscribeFromContainerEvents()
        {
            //if (_itemContainer == null) return;
            //_itemContainer.OnItemAdded -= HandleItemAdded;
            //_itemContainer.OnItemRemoved -= HandleItemRemoved;
            //_itemContainer.OnCleared -= HandleContainerCleared;

            //_equipmentContainerDefinition.OnItemEquipped -= HandleItemEquipped;
            //_equipmentContainerDefinition.OnItemUnequipped -= HandleItemUnequipped;
        }

        protected void Configure()
        {
            _telegraph = new Telegraph();
            AddItemToInventoryGrid(_telegraph);
        }

        private void LoadInitialVisuals()
        {
            foreach (var item in _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots)
            {
                CreateVisualForItem(item.EquippedItem);
            }
        }
        private void CreateVisualForItem(ItemBaseDefinition item)
        {
            //Debug.Log($"[GridPageElementBase] CreateVisualForItem: Создание нового ItemVisual для '{item.name}' с данными: Angle={item.Dimensions.Angle}, Size=({item.Dimensions.Width},{item.Dimensions.Height}), Pos={item.GridPosition}");
            var newGridData = new ItemGridData(item, item.GridPosition);
            var newItemVisual = new ItemVisual(
                itemsPage: _itemsPage,
                ownerInventory: this,
                itemDefinition: item,
                gridPosition: item.GridPosition,
                gridSize: new Vector2Int(item.Dimensions.Width, item.Dimensions.Height));

            //Debug.Log($"[GridPageElementBase] CreateVisualForItem: Новый ItemVisual создан. HashCode: {newItemVisual.GetHashCode()}");
            RegisterVisual(newItemVisual, newGridData);
            AddItemToInventoryGrid(newItemVisual);
            newItemVisual.SetPosition(new Vector2(item.GridPosition.x * CellSize.x, item.GridPosition.y * CellSize.y));
        }

        /// <summary>
        /// Обрабатывает событие экипировки предмета.
        /// </summary>
        private void HandleItemEquipped(EquipmentSlot slot, ItemBaseDefinition item)
        {
            //if (_slotVisuals.TryGetValue(slot, out EquipmentSlotVisual slotVisual))
            //{
            //    slotVisual.SetItem(item);
            //}
        }

        /// <summary>
        /// Обрабатывает событие снятия предмета.
        /// </summary>
        private void HandleItemUnequipped(EquipmentSlot slot, ItemBaseDefinition item)
        {
            //if (_slotVisuals.TryGetValue(slot, out EquipmentSlotVisual slotVisual))
            //{
            //    slotVisual.SetItem(null); // Убираем визуальное представление предмета
            //}
        }

        /// <summary>
        /// Возвращает EquipmentSlotVisual по логическому EquipmentSlot.
        /// </summary>
        //public EquipmentSlotVisual GetSlotVisual(EquipmentSlot slot)
        //{
        //    //_slotVisuals.TryGetValue(slot, out EquipmentSlotVisual slotVisual);
        //    //return slotVisual;
        //}

        /// <summary>
        /// Возвращает логический EquipmentSlot по ItemVisual.
        /// </summary>
        //public EquipmentSlot GetSlotByItemVisual(ItemVisual itemVisual)
        //{
        //    return _itemVisuals.FirstOrDefault(s => s.EquippedItem == itemVisual.ItemDefinition);

        //    return null;
        //}

        /// <summary>
        /// Возвращает ItemVisual для экипированного предмета, если он существует в UI слотах.
        /// </summary>
        /// <param name="itemDef">Определение предмета, для которого ищется ItemVisual.</param>
        /// <returns>Найденный ItemVisual или null.</returns>
        public ItemVisual GetItemVisualForEquippedItem(ItemBaseDefinition itemDef)
        {
            foreach (ItemVisual slotVisual in _itemVisuals)
            {
                if (slotVisual != null && slotVisual.ItemDefinition == itemDef)
                {
                    return slotVisual;
                }
            }
            return null;
        }

        public void PickUp(ItemVisual storedItem)
        {
            // Здесь логика PickUp будет обрабатываться в ItemsPage
        }

        public void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            // Здесь логика Drop будет обрабатываться в ItemsPage
        }


        /// <summary>
        /// Показывает целевую область для размещения перетаскиваемого предмета,
        /// а также определяет возможные конфликты размещения.
        /// </summary>
        /// <param name="draggedItem">Перетаскиваемый визуальный элемент предмета.</param>
        /// <returns>Результаты размещения, включающие информацию о конфликте, предложенной позиции и пересекающемся предмете.</returns>
        public PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            _placementResults = new PlacementResults();
            _placementResults.OverlapItem = null;

            if (_root == null || !_root.enabledSelf || _root.resolvedStyle.display == DisplayStyle.None || _root.resolvedStyle.visibility == Visibility.Hidden)
            {
                _telegraph.Hide();
                return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            Vector2 mouseLocalPositionInRoot = _root.WorldToLocal(_itemsPage.MouseUILocalPosition);
            EquipmentSlot targetSlot = _equipmentContainer.PlayerEquipmentContainerDefinition.GetSlot(mouseLocalPositionInRoot);

            if (targetSlot == null)
            {
                _telegraph.Hide();
                return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            // Курсор находится над слотом
            if (targetSlot.CanEquip(draggedItem.ItemDefinition))
            {
                if (targetSlot.IsEmpty)
                {
                    _placementResults.Conflict = ReasonConflict.None; // Слот подходит и пуст
                }
                else
                {
                    _placementResults.Conflict = ReasonConflict.SwapAvailable; // Слот подходит, но занят (возможен свап)
                    // Получаем ItemVisual для экипированного предмета через EquipmentPageElement
                    //_placementResults.OverlapItem = (_itemsPage.GetEquipmentPageElement() as EquipmentPageElement)?.GetItemVisualForEquippedItem(targetSlot.EquippedItem);
                }
            }
            else
            {
                _placementResults.Conflict = ReasonConflict.invalidSlotType; // Предмет не подходит по типу
            }

            // Отображаем телеграф на позиции и размере слота
            _telegraph.SetPosition(targetSlot.Rect.position);
            _telegraph.SetPlacement(_placementResults.Conflict, targetSlot.Rect.x, targetSlot.Rect.y);

            // Инициализируем PlacementResults с данными слота
            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: targetSlot.Rect.position,
                                          suggestedGridPosition: Vector2Int.zero, // Не используется для экипировки
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }


        public void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)
        {
            throw new NotImplementedException();
        }

        public void RemoveStoredItem(ItemVisual storedItem)
        {
            throw new NotImplementedException();
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            throw new NotImplementedException();
        }

        public ItemGridData GetItemGridData(ItemVisual itemVisual)
        {
            throw new NotImplementedException();
        }

        public void RegisterVisual(ItemVisual visual, ItemGridData gridData)
        {
            throw new NotImplementedException();
        }

        public void UnregisterVisual(ItemVisual visual)
        {
            throw new NotImplementedException();
        }

        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            suggestedGridPosition = Vector2Int.zero;
            return false;
        }

        public void FinalizeDrag() => _telegraph.Hide();

        public void Dispose()
        {
            UnsubscribeFromContainerEvents();
        }

    }
}