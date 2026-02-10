using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public abstract class GridPageElementBase : IDropTarget, IDisposable
    {
        protected Dictionary<ItemVisual, ItemGridData> _visuals = new Dictionary<ItemVisual, ItemGridData>();
        protected Rect _gridRect;

        // Зависимости
        protected UIDocument _document;
        protected MonoBehaviour _coroutineRunner;
        protected ItemsPage _itemsPage;
        protected ItemContainer _itemContainer;

        // UI-элементы
        protected VisualElement _root;
        private VisualElement _inventoryGrid;
        private const string _inventoryGridID = "grid";
        protected Telegraph _telegraph;
        protected PlacementResults _placementResults;
        private readonly float _gridHoverSnapToBoundaryPixels = 64f;

        public UIDocument GetDocument => _document;
        public ItemContainer ItemContainer => _itemContainer;
        public Vector2 CellSize => _itemContainer.CellSize;
        public VisualElement Root => _root;
        public Telegraph Telegraph => _telegraph;
        public bool SuppressNextVisualCreation { get; set; }

        protected GridPageElementBase(ItemsPage itemsPage, UIDocument document, ItemContainer itemContainer,
string rootID)
        {
            _itemsPage = itemsPage;
            _document = document;
            _coroutineRunner = itemsPage;
            _itemContainer = itemContainer;

            _root = _document.rootVisualElement.Q<VisualElement>(rootID);
            _inventoryGrid = _root.Q<VisualElement>(_inventoryGridID);

            _coroutineRunner.StartCoroutine(Initialize());
        }

        protected IEnumerator Initialize()
        {
            Configure();
            yield return new WaitForEndOfFrame();

            SubscribeToContainerEvents();
            LoadInitialVisuals();
        }

        protected virtual void SubscribeToContainerEvents()
        {
            if (_itemContainer == null) return;
            _itemContainer.OnItemAdded += HandleItemAdded;
            _itemContainer.OnItemRemoved += HandleItemRemoved;
            _itemContainer.OnCleared += HandleContainerCleared;
        }

        protected virtual void UnsubscribeFromContainerEvents()
        {
            if (_itemContainer == null) return;
            _itemContainer.OnItemAdded -= HandleItemAdded;
            _itemContainer.OnItemRemoved -= HandleItemRemoved;
            _itemContainer.OnCleared -= HandleContainerCleared;
        }

        protected void Configure()
        {
            _telegraph = new Telegraph();
            AddItemToInventoryGrid(_telegraph);
        }

        private void HandleItemAdded(ItemBaseDefinition item)
        {
            //Debug.Log($"[GridPageElementBase:{_root.name}] HandleItemAdded вызван для '{item.name}'. SuppressNextVisualCreation: {SuppressNextVisualCreation}");

            if (SuppressNextVisualCreation)
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] SuppressNextVisualCreation установлен. Пропускаем создание/обновление visual для '{item.name}'.");
                SuppressNextVisualCreation = false;
                return;
            }

            var existingVisual = _visuals.Keys.FirstOrDefault(visual => GetItemDefinition(visual) == item);
            if (existingVisual != null)
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] HandleItemAdded: Найден существующий visual для '{item.name}', обновляем его.");
                existingVisual.UpdatePcs();
                existingVisual.SetPosition(new Vector2(item.GridPosition.x * CellSize.x, item.GridPosition.y * CellSize.y));
                _visuals[existingVisual] = new ItemGridData(item, item.GridPosition);
            }
            else
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] HandleItemAdded: Существующий visual для '{item.name}' не найден, создаем новый.");
                CreateVisualForItem(item);
            }
        }

        private void HandleItemRemoved(ItemBaseDefinition item)
        {
            var visualToRemove = _visuals.Keys.FirstOrDefault(visual => GetItemDefinition(visual) == item);
            if (visualToRemove != null)
            {
                //Debug.Log($"[GridPageElementBase] HandleItemRemoved: Найден и удаляется visual для '{item.name}'. HashCode: {visualToRemove.GetHashCode()}");
                UnregisterVisual(visualToRemove);
                visualToRemove.RemoveFromHierarchy();
            }
        }

        private void HandleContainerCleared()
        {
            foreach (var visual in _visuals.Keys.ToList())
            {
                visual.RemoveFromHierarchy();
            }
            _visuals.Clear();
        }

        //private void CreateGridBoundaryVisualizer() // Оставлен на случай необходимости дебага
        //{
        //if (_inventoryGrid == null || CellSize.x <= 0 || CellSize.y <= 0) return;
        //var _gridRect = _itemContainer.GridWorldRect;
        //Debug.Log($"[GridPageElementBase:{_root.name}] CreateGridBoundaryVisualizer: отрисовываем границу по Rect: {_gridRect}. CellSize: {CellSize}", _coroutineRunner);
        //var test1 = new VisualElement();
        //test1.name = "test1";
        //test1.style.width = _gridRect.width;
        //test1.style.height = _gridRect.height;
        //test1.style.left = _gridRect.x;
        //test1.style.top = _gridRect.y;
        //test1.SetBorderColor(Color.blue);
        //test1.SetBorderWidth(5);
        //test1.style.position = Position.Absolute;
        //test1.pickingMode = PickingMode.Ignore;
        //_document.rootVisualElement.Add(test1);
        //}

        private void LoadInitialVisuals()
        {
            foreach (var item in _itemContainer.GetItems())
            {
                CreateVisualForItem(item);
            }
        }
        private void CreateVisualForItem(ItemBaseDefinition item)
        {
            //Debug.Log($"[GridPageElementBase:{_root.name}] CreateVisualForItem: Создание НОВОГО ItemVisual для '{item.name}' с данными: Angle={item.Dimensions.Angle}, Size=({item.Dimensions.Width},{item.Dimensions.Height}), Pos={item.GridPosition}");
            var newGridData = new ItemGridData(item, item.GridPosition);
            Debug.Log("CreateVisualForItem");
            var newItemVisual = new ItemVisual(
                itemsPage: _itemsPage,
                ownerInventory: this,
                itemDefinition: item,
                gridPosition: item.GridPosition,
                gridSize: new Vector2Int(item.Dimensions.Width, item.Dimensions.Height));

            //Debug.Log($"[GridPageElementBase:{_root.name}] CreateVisualForItem: Новый ItemVisual '{newItemVisual.name}' создан. HashCode: {newItemVisual.GetHashCode()}. Owner: {this.GetType().Name}.");
            RegisterVisual(newItemVisual, newGridData);
            AddItemToInventoryGrid(newItemVisual);
            newItemVisual.SetPosition(new Vector2(item.GridPosition.x * CellSize.x, item.GridPosition.y * CellSize.y));
        }

        /// <summary>
        /// Добавляет визуальный элемент предмета в сетку инвентаря.
        /// </summary>
        /// <param name="item">Визуальный элемент, который нужно добавить.</param>
        public void AddItemToInventoryGrid(VisualElement item)
        {
            _inventoryGrid.Add(item);
        }

        // --- Реализация IDropTarget ---
        /// <summary>
        /// Пытается найти свободное место для предмета в контейнере.
        /// </summary>
        /// <param name="item">Предмет, для которого ищется место.</param>
        /// <param name="suggestedGridPosition">Предложенная позиция в сетке, если место найдено.</param>
        /// <returns>True, если свободное место найдено; иначе false.</returns>
        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            return _itemContainer.TryFindPlacement(item, out suggestedGridPosition);
        }

        /// <summary>
        /// Возвращает логические данные предмета (позиция, размер) по его визуальному представлению.
        /// </summary>
        /// <param name="itemVisual">Визуальный элемент предмета.</param>
        /// <returns>Объект <see cref="ItemGridData"/> или null, если визуальный элемент не зарегистрирован.</returns>
        public ItemGridData GetItemGridData(ItemVisual itemVisual)
        {
            _visuals.TryGetValue(itemVisual, out ItemGridData gridData);
            return gridData;
        }

        /// <summary>
        /// Регистрирует визуальный элемент предмета и его логические данные.
        /// </summary>
        /// <param name="visual">Визуальный элемент предмета.</param>
        /// <param name="gridData">Логические данные предмета.</param>
        public void RegisterVisual(ItemVisual visual, ItemGridData gridData)
        {
            if (!_visuals.ContainsKey(visual))
                _visuals.Add(visual, gridData);
        }

        /// <summary>
        /// Отменяет регистрацию визуального элемента предмета.
        /// </summary>
        /// <param name="visual">Визуальный элемент предмета.</param>
        public void UnregisterVisual(ItemVisual visual)
        {
            if (_visuals.ContainsKey(visual))
                _visuals.Remove(visual);
        }

        /// <summary>
        /// Показывает целевую область для размещения перетаскиваемого предмета,
        /// а также определяет возможные конфликты размещения.
        /// </summary>
        /// <param name="draggedItem">Перетаскиваемый визуальный элемент предмета.</param>
        /// <returns>Результаты размещения, включающие информацию о конфликте, предложенной позиции и пересекающемся предмете.</returns>
        public virtual PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Начало проверки для предмета '{draggedItem.ItemDefinition.name}'", _coroutineRunner);

            if (!_root.enabledSelf || _root.resolvedStyle.display == DisplayStyle.None || _root.resolvedStyle.visibility == Visibility.Hidden)
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Корневой элемент скрыт или неактивен. Conflict: beyondTheGridBoundary", _coroutineRunner);
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            Vector2Int itemGridSize = new Vector2Int(draggedItem.ItemDefinition.Dimensions.Width, draggedItem.ItemDefinition.Dimensions.Height);
            _placementResults = new PlacementResults();
            _placementResults.OverlapItem = null;

            //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: currentHoverGridPosition = {currentHoverGridPosition}, itemGridSize = {itemGridSize}", _coroutineRunner);

            if (_itemContainer.IsGridAreaFree(currentHoverGridPosition, itemGridSize, allowRotation: false))
            {
                _placementResults.Conflict = ReasonConflict.None;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Место свободно. Conflict: None", _coroutineRunner);
            }
            else
            {
                List<ItemVisual> overlappingItems = FindOverlappingItems(currentHoverGridPosition, itemGridSize, draggedItem);
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Место занято. Количество пересекающихся предметов: {overlappingItems.Count}", _coroutineRunner);

                if (overlappingItems.Count == 1)
                {
                    ItemVisual overlapItem = overlappingItems[0];
                    bool isSameStackableType = draggedItem.ItemDefinition.Stackable &&
                                               overlapItem.ItemDefinition.Stackable &&
                                               draggedItem.ItemDefinition.DefinitionName == overlapItem.ItemDefinition.DefinitionName;

                    if (isSameStackableType)
                    {
                        _placementResults.Conflict = ReasonConflict.StackAvailable;
                        //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Пересечение с одним предметом. Тип: StackAvailable", _coroutineRunner);
                    }
                    else
                    {
                        // Вырубили свап!!!
                        //_placementResults.Conflict = ReasonConflict.SwapAvailable;
                        //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Пересечение с одним предметом. Тип: SwapAvailable", _coroutineRunner);
                    }
                    _placementResults.OverlapItem = overlapItem;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                }
                else
                {
                    _placementResults.Conflict = ReasonConflict.intersectsObjects;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                    //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Пересечение с несколькими предметами или занято 'пустым' местом. Conflict: intersectsObjects", _coroutineRunner);
                }
            }

            if (!_itemContainer.IsGridAreaFree(currentHoverGridPosition, itemGridSize, allowRotation: false) && _placementResults.Conflict == ReasonConflict.None)
            {
                _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Предмет '{draggedItem.ItemDefinition.name}' находится за пределами сетки. Позиция: {currentHoverGridPosition}, Размер: {itemGridSize}", _coroutineRunner);
            }

            if (_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary || _placementResults.Conflict == ReasonConflict.intersectsObjects)
            {
                _telegraph.Hide();
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Telegraph скрыт из-за конфликта: {_placementResults.Conflict}", _coroutineRunner);
            }
            else
            {
                var pos = new Vector2(_placementResults.SuggestedGridPosition.x * CellSize.x, _placementResults.SuggestedGridPosition.y * CellSize.y);
                _telegraph.SetPosition(pos);
                _telegraph.SetPlacement(_placementResults.Conflict, itemGridSize.x * CellSize.x, itemGridSize.y * CellSize.y);
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Telegraph показан на позиции {pos} с размером {itemGridSize.x * CellSize.x}x{itemGridSize.y * CellSize.y}. Conflict: {_placementResults.Conflict}", _coroutineRunner);
            }

            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x * CellSize.x, _placementResults.SuggestedGridPosition.y * CellSize.y),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }

        protected Vector2Int CalculateCurrentHoverGridPosition()
        {
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);

            float adjustedX = mouseLocalPosition.x;
            float adjustedY = mouseLocalPosition.y;

            if (adjustedX < 0 && adjustedX > -_gridHoverSnapToBoundaryPixels)
                adjustedX = 0;
            else if (adjustedX > _inventoryGrid.localBound.width && adjustedX < _inventoryGrid.localBound.width + _gridHoverSnapToBoundaryPixels)
                adjustedX = _inventoryGrid.localBound.width;

            if (adjustedY < 0 && adjustedY > -_gridHoverSnapToBoundaryPixels)
                adjustedY = 0;
            else if (adjustedY > _inventoryGrid.localBound.height && adjustedY < _inventoryGrid.localBound.height + _gridHoverSnapToBoundaryPixels)
                adjustedY = _inventoryGrid.localBound.height;

            int gridX = Mathf.FloorToInt(adjustedX / CellSize.x);
            int gridY = Mathf.FloorToInt(adjustedY / CellSize.y);
            return new Vector2Int(gridX, gridY);
        }

        protected List<ItemVisual> FindOverlappingItems(Vector2Int start, Vector2Int size, ItemVisual draggedItem)
        {
            List<ItemVisual> overlappingItems = new List<ItemVisual>();
            RectInt targetRect = new RectInt(start.x, start.y, size.x, size.y);

            foreach (var entry in _visuals)
            {
                ItemVisual currentItem = entry.Key;
                if (currentItem == draggedItem) continue;

                ItemGridData gridData = entry.Value;
                RectInt currentItemRect = new RectInt(gridData.GridPosition.x, gridData.GridPosition.y,
gridData.GridSize.x, gridData.GridSize.y);

                if (targetRect.Overlaps(currentItemRect))
                    overlappingItems.Add(currentItem);
            }
            return overlappingItems;
        }

        /// <summary>
        /// Завершает операцию перетаскивания, скрывая телеграф.
        /// </summary>
        public virtual void FinalizeDrag() => _telegraph.Hide();

        /// <summary>
        /// Добавляет предмет в контейнер на указанную позицию.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета, который нужно добавить.</param>
        /// <param name="gridPosition">Позиция в сетке для добавления.</param>
        public virtual void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)
        {
            _itemContainer.MoveItem(storedItem.ItemDefinition, gridPosition);
        }

        /// <summary>
        /// Удаляет предмет из контейнера.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета, который нужно удалить.</param>
        public virtual void RemoveStoredItem(ItemVisual storedItem)
        {
            _itemContainer.RemoveItem(GetItemDefinition(storedItem), true);
        }

        /// <summary>
        /// Поднимает предмет из сетки, подготавливая его к перетаскиванию.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета, который нужно поднять.</param>
        public virtual void PickUp(ItemVisual storedItem)
        {
            var itemDef = GetItemDefinition(storedItem);
            if (itemDef != null)
            {
                //Debug.Log($"[GridPageElementBase] PickUp: Вызов OccupyGridCells(false) для '{itemDef.name}' на позиции {itemDef.GridPosition}");
                _itemContainer.OccupyGridCells(itemDef, false);
                itemDef.GridPosition = new Vector2Int(-1, -1);
            }
            ItemsPage.CurrentDraggedItem = storedItem;
            _document.rootVisualElement.Add(storedItem);
            storedItem.SetOwnerInventory(this);
        }

        /// <summary>
        /// Помещает предмет обратно в сетку на указанную позицию.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета, который нужно поместить.</param>
        /// <param name="gridPosition">Позиция в сетке для размещения.</param>
        public virtual void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            _itemContainer.MoveItem(storedItem.ItemDefinition, gridPosition);
        }

        /// <summary>
        /// Возвращает определение предмета по его визуальному элементу.
        /// </summary>
        /// <param name="itemVisual">Визуальный элемент предмета.</param>
        /// <returns>Определение предмета (<see cref="ItemBaseDefinition"/>) или null, если не найдено.</returns>
        public ItemBaseDefinition GetItemDefinition(ItemVisual itemVisual)
        {
            _visuals.TryGetValue(itemVisual, out ItemGridData gridData);
            return gridData?.ItemDefinition;
        }

        /// <summary>
        /// Возвращает визуальный элемент предмета по его определению.
        /// </summary>
        /// <param name="itemDefinition">Определение предмета.</param>
        /// <returns>Визуальный элемент предмета (<see cref="ItemVisual"/>) или null, если не найден.</returns>
        public ItemVisual GetItemVisual(ItemBaseDefinition itemDefinition)
        {
            foreach (var entry in _visuals)
            {
                if (entry.Key.ItemDefinition == itemDefinition)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Очищает существующие визуальные элементы и пересоздает их на основе текущих данных в ItemContainer.
        /// </summary>
        public void RefreshVisuals()
        {
            foreach (var visual in _visuals.Keys.ToList())
            {
                visual.RemoveFromHierarchy();
            }
            _visuals.Clear();

            LoadInitialVisuals();
            //Debug.Log($"[GridPageElementBase:{_root.name}] Visuals refreshed. Recreated {_visuals.Count} items.", _coroutineRunner);
        }

        /// <summary>
        /// Выполняет очистку ресурсов и отписку от событий.
        /// </summary>
        public virtual void Dispose()
        {
            UnsubscribeFromContainerEvents();
        }
    }
}