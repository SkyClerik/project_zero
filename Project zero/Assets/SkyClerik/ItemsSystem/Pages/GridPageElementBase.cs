using SkyClerik.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public abstract class GridPageElementBase : IDropTarget
    {
        // Общие поля для управления сеткой и предметами
        protected Dictionary<ItemVisual, ItemGridData> _placedItemsGridData = new Dictionary<ItemVisual, ItemGridData>();
        protected bool[,] _gridOccupancy;
        protected Dictionary<ItemVisual, ItemGridData> _visualToGridDataMap = new Dictionary<ItemVisual, ItemGridData>();
        protected RectangleSize _inventoryDimensions;
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
        protected Telegraph _telegraph;
        protected PlacementResults _placementResults;

        // Свойства для доступа к внутренним элементам
        public UIDocument GetDocument => _document;
        public Telegraph Telegraph => _telegraph;
        public ItemContainerBase ItemContainer => _itemContainer;
        public Vector2 CellSize => new Vector2(_cellSize.width, _cellSize.height);
        public VisualElement Root => _root;

        protected GridPageElementBase(ItemsPage itemsPage, UIDocument document, ItemContainerBase itemContainer, Vector2 cellSize, Vector2Int inventoryGridSize, string rootID)
        {
            _itemsPage = itemsPage;
            _document = document;
            _coroutineRunner = itemsPage;
            _itemContainer = itemContainer;

            _root = _document.rootVisualElement.Q<VisualElement>(rootID);
            _inventoryGrid = _root.Q<VisualElement>(_inventoryGridID);

            _cellSize = new Rect(0, 0, cellSize.x, cellSize.y);
            _inventoryDimensions.width = inventoryGridSize.x;
            _inventoryDimensions.height = inventoryGridSize.y;
            _gridOccupancy = new bool[_inventoryDimensions.width, _inventoryDimensions.height];

            _coroutineRunner.StartCoroutine(Initialize());
        }

        protected IEnumerator Initialize()
        {
            yield return _coroutineRunner.StartCoroutine(Configure());
            yield return _coroutineRunner.StartCoroutine(LoadInventory());
        }

        protected IEnumerator Configure()
        {
            _telegraph = new Telegraph();
            AddItemToInventoryGrid(_telegraph);

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

        protected virtual void CalculateGridRect()
        {
            // Получаем оригинальные границы
            Rect originalRect = _inventoryGrid.worldBound;

            // Считаем отступ в половину ячейки
            float marginX = _cellSize.width / 2f;
            float marginY = _cellSize.height / 2f;

            // Создаём новый, расширенный прямоугольник для "зоны захвата"
            _gridRect = new Rect(
                originalRect.x - marginX,
                originalRect.y - marginY,
                originalRect.width + marginX * 2,
                originalRect.height + marginY * 2
            );
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
            if (start.x < 0 || start.y < 0 || start.x + size.x > _inventoryDimensions.width || start.y + size.y > _inventoryDimensions.height)
                return false;

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int currentX = start.x + x;
                    int currentY = start.y + y;
                    if (currentX >= _gridOccupancy.GetLength(0) || currentY >= _gridOccupancy.GetLength(1) || currentX < 0 || currentY < 0)
                        return false;

                    if (_gridOccupancy[currentX, currentY])
                        return false;
                }
            }
            return true;
        }

        public virtual bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.DefaultWidth, item.Dimensions.DefaultHeight);

            for (int y = 0; y <= _inventoryDimensions.height - itemGridSize.y; y++)
            {
                for (int x = 0; x <= _inventoryDimensions.width - itemGridSize.x; x++)
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
            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            Vector2Int itemGridSize = new Vector2Int(draggedItem.ItemDefinition.Dimensions.CurrentWidth, draggedItem.ItemDefinition.Dimensions.CurrentHeight);

            _placementResults = new PlacementResults();
            _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
            _placementResults.OverlapItem = null;

            // Всегда определяем пересекающиеся предметы, так как это влияет на Swap
            List<ItemVisual> overlappingItems = FindOverlappingItems(currentHoverGridPosition, itemGridSize, draggedItem);

            // 1. Проверка на выход за границы
            if (currentHoverGridPosition.x < 0 || currentHoverGridPosition.y < 0 ||
                currentHoverGridPosition.x + itemGridSize.x > _inventoryDimensions.width ||
                currentHoverGridPosition.y + itemGridSize.y > _inventoryDimensions.height)
            {
                _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
            }
            // 2. Проверка на пересечение с предметами (Swap или Multiple Intersect)
            else if (overlappingItems.Count == 1)
            {
                // Пересечение с одним предметом - возможен Swap
                ItemVisual overlapItem = overlappingItems[0];
                _placementResults.Conflict = ReasonConflict.SwapAvailable;
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

            if (_placementResults.Conflict == ReasonConflict.None || _placementResults.Conflict == ReasonConflict.SwapAvailable)
            {
                _telegraph.style.left = _placementResults.SuggestedGridPosition.x * _cellSize.width;
                _telegraph.style.top = _placementResults.SuggestedGridPosition.y * _cellSize.height;
                _telegraph.style.width = itemGridSize.x * _cellSize.width;
                _telegraph.style.height = itemGridSize.y * _cellSize.height;
                _telegraph.style.display = DisplayStyle.Flex;
            }
            else
            {
                _telegraph.Hide();
            }

            _telegraph.SetPlacement(_placementResults.Conflict, itemGridSize.x * _cellSize.width, itemGridSize.y * _cellSize.height); // Передаем размеры
            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x * _cellSize.width, _placementResults.SuggestedGridPosition.y * _cellSize.height),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }

        protected Vector2Int CalculateCurrentHoverGridPosition()
        {
            // Получаем позицию мыши в экранных координатах
            Vector2 mouseScreenPosition = Input.mousePosition;
            // Преобразуем ее в локальные координаты _inventoryGrid.
            // Так как _gridRect теперь больше, mouseLocalPosition может быть отрицательной или выходить за правую/нижнюю границу.
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(mouseScreenPosition);

            float gridWidthInPixels = _inventoryGrid.resolvedStyle.width;
            float gridHeightInPixels = _inventoryGrid.resolvedStyle.height;

            float halfCellWidth = _cellSize.width / 2f;
            float halfCellHeight = _cellSize.height / 2f;

            // Округляем позицию до ближайшей ячейки, если курсор в "зоне захвата".
            // Эта логика теперь работает в паре с расширенным _gridRect.
            float finalX = mouseLocalPosition.x;
            if (finalX < 0 && finalX >= -halfCellWidth)
            {
                finalX = 0;
            }
            else if (finalX > gridWidthInPixels && finalX <= gridWidthInPixels + halfCellWidth)
            {
                finalX = gridWidthInPixels - 0.001f; // -epsilon чтобы остаться в последней ячейке
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

            // Вычисляем позицию в сетке
            int gridX = Mathf.FloorToInt(finalX / _cellSize.width);
            // Инвертируем Y-координату, так как UI Toolkit использует верхний левый угол как (0,0)
            int gridY = Mathf.FloorToInt((gridHeightInPixels - finalY) / _cellSize.height);

            Vector2Int currentHoverGridPosition = new Vector2Int(gridX, gridY);

            return currentHoverGridPosition;
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
                {
                    overlappingItems.Add(currentItem);
                }
            }
            return overlappingItems;
        }

        public virtual void FinalizeDrag()
        {
            _telegraph.Hide();
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
            {
                _visualToGridDataMap.Add(visual, gridData);
            }
        }

        public virtual void UnregisterVisual(ItemVisual visual)
        {
            if (_visualToGridDataMap.ContainsKey(visual))
            {
                _visualToGridDataMap.Remove(visual);
            }
        }
    }
}