
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public abstract class GridPageElementBase : IDropTarget
    {
        // Словарь для хранения связей между визуальными элементами и их логическими данными
        protected Dictionary<ItemVisual, ItemGridData> _visuals = new Dictionary<ItemVisual, ItemGridData>();
        protected Rect _gridRect;

        // Зависимости
        protected UIDocument _document;
        protected MonoBehaviour _coroutineRunner;
        protected ItemsPage _itemsPage;
        protected ItemContainer _itemContainer;

        // UI-элементы
        protected VisualElement _root;
        protected VisualElement _inventoryGrid;
        private const string _inventoryGridID = "grid";
        protected Telegraph _telegraph;
        protected PlacementResults _placementResults;
        public LogicalGridVisualizer LogicalGridVisualizer; // Добавляем новое поле

        // --- Свойства IDropTarget и прочие ---
        public UIDocument GetDocument => _document;
        public ItemContainer ItemContainer => _itemContainer;
        public Vector2 CellSize => _itemContainer.CellSize; // Теперь берем из ItemContainer
        public VisualElement Root => _root;
        public Telegraph Telegraph => _telegraph;


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

        // --- Инициализация и подписка на события ---
                protected IEnumerator Initialize()
                {
                    Configure();
                    yield return new WaitForEndOfFrame();
                    CalculateCellSize();
                    CreateGridBoundaryVisualizer(); // Возвращаем создание визуального отладчика

            // Мой хороший, здесь мы инициализируем наш новый визуализатор
            LogicalGridVisualizer = new LogicalGridVisualizer();
            LogicalGridVisualizer.Init(_itemContainer);
            _document.rootVisualElement.Add(LogicalGridVisualizer); // Добавляем его к корневому элементу UI

        
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
        
                // --- Обработчики событий от ItemContainer ---
        
                private void HandleItemAdded(ItemBaseDefinition item)
                {
                    var existingVisual = _visuals.Keys.FirstOrDefault(visual => GetItemDefinition(visual) == item);
                    if (existingVisual != null)
                    {
                        existingVisual.UpdatePcs();
                        existingVisual.SetPosition(new Vector2(item.GridPosition.x * CellSize.x, item.GridPosition.y * CellSize.y));
                        _visuals[existingVisual] = new ItemGridData(item, item.GridPosition);
                    }
                    else
                    {
                        CreateVisualForItem(item);
                    }
                }
        
                private void HandleItemRemoved(ItemBaseDefinition item)
                {
                    var visualToRemove = _visuals.Keys.FirstOrDefault(visual => GetItemDefinition(visual) == item);
                    if (visualToRemove != null)
                    {
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
                
                private void CreateGridBoundaryVisualizer()
                {
                    if (_inventoryGrid == null || CellSize.x <= 0 || CellSize.y <= 0) return;
        
                    // Используем данные из ItemContainer для позиционирования
                    var _gridRect = _itemContainer.GridWorldRect; // Берем готовую мировую позицию сетки
        
                    Debug.Log($"[GridPageElementBase:{_root.name}] CreateGridBoundaryVisualizer: отрисовываем границу по Rect: {_gridRect}. CellSize: {CellSize}", _coroutineRunner);
        
                    var test1 = new VisualElement();
                    test1.name = "test1";
                    test1.style.width = _gridRect.width;
                    test1.style.height = _gridRect.height;
                    test1.style.left = _gridRect.x;
                    test1.style.top = _gridRect.y;
                    test1.SetBorderColor(Color.blue);
                    test1.SetBorderWidth(5);
                    test1.style.position = Position.Absolute;
                    test1.pickingMode = PickingMode.Ignore;
                    _document.rootVisualElement.Add(test1);
                }
                
                // --- Логика UI ---
                
                private void LoadInitialVisuals()
                {
                    foreach(var item in _itemContainer.GetItems())
                    {
                        CreateVisualForItem(item);
                    }
                }
        private void CreateVisualForItem(ItemBaseDefinition item)
        {
            var newGridData = new ItemGridData(item, item.GridPosition);
            var newItemVisual = new ItemVisual(
                itemsPage: _itemsPage,
                ownerInventory: this,
                itemDefinition: item,
                gridPosition: item.GridPosition,
                gridSize: new Vector2Int(item.Dimensions.DefaultWidth, item.Dimensions.DefaultHeight));

            RegisterVisual(newItemVisual, newGridData);
            AddItemToInventoryGrid(newItemVisual);
            newItemVisual.SetPosition(new Vector2(item.GridPosition.x * CellSize.x, item.GridPosition.y * CellSize.y));
        }

        protected virtual void CalculateCellSize()
        {
            // Теперь берем размер ячейки из ItemContainer, который уже рассчитан
            // и имеет приоритет над UI.
            if (CellSize.x <= 0 || CellSize.y <= 0)
            {
                Debug.LogWarning($"Размер ячейки в ItemContainer не рассчитан или равен нулю для '{_root.name}'. Возможно, CalculateGridDimensionsFromUI не был вызван.", _coroutineRunner);
            }
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            _inventoryGrid.Add(item);
        }

        public void AddLoot(LutContainer sourceLut)
        {
            if (sourceLut == null) return;
            var unplacedClones = _itemContainer.AddClonedItems(sourceLut.Items);

            if (unplacedClones.Any())
            {
                Debug.Log($"Не удалось разместить {unplacedClones.Count} предметов. Возвращаем в LutContainer.");
                   sourceLut.Items.Clear();
                sourceLut.Items.AddRange(unplacedClones);
            }
        }

        // --- Реализация IDropTarget ---
        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            return _itemContainer.TryFindPlacement(item, out suggestedGridPosition);
        }

        public ItemGridData GetItemGridData(ItemVisual itemVisual)
        {
            _visuals.TryGetValue(itemVisual, out ItemGridData gridData);
            return gridData;
        }

        public void RegisterVisual(ItemVisual visual, ItemGridData gridData)
        {
            if (!_visuals.ContainsKey(visual))
                _visuals.Add(visual, gridData);
        }

        public void UnregisterVisual(ItemVisual visual)
        {
            if (_visuals.ContainsKey(visual))
                _visuals.Remove(visual);
        }

        public virtual PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Начало проверки для предмета '{draggedItem.ItemDefinition.name}'", _coroutineRunner);

            // Если корневой элемент скрыт или неактивен, считать его недоступным
            if (!_root.enabledSelf || _root.resolvedStyle.display == DisplayStyle.None || _root.resolvedStyle.visibility == Visibility.Hidden)
            {
                Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Корневой элемент скрыт или неактивен. Conflict: beyondTheGridBoundary", _coroutineRunner);
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }
            
            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            Vector2Int itemGridSize = new Vector2Int(draggedItem.ItemDefinition.Dimensions.CurrentWidth, draggedItem.ItemDefinition.Dimensions.DefaultHeight);
            _placementResults = new PlacementResults();
            _placementResults.OverlapItem = null;

            Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: currentHoverGridPosition = {currentHoverGridPosition}, itemGridSize = {itemGridSize}", _coroutineRunner);

            // Проверяем, свободно ли место для размещения предмета
            if (_itemContainer.IsGridAreaFree(currentHoverGridPosition, itemGridSize))
            {
                _placementResults.Conflict = ReasonConflict.None;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Место свободно. Conflict: None", _coroutineRunner);
            }
            else
            {
                List<ItemVisual> overlappingItems = FindOverlappingItems(currentHoverGridPosition, itemGridSize, draggedItem);
                Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Место занято. Количество пересекающихся предметов: {overlappingItems.Count}", _coroutineRunner);

                if (overlappingItems.Count == 1)
                {
                    ItemVisual overlapItem = overlappingItems[0];
                    bool isSameStackableType = draggedItem.ItemDefinition.Stackable &&
                                               overlapItem.ItemDefinition.Stackable &&
                                               draggedItem.ItemDefinition.DefinitionName == overlapItem.ItemDefinition.DefinitionName;
                    
                    if (isSameStackableType)
                    {
                        _placementResults.Conflict = ReasonConflict.StackAvailable;
                        Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Пересечение с одним предметом. Тип: StackAvailable", _coroutineRunner);
                    }
                    else
                    {
                        _placementResults.Conflict = ReasonConflict.SwapAvailable;
                        Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Пересечение с одним предметом. Тип: SwapAvailable", _coroutineRunner);
                    }
                    _placementResults.OverlapItem = overlapItem;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                }
                else
                {
                    _placementResults.Conflict = ReasonConflict.intersectsObjects;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                    Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Пересечение с несколькими предметами или занято 'пустым' местом. Conflict: intersectsObjects", _coroutineRunner);
                }
            }

            // Дополнительная проверка на выход за границы, если IsGridAreaFree вернул true, но Conflict остался None (не должно случаться, но для безопасности)
            if (!_itemContainer.IsGridAreaFree(currentHoverGridPosition, itemGridSize) && _placementResults.Conflict == ReasonConflict.None)
            {
                _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
                Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Переопределен Conflict: beyondTheGridBoundary, так как IsGridAreaFree вернул false", _coroutineRunner);
            }

            // Управление видимостью телеграфа
            if (_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary || _placementResults.Conflict == ReasonConflict.intersectsObjects)
            {
                _telegraph.Hide();
                Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Telegraph скрыт из-за конфликта: {_placementResults.Conflict}", _coroutineRunner);
            }
            else
            {
                var pos = new Vector2(_placementResults.SuggestedGridPosition.x * CellSize.x, _placementResults.SuggestedGridPosition.y * CellSize.y);
                _telegraph.SetPosition(pos);
                _telegraph.SetPlacement(_placementResults.Conflict, itemGridSize.x * CellSize.x, itemGridSize.y * CellSize.y);
                Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Telegraph показан на позиции {pos} с размером {itemGridSize.x * CellSize.x}x{itemGridSize.y * CellSize.y}. Conflict: {_placementResults.Conflict}", _coroutineRunner);
            }

            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x * CellSize.x, _placementResults.SuggestedGridPosition.y * CellSize.y),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }

        protected Vector2Int CalculateCurrentHoverGridPosition()
        {
            Debug.Log($"[GridPageElementBase:{_root.name}] CalculateCurrentHoverGridPosition: _itemsPage.MouseUILocalPosition = {_itemsPage.MouseUILocalPosition}");
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);
            Debug.Log($"[GridPageElementBase:{_root.name}] CalculateCurrentHoverGridPosition: mouseLocalPosition (relative to _inventoryGrid) = {mouseLocalPosition}");
            Debug.Log($"[GridPageElementBase:{_root.name}] CalculateCurrentHoverGridPosition: CellSize = {CellSize}");
            Debug.Log($"[GridPageElementBase:{_root.name}] CalculateCurrentHoverGridPosition: _itemContainer.GridDimensions = {_itemContainer.GridDimensions}");
            Debug.Log($"[GridPageElementBase:{_root.name}] CalculateCurrentHoverGridPosition: _itemContainer.GridWorldRect = {_itemContainer.GridWorldRect}");
            Debug.Log($"[GridPageElementBase:{_root.name}] CalculateCurrentHoverGridPosition: _inventoryGrid.resolvedStyle.width = {_inventoryGrid.resolvedStyle.width}, _inventoryGrid.resolvedStyle.height = {_inventoryGrid.resolvedStyle.height}");

            int gridX = Mathf.FloorToInt(mouseLocalPosition.x / CellSize.x);
            int gridY = Mathf.FloorToInt(mouseLocalPosition.y / CellSize.y);
            
            Debug.Log($"[GridPageElementBase:{_root.name}] CalculateCurrentHoverGridPosition: Calculated gridX = {gridX}, gridY = {gridY}. Expected range: 0-{_itemContainer.GridDimensions.x -1}, 0-{_itemContainer.GridDimensions.y -1}");

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

        public virtual void FinalizeDrag() => _telegraph.Hide();

        public void SetLogicalGridVisualizerActive(bool active)
        {
            if (LogicalGridVisualizer != null)
            {
                LogicalGridVisualizer.IsEnabled = active;
            }
            else
            {
                Debug.LogWarning($"[GridPageElementBase:{_root.name}] Попытка установить состояние LogicalGridVisualizer до его инициализации.");
            }
        }

        public virtual void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)
        {
            // Этот метод вызывается, когда предмет из другого инвентаря бросают сюда.
            // Логика идентична Drop, так как это просто перемещение.
            _itemContainer.MoveItem(storedItem.ItemDefinition, gridPosition);
        }

        public virtual void RemoveStoredItem(ItemVisual storedItem)
        {
            _itemContainer.RemoveItem(GetItemDefinition(storedItem), true);
        }

        public virtual void PickUp(ItemVisual storedItem)
        {
            var itemDef = GetItemDefinition(storedItem);
            if (itemDef != null)
            {
                Debug.Log($"[GridPageElementBase:{_root.name}] PickUp: Освобождаем ячейки для предмета '{itemDef.name}' с GridPosition = {itemDef.GridPosition}", _coroutineRunner);
                _itemContainer.OccupyGridCells(itemDef, false);
            }
            ItemsPage.CurrentDraggedItem = storedItem;
            storedItem.SetOwnerInventory(this);
        }

        public virtual void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            // Этот метод вызывается, когда предмет бросают в пределах этого же инвентаря.
            _itemContainer.MoveItem(storedItem.ItemDefinition, gridPosition);
        }

        public ItemBaseDefinition GetItemDefinition(ItemVisual itemVisual)
        {
            _visuals.TryGetValue(itemVisual, out ItemGridData gridData);
            return gridData?.ItemDefinition;
        }

        public virtual void Dispose()
        {
            UnsubscribeFromContainerEvents();
        }
    }
}