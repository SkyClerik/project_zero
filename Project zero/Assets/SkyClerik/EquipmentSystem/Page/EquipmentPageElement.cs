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
            _title = _header.Q<Label>(_titleID);

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

        protected IEnumerator Initialize()
        {
            // Добавляем цикл ожидания, пока _inventoryGrid не получит реальные размеры
            // и пока в нем не появятся дочерние элементы
            while (_inventoryGrid == null || _inventoryGrid.resolvedStyle.width == 0 || _inventoryGrid.childCount == 0)
            {
                yield return null; // Ждем следующий кадр
            }

            _cells = _inventoryGrid.Children().ToList(); // Инициализация _cells теперь здесь, после ожидания, и из _inventoryGrid

            // Заполняем CallPlace для каждого EquipmentSlot
            for (int i = 0; i < _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots.Count; i++)
            {
                if (i < _cells.Count) // Проверяем границы, чтобы не было ArgumentOutOfRangeException
                {
                    _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots[i].CallPlace = _cells[i];
                }
            }
            
            _telegraph = new Telegraph();
            AddItemToInventoryGrid(_telegraph);
            LoadInitialVisuals();
        }

        protected void LoadInitialVisuals()
        {
            foreach (EquipmentSlot slot in _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots)
            {
                if (slot.ItemVisual != null)
                {
                    slot.CallPlace.Add(slot.ItemVisual);
                }
            }
        }

        /// <summary>
        /// Обрабатывает событие экипировки предмета.
        /// </summary>
        private void HandleItemEquipped(EquipmentSlot slot, ItemVisual item)
        {
            slot.CallPlace.Add(item);
        }

        /// <summary>
        /// Обрабатывает событие снятия предмета.
        /// </summary>
        private void HandleItemUnequipped(EquipmentSlot slot, ItemVisual item)
        {
            item.RemoveFromHierarchy();
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

            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - Начало. draggedItem: {draggedItem?.ItemDefinition?.name}");
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - _root: {_root?.name}, enabledSelf: {_root?.enabledSelf}, display: {_root?.resolvedStyle.display}, visibility: {_root?.resolvedStyle.visibility}");
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - _root.worldBound: {_root.worldBound}");
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - _root.localBound: {_root.localBound}");
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - _inventoryGrid.layout: {_inventoryGrid.layout}");

            if (_root == null || !_root.enabledSelf || _root.resolvedStyle.display == DisplayStyle.None || _root.resolvedStyle.visibility == Visibility.Hidden)
            {
                _telegraph.Hide();
                Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - _root не активен/виден. Скрываю телеграф. Conflict: ReasonConflict.beyondTheGridBoundary");
                return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);
            Debug.Log($"mouseLocalPosition: {mouseLocalPosition}");
            EquipmentSlot targetSlot = _equipmentContainer.PlayerEquipmentContainerDefinition.GetSlot(mouseLocalPosition);
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - targetSlot: {(targetSlot != null ? "Найден" : "NULL")}");

            if (targetSlot == null)
            {
                _telegraph.Hide();
                Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - targetSlot == null. Скрываю телеграф. Conflict: ReasonConflict.beyondTheGridBoundary");
                return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            // Курсор находится над слотом
            bool canEquip = targetSlot.CanEquip(draggedItem.ItemDefinition);
            bool isEmpty = targetSlot.IsEmpty;
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - targetSlot.CanEquip({draggedItem.ItemDefinition.name}): {canEquip}. targetSlot.IsEmpty: {isEmpty}");

            if (canEquip)
            {
                if (isEmpty)
                {
                    _placementResults.Conflict = ReasonConflict.None;
                    Debug.Log($"Слот подходит и пуст");
                }
                else
                {
                    _placementResults.Conflict = ReasonConflict.SwapAvailable;
                    Debug.Log($" Слот подходит, но занят (возможен свап)");
                }
            }
            else
            {
                _placementResults.Conflict = ReasonConflict.invalidSlotType;
                Debug.Log($"Предмет не подходит по типу");
            }
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - Определенный Conflict: {_placementResults.Conflict}");


            // Отображаем телеграф на позиции и размере слота
            // Преобразуем локальную позицию слота (относительно _inventoryGrid)
            // в локальную позицию относительно _root (equipment_root)
            Vector2 telegraphLocalPositionInRoot = _inventoryGrid.ChangeCoordinatesTo(_root, targetSlot.Rect.position);
            _telegraph.SetPosition(telegraphLocalPositionInRoot);
            _telegraph.SetPlacement(_placementResults.Conflict, telegraphLocalPositionInRoot.x, telegraphLocalPositionInRoot.y);
            Debug.Log($"[EquipmentPageElement.ShowPlacementTarget] - Telegraph set to position: {telegraphLocalPositionInRoot}, size: {targetSlot.Rect.size}, conflict: {_placementResults.Conflict}");


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
            Vector2 mouseLocalPositionInRoot = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);
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
            EquipmentSlot slotToRemoveFrom = _equipmentContainer.PlayerEquipmentContainerDefinition.EquipmentSlots.FirstOrDefault(s => s.ItemVisual == storedItem);

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
            // EquipmentPageElement не использует регистрацию визуальных элементов, так как ItemVisual
            // напрямую добавляется в EquipmentSlot.CallPlace при экипировке.
        }

        public void UnregisterVisual(ItemVisual visual)
        {
            // EquipmentPageElement не использует выгрузку визуальных элементов, так как ItemVisual
            // удаляется из иерархии при снятии экипировки.
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