using SkyClerik.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
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

        protected VisualElement _inventoryGrid;
        private const string _inventoryGridID = "grid";

        // Зависимости
        protected ItemsPage _itemsPage;
        protected UIDocument _document;
        protected MonoBehaviour _coroutineRunner;
        protected EquipmentContainer _equipmentContainer;

        // UI-элементы
        protected VisualElement _root;
        protected Telegraph _telegraph;
        protected PlacementResults _placementResults;
        protected List<VisualElement> _cells; // Только объявление, инициализация в конструкторе

        public UIDocument GetDocument => _document;
        public VisualElement Root => _root;
        public Telegraph Telegraph => _telegraph;
        private Vector2 _cellSize;

        public Vector2 CellSize => _cellSize;

        private PlayerItemContainer _playerItemContainer;

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
            _inventoryGrid = _root.Q<VisualElement>(_inventoryGridID);

            _playerItemContainer = ServiceProvider.Get<PlayerItemContainer>();
            _cellSize = _playerItemContainer.CellSize;

            _header = _root.Q(_headerID);
            _title = _root.Q<Label>(_titleID);

            _title.text = _titleText;

            SubscribeToContainerEvents();

            _coroutineRunner = itemsPage;
            _coroutineRunner.StartCoroutine(Initialize());
        }

        protected void SubscribeToContainerEvents()
        {
            _equipmentContainer.PlayerEquipmentContainerDefinition.OnItemEquipped += HandleItemEquipped;
            _equipmentContainer.PlayerEquipmentContainerDefinition.OnItemUnequipped += HandleItemUnequipped;
        }

        protected void UnsubscribeFromContainerEvents()
        {
            _equipmentContainer.PlayerEquipmentContainerDefinition.OnItemEquipped -= HandleItemEquipped;
            _equipmentContainer.PlayerEquipmentContainerDefinition.OnItemUnequipped -= HandleItemUnequipped;
        }

        // --- Инициализация и подписка на события ---
        protected IEnumerator Initialize()
        {
            _telegraph = new Telegraph();
            AddItemToInventoryGrid(_telegraph);

            yield return new WaitForEndOfFrame();
            ;
            while (_inventoryGrid.resolvedStyle.width == 0)
                yield return null;

            _cells = _inventoryGrid.Children().ToList();
            for (int i = 0; i < _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots.Count; i++)
            {
                _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots[i].CallPlace = _cells[i];
            }

            foreach (var slot in _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots)
            {
                if (slot.EquippedItem != null)
                {
                    Debug.Log($"slot item : {slot.EquippedItem.DefinitionName}");
                    CreateItemVisualInSlot(slot, slot.EquippedItem);
                }
            }
        }

        private void CreateItemVisualInSlot(EquipmentSlot slot, ItemBaseDefinition equippedItem)
        {
            ItemVisual newItemVisual = new ItemVisual(
                itemsPage: _itemsPage,
                ownerInventory: this,
                itemDefinition: equippedItem,
                gridPosition: Vector2Int.zero,
                gridSize: new Vector2Int(equippedItem.Dimensions.Width * 128, equippedItem.Dimensions.Height * 128));

            slot.CallPlace.Add(newItemVisual);
        }

        /// <summary>
        /// Обрабатывает событие экипировки предмета.
        /// </summary>
        private void HandleItemEquipped(EquipmentSlot slot, ItemVisual item)
        {
            // Ну тут не создание должно быть то, это присваение ячейке уже имеющегося itemVisual
            //CreateItemVisualInSlot(slot, item);
        }

        /// <summary>
        /// Обрабатывает событие снятия предмета.
        /// </summary>
        private void HandleItemUnequipped(EquipmentSlot slot, ItemVisual item)
        {
            //int index = _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots.IndexOf(slot);
            //if (index != -1 && index < _cells.Count)
            //{
            //    VisualElement slotVisualElement = _cells[index];
            //    ItemVisual itemVisualToRemove = _itemVisuals.FirstOrDefault(iv => iv.ItemDefinition == item && iv.parent == slotVisualElement);
            //    if (itemVisualToRemove != null)
            //    {
            //        itemVisualToRemove.RemoveFromHierarchy();
            //        _itemVisuals.Remove(itemVisualToRemove);
            //    }
            //}
        }

        /// <summary>
        /// Возвращает ItemVisual для экипированного предмета, если он существует в UI слотах.
        /// </summary>
        /// <param name="itemDef">Определение предмета, для которого ищется ItemVisual.</param>
        /// <returns>Найденный ItemVisual или null.</returns>
        private ItemVisual GetItemVisualForEquippedItem(ItemVisual itemDef)
        {
            //foreach (ItemVisual slotVisual in _itemVisuals)
            //{
            //    if (slotVisual != null && slotVisual.ItemDefinition == itemDef)
            //    {
            //        return slotVisual;
            //    }
            //}
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
            // ItemsPage уже определил, что предмет можно экипировать в этот контейнер (EquipmentPageElement)
            // Теперь нужно найти, в какой именно слот
            Vector2 mouseLocalPositionInRoot = _root.WorldToLocal(_itemsPage.MouseUILocalPosition);
            EquipmentSlot targetSlot = _equipmentContainer.PlayerEquipmentContainerDefinition.GetSlot(mouseLocalPositionInRoot);

            if (targetSlot != null)
            {
                ItemVisual unequippedItem;
                _equipmentContainer.PlayerEquipmentContainerDefinition.TryEquipItem(storedItem, targetSlot, out unequippedItem);
                // Если был unequippedItem, он будет обработан через OnItemUnequipped -> HandleItemUnequipped
            }
        }

        public void RemoveStoredItem(ItemVisual storedItem)
        {
            // ItemsPage хочет удалить этот предмет из экипировки
            // Нужно найти слот, в котором находится этот предмет, и снять его.
            EquipmentSlot slotToRemoveFrom = _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots.FirstOrDefault(s => s.ItemVisual.ItemDefinition == storedItem.ItemDefinition);

            if (slotToRemoveFrom != null)
            {
                _equipmentContainer.PlayerEquipmentContainerDefinition.UnequipItem(slotToRemoveFrom);
                // OnItemUnequipped -> HandleItemUnequipped будет вызван
            }
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            _root.Add(item);
        }

        public ItemGridData GetItemGridData(ItemVisual itemVisual)
        {
            return null; // Для экипировки предметы не располагаются в сетке.
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
            return false; // Для экипировки у нас фиксированные слоты, а не динамический поиск места.
        }

        public void FinalizeDrag() => _telegraph.Hide();

        public void Dispose()
        {
            UnsubscribeFromContainerEvents();
        }

    }
}