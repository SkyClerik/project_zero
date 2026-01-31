using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public abstract class GridPageElementBase : IDropTarget
    {
        // Общие поля для управления сеткой и предметами
        protected Dictionary<ItemVisual, ItemGridData> _placedItemsGridData = new Dictionary<ItemVisual, ItemGridData>();
        protected bool[,] _gridOccupancy;
        protected Dictionary<ItemVisual, ItemGridData> _visualToGridDataMap = new Dictionary<ItemVisual, ItemGridData>();
        protected Rect _cellSize;
        protected Rect _gridRect;

        // Общие зависимости
        protected UIDocument _document;
        protected MonoBehaviour _coroutineRunner;
        protected ItemsPage _itemsPage;
        protected ItemContainerBase _itemContainer;

        // Общие UI-элементы и логика перетаскивания
        protected VisualElement _root;
        protected VisualElement _inventoryGrid;
        private const string _inventoryGridID = "grid";
        //protected Telegraph _telegraph;
        protected PlacementResults _placementResults;

        // Свойства для доступа к внутренним элементам
        public UIDocument GetDocument => _document;
        //public Telegraph Telegraph => _telegraph;
        public ItemContainerBase ItemContainer => _itemContainer;
        public Vector2 CellSize => new Vector2(_cellSize.width, _cellSize.height);
        public VisualElement Root => _root;
        public VisualElement InventoryGrid => _inventoryGrid;

        protected GridPageElementBase(ItemsPage itemsPage, UIDocument document, ItemContainerBase itemContainer, string rootID)
        {
            _itemsPage = itemsPage;
            _document = document;
            _coroutineRunner = itemsPage;
            _itemContainer = itemContainer;

            _root = _document.rootVisualElement.Q<VisualElement>(rootID);
            _inventoryGrid = _root.Q<VisualElement>(_inventoryGridID);

            _coroutineRunner.StartCoroutine(Initialize());
        }

        private void CalculateGridRect()
        {
            var firstBox = _inventoryGrid.ElementAt(0);
            Debug.Log($"firstBox: {firstBox.localBound}");
            _cellSize = firstBox.localBound;
            int widthCount = (int)(_inventoryGrid.localBound.width / _cellSize.width);
            int heightCount = (int)(_inventoryGrid.localBound.height / _cellSize.height);
            float gridWidth = (widthCount * _cellSize.width);
            float gridHeight = (heightCount * _cellSize.height);

            Vector2 position = firstBox.ChangeCoordinatesTo(_document.rootVisualElement, firstBox.localBound.position);
            Debug.Log($"firstBox.worldBound: {firstBox.worldBound}");
            // _gridRect должен содержать глобальную позицию и пиксельные размеры inventoryGrid
            _gridRect = new Rect(position.x, position.y, gridWidth, gridHeight);
            // _gridOccupancy должен быть массивом булевых значений, соответствующих количеству ячеек, а не пикселям
            _gridOccupancy = new bool[widthCount, heightCount];

            var test = new VisualElement();
            test.name = "Test";
            test.style.width = gridWidth;
            test.style.height = gridHeight;
            test.style.left = position.x;
            test.style.top = position.y;
            test.SetBorderColor(Color.blue);
            test.SetBorderWidth(5);
            test.style.position = Position.Absolute;
            test.pickingMode = PickingMode.Ignore;
            Debug.Log($"test: {test.localBound}");
            _document.rootVisualElement.Add(test);
        }

        protected IEnumerator Initialize()
        {
            yield return _coroutineRunner.StartCoroutine(Configure());
            yield return _coroutineRunner.StartCoroutine(LoadInventory());
        }

        protected IEnumerator Configure()
        {
            yield return new WaitForEndOfFrame();

            while (_document.rootVisualElement.layout.width <= 0 || _document.rootVisualElement.layout.height <= 0)
                yield return null; // Ждём следующего кадра

            yield return new WaitForEndOfFrame();
            CalculateGridRect();
        }

        protected IEnumerator LoadInventory()
        {
            if (!_itemContainer.GetItems().Any())
            {
                yield break;
            }

            foreach (var itemInContainer in _itemContainer.GetItems().ToList())
            {
                if (TryFindPlacement(itemInContainer, out Vector2Int gridPosition))
                {
                    ItemGridData newGridData = new ItemGridData(itemInContainer, gridPosition);

                    ItemVisual inventoryItemVisual = new ItemVisual(
                        itemsPage: _itemsPage,
                        ownerInventory: this,
                        itemDefinition: itemInContainer,
                        gridPosition: gridPosition,
                        gridSize: newGridData.GridSize);

                    _placedItemsGridData.Add(inventoryItemVisual, newGridData);
                    OccupyGridCells(newGridData, true);

                    AddItemToInventoryGrid(inventoryItemVisual);
                    RegisterVisual(inventoryItemVisual, newGridData);
                    inventoryItemVisual.SetPosition(new Vector2(gridPosition.x * _cellSize.width, gridPosition.y * _cellSize.height));
                }
                else
                {
                    Debug.LogWarning($"[{GetType().Name}.LoadInventory] Не удалось разместить предмет {itemInContainer.name} в инвентаре. Возможно, нет места.");
                    _itemContainer.RemoveItem(itemInContainer);
                }
            }
            yield break;
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            _inventoryGrid.Add(item);
        }

        public void AddLoot(ItemContainerBase sourceContainer)
        {
            var itemsToLoot = sourceContainer.GetItems().ToList();
            var sortedLoot = itemsToLoot.OrderByDescending(item =>
                item.Dimensions.DefaultWidth * item.Dimensions.DefaultHeight).ToList();

            var successfullyAddedOriginals = new List<ItemBaseDefinition>();

            foreach (var item in sortedLoot)
            {
                if (TryAddItemInternal(item))
                {
                    successfullyAddedOriginals.Add(item);
                }
            }

            foreach (var originalItem in successfullyAddedOriginals)
            {
                sourceContainer.RemoveItem(originalItem, destroy: false);
            }
        }

        private bool TryAddItemInternal(ItemBaseDefinition itemToAdd)
        {
            if (itemToAdd.Stackable)
            {
                foreach (var visual in _visualToGridDataMap.Keys.ToList())
                {
                    if (itemToAdd.Stack <= 0) break;

                    var existingItemDef = visual.ItemDefinition;
                    if (existingItemDef.Stackable &&
                        existingItemDef.DefinitionName == itemToAdd.DefinitionName &&
                        existingItemDef.Stack < existingItemDef.MaxStack)
                    {
                        int spaceAvailable = existingItemDef.MaxStack - existingItemDef.Stack;
                        int amountToTransfer = Mathf.Min(spaceAvailable, itemToAdd.Stack);

                        if (amountToTransfer > 0)
                        {
                            existingItemDef.AddStack(amountToTransfer, out _);
                            itemToAdd.RemoveStack(amountToTransfer);
                            visual.UpdatePcs();
                        }
                    }
                }
            }

            if (itemToAdd.Stack <= 0)
            {
                return true;
            }

            if (TryFindPlacement(itemToAdd, out Vector2Int position))
            {
                var clonedItem = _itemContainer.AddItemAsClone(itemToAdd);
                if (clonedItem == null) return false;

                ItemGridData newGridData = new ItemGridData(clonedItem, position);

                ItemVisual newItemVisual = new ItemVisual(
                    itemsPage: _itemsPage,
                    ownerInventory: this,
                    itemDefinition: clonedItem,
                    gridPosition: position,
                    gridSize: newGridData.GridSize);

                _placedItemsGridData.Add(newItemVisual, newGridData);
                OccupyGridCells(newGridData, true);
                RegisterVisual(newItemVisual, newGridData);
                AddItemToInventoryGrid(newItemVisual);
                newItemVisual.SetPosition(new Vector2(position.x * _cellSize.width, position.y * _cellSize.height));

                return true;
            }

            return false;
        }

        protected void RemoveItemFromInventoryGrid(VisualElement item)
        {
            _inventoryGrid.Remove(item);
        }

        protected static void SetItemPosition(VisualElement element, Vector2 vector)
        {
            element.style.left = vector.x;
            element.style.top = vector.y;
        }

        protected void OccupyGridCells(ItemGridData gridData, bool occupy)
        {
            for (int y = 0; y < gridData.GridSize.y; y++)
            {
                for (int x = 0; x < gridData.GridSize.x; x++)
                {
                    _gridOccupancy[gridData.GridPosition.x + x, gridData.GridPosition.y + y] = occupy;
                }
            }
        }

        public bool IsGridAreaFree(Vector2Int start, Vector2Int size)
        {
            // Проверка на выход за границы _gridOccupancy (количество ячеек)
            if (start.x < 0 || start.y < 0 ||
                start.x + size.x > _gridOccupancy.GetLength(0) || // Используем количество ячеек по X
                start.y + size.y > _gridOccupancy.GetLength(1))   // Используем количество ячеек по Y
                return false;

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int currentX = start.x + x;
                    int currentY = start.y + y;
                    // Эти проверки становятся излишними, так как они уже покрыты выше
                    // if (currentX >= _gridOccupancy.GetLength(0) || currentY >= _gridOccupancy.GetLength(1) || currentX < 0 || currentY < 0)
                    //     return false;

                    if (_gridOccupancy[currentX, currentY])
                        return false;
                }
            }
            return true;
        }

        public virtual bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.DefaultWidth, item.Dimensions.DefaultHeight);

            // Используем размеры _gridOccupancy (количество ячеек) для итерации
            for (int y = 0; y <= _gridOccupancy.GetLength(1) - itemGridSize.y; y++)
            {
                for (int x = 0; x <= _gridOccupancy.GetLength(0) - itemGridSize.x; x++)
                {
                    Vector2Int currentPosition = new Vector2Int(x, y);
                    if (IsGridAreaFree(currentPosition, itemGridSize))
                    {
                        suggestedGridPosition = currentPosition;
                        return true;
                    }
                }
            }

            suggestedGridPosition = Vector2Int.zero;
            return false;
        }

        public virtual PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            if (draggedItem == null)
            {
                Debug.LogError("[ShowPlacementTarget] draggedItem is null!");
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }
            // Сначала переводим глобальные координаты мыши в локальные координаты _inventoryGrid
            Vector2 mouseLocalPositionInGrid = _inventoryGrid.WorldToLocal(Input.mousePosition);

            // Проверяем, находится ли курсор внутри границ _inventoryGrid
            // localBound - это прямоугольник, описывающий элемент в его локальной системе координат.
            if (!_inventoryGrid.localBound.Contains(mouseLocalPositionInGrid))
            {
                if (_itemsPage == null)
            {
                Debug.LogError("[ShowPlacementTarget] _itemsPage is null!");
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }
            if (_itemsPage.Telegraph == null)
            {
                Debug.LogError("[ShowPlacementTarget] _itemsPage.Telegraph is null!");
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }
            _itemsPage.Telegraph.Hide();
                // Возвращаем результат, указывающий, что мышь за пределами сетки
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            Vector2Int itemGridSize = new Vector2Int(draggedItem.ItemDefinition.Dimensions.CurrentWidth, draggedItem.ItemDefinition.Dimensions.CurrentHeight);

            _placementResults = new PlacementResults();
            _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
            _placementResults.OverlapItem = null;

            // Всегда определяем пересекающиеся предметы, так как это влияет на Swap
            List<ItemVisual> overlappingItems = FindOverlappingItems(currentHoverGridPosition, itemGridSize, draggedItem);

            // 1. Проверка на выход за границы
            if (currentHoverGridPosition.x < 0 || currentHoverGridPosition.y < 0 ||
                currentHoverGridPosition.x + itemGridSize.x > _gridOccupancy.GetLength(0) ||
                currentHoverGridPosition.y + itemGridSize.y > _gridOccupancy.GetLength(1))
            {
                _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
            }
            // 2. Проверка на пересечение с предметами (Swap или Multiple Intersect)
            else if (overlappingItems.Count == 1)
            {
                ItemVisual overlapItem = overlappingItems[0];

                bool isSameStackableType = draggedItem.ItemDefinition.Stackable &&
                                           overlapItem.ItemDefinition.Stackable &&
                                           draggedItem.ItemDefinition.DefinitionName == overlapItem.ItemDefinition.DefinitionName;

                if (isSameStackableType)
                {
                    _placementResults.Conflict = ReasonConflict.StackAvailable;
                }
                else
                {
                    _placementResults.Conflict = ReasonConflict.SwapAvailable;
                }

                _placementResults.OverlapItem = overlapItem;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }
            else if (overlappingItems.Count > 1)
            {
                // Пересечение с несколькими предметами - конфликт
                _placementResults.Conflict = ReasonConflict.intersectsObjects;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }
            // 3. Если нет пересечений с предметами, проверяем, свободно ли место
            else if (IsGridAreaFree(currentHoverGridPosition, itemGridSize))
            {
                _placementResults.Conflict = ReasonConflict.None;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }
            // 4. Если место занято, но нет пересечений с предметами (например, занято "пустотой")
            else
            {
                _placementResults.Conflict = ReasonConflict.intersectsObjects;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }

            if (_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary || _placementResults.Conflict == ReasonConflict.intersectsObjects)
            {
                Debug.Log($"_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary");
                if (_itemsPage == null)
            {
                Debug.LogError("[ShowPlacementTarget] _itemsPage is null!");
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }
            if (_itemsPage.Telegraph == null)
            {
                Debug.LogError("[ShowPlacementTarget] _itemsPage.Telegraph is null!");
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }
            _itemsPage.Telegraph.Hide();
            }
            else
            {
                // Получаем WorldToLocal позицию _inventoryGrid относительно rootVisualElement (родителя Telegraph)
                Vector2 inventoryGridPosInRoot = _inventoryGrid.ChangeCoordinatesTo(_itemsPage.Telegraph.parent, Vector2.zero);

                // Вычисляем позицию Telegraph в пикселях относительно _document.rootVisualElement
                float telegraphPixelX = inventoryGridPosInRoot.x + _placementResults.SuggestedGridPosition.x * _cellSize.width;
                float telegraphPixelY = inventoryGridPosInRoot.y + _placementResults.SuggestedGridPosition.y * _cellSize.height;

                // Debugging for Telegraph position
                Debug.Log($"[Telegraph Debug] _inventoryGrid.worldBound: {_inventoryGrid.worldBound}");
                Debug.Log($"[Telegraph Debug] inventoryGridPosInRoot: {inventoryGridPosInRoot}");
                Debug.Log($"[Telegraph Debug] SuggestedGridPosition: {_placementResults.SuggestedGridPosition}");
                Debug.Log($"[Telegraph Debug] CellSize: {_cellSize}");
                Debug.Log($"[Telegraph Debug] Calculated telegraphPixelX: {telegraphPixelX}");
                Debug.Log($"[Telegraph Debug] Calculated telegraphPixelY: {telegraphPixelY}");

                _itemsPage.Telegraph.SetPosition(new Vector2(telegraphPixelX, telegraphPixelY));
                _itemsPage.Telegraph.SetPlacement(_placementResults.Conflict, itemGridSize.x * _cellSize.width, itemGridSize.y * _cellSize.height);
            }

            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x * _cellSize.width, _placementResults.SuggestedGridPosition.y * _cellSize.height),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }

        protected Vector2Int CalculateCurrentHoverGridPosition_WithMargin()
        {
            Vector2 mouseScreenPosition = Input.mousePosition;
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(mouseScreenPosition);

            float gridWidthInPixels = _inventoryGrid.resolvedStyle.width;
            float gridHeightInPixels = _inventoryGrid.resolvedStyle.height;

            float halfCellWidth = _cellSize.width / 2f;
            float halfCellHeight = _cellSize.height / 2f;

            float finalX = mouseLocalPosition.x;
            if (finalX < 0 && finalX >= -halfCellWidth)
            {
                finalX = 0;
            }
            else if (finalX > gridWidthInPixels && finalX <= gridWidthInPixels + halfCellWidth)
            {
                finalX = gridWidthInPixels - 0.001f;
            }

            float finalY = mouseLocalPosition.y;
            if (finalY < 0 && finalY >= -halfCellHeight)
            {
                finalY = 0;
            }
            else if (finalY > gridHeightInPixels && finalY <= gridHeightInPixels + halfCellHeight)
            {
                finalY = gridHeightInPixels - 0.001f;
            }

            int gridX = Mathf.FloorToInt(finalX / _cellSize.width);
            int gridY = Mathf.FloorToInt((gridHeightInPixels - finalY) / _cellSize.height);

            return new Vector2Int(gridX, gridY);
        }

        protected Vector2Int CalculateCurrentHoverGridPosition()
        {
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(Input.mousePosition);

            float contentX = mouseLocalPosition.x;
            float contentY = mouseLocalPosition.y;

            // Вычисляем смещение первого item_box относительно левого края _inventoryGrid
            // Это необходимо, если justify-content: center; или другие стили центрируют ячейки
            float offsetX = _inventoryGrid.ElementAt(0).worldBound.x - _inventoryGrid.worldBound.x;
            float offsetY = _inventoryGrid.ElementAt(0).worldBound.y - _inventoryGrid.worldBound.y;

            // Вычитаем смещение из координат мыши
            contentX -= offsetX;
            contentY -= offsetY;

            int gridX = Mathf.FloorToInt(contentX / _cellSize.width);
            // Пользовательская логика: Y инвертируется. Сохраняем как есть.
            int gridY = Mathf.FloorToInt((_gridRect.height - contentY) / _cellSize.height);

            return new Vector2Int(gridX, gridY);
        }

        protected List<ItemVisual> FindOverlappingItems(Vector2Int start, Vector2Int size, ItemVisual draggedItem)
        {
            List<ItemVisual> overlappingItems = new List<ItemVisual>();
            RectInt targetRect = new RectInt(start.x, start.y, size.x, size.y);

            foreach (var entry in _visualToGridDataMap)
            {
                ItemVisual currentItem = entry.Key;
                if (currentItem == draggedItem) continue; // Игнорируем сам перетаскиваемый предмет

                ItemGridData gridData = entry.Value;
                RectInt currentItemRect = new RectInt(gridData.GridPosition.x, gridData.GridPosition.y, gridData.GridSize.x, gridData.GridSize.y);

                if (targetRect.Overlaps(currentItemRect))
                    overlappingItems.Add(currentItem);
            }
            return overlappingItems;
        }

        public virtual void FinalizeDrag()
        {
            Debug.Log($"FinalizeDrag");
            _itemsPage.Telegraph.Hide();
        }

        public virtual void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)
        {
            ItemGridData gridData;
            if (_placedItemsGridData.TryGetValue(storedItem, out var existingGridData))
            {
                gridData = existingGridData;
                OccupyGridCells(gridData, false);
                gridData.GridPosition = gridPosition;
            }
            else
            {
                gridData = new ItemGridData(storedItem.ItemDefinition, gridPosition);
                _placedItemsGridData.Add(storedItem, gridData);
            }

            OccupyGridCells(gridData, true);
            RegisterVisual(storedItem, gridData);
            AddItemToInventoryGrid(storedItem);

            storedItem.SetPosition(new Vector2(gridPosition.x * _cellSize.width, gridPosition.y * _cellSize.height));
            storedItem.SetOwnerInventory(this);
        }

        public virtual void RemoveStoredItem(ItemVisual storedItem)
        {
            if (_visualToGridDataMap.TryGetValue(storedItem, out ItemGridData gridData))
            {
                OccupyGridCells(gridData, false);
                _placedItemsGridData.Remove(storedItem);
                UnregisterVisual(storedItem);
                storedItem.RemoveFromHierarchy();
            }
        }

        public virtual void PickUp(ItemVisual storedItem)
        {
            if (_placedItemsGridData.TryGetValue(storedItem, out ItemGridData gridData))
            {
                OccupyGridCells(gridData, false);
                _placedItemsGridData.Remove(storedItem);
            }
            UnregisterVisual(storedItem);
            ItemsPage.CurrentDraggedItem = storedItem;
            storedItem.SetOwnerInventory(this);
        }

        public virtual void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            AddStoredItem(storedItem, gridPosition);
        }

        public virtual ItemGridData GetItemGridData(ItemVisual itemVisual)
        {
            _placedItemsGridData.TryGetValue(itemVisual, out ItemGridData gridData);
            return gridData;
        }

        public virtual void RegisterVisual(ItemVisual visual, ItemGridData gridData)
        {
            if (!_visualToGridDataMap.ContainsKey(visual))
                _visualToGridDataMap.Add(visual, gridData);
        }

        public virtual void UnregisterVisual(ItemVisual visual)
        {
            if (_visualToGridDataMap.ContainsKey(visual))
                _visualToGridDataMap.Remove(visual);
        }
    }
}