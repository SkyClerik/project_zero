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
        // Общие поля для управления сеткой и предметами
        protected Dictionary<ItemVisual, ItemGridData> _placedItemsGridData = new Dictionary<ItemVisual, ItemGridData>();
        protected bool[,] _gridOccupancy;

        //protected RectangleSize _inventoryDimensions;
        protected Rect _cellSize;
        protected Rect _gridRect;

        // Общие зависимости
        protected UIDocument _document;
        protected MonoBehaviour _coroutineRunner;
        protected ItemsPage _itemsPage;
        protected ItemContainer _itemContainer;

        // Общие UI-элементы и логика перетаскивания
        protected VisualElement _root;
        protected VisualElement _inventoryGrid;
        private const string _inventoryGridID = "grid";
        protected Telegraph _telegraph;
        protected PlacementResults _placementResults;
        private float _gridHoverSnapToBoundaryPixels = 20f; // Размер буферной зоны в пикселях для "притягивания" курсора к границам сетки

        // Свойства для доступа к внутренним элементам
        public UIDocument GetDocument => _document;
        public Telegraph Telegraph => _telegraph;
        public ItemContainer ItemContainer => _itemContainer;
        public Vector2 CellSize => new Vector2(_cellSize.width, _cellSize.height);
        public VisualElement Root => _root;

        protected GridPageElementBase(ItemsPage itemsPage, UIDocument document, ItemContainer itemContainer, string rootID)
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
            yield return _coroutineRunner.StartCoroutine(Configure());

            yield return new WaitForEndOfFrame();

            CalculateGridRect();
            yield return _coroutineRunner.StartCoroutine(LoadInventory());
        }

        protected IEnumerator Configure()
        {
            _telegraph = new Telegraph();
            AddItemToInventoryGrid(_telegraph);
            yield break;
        }

        protected IEnumerator LoadInventory()
        {
            if (!_itemContainer.GetItems().Any())
            {
                yield break;
            }

            // Добавляем предметы в сетку с использованием новой логики AddItemsToGrid
            List<ItemBaseDefinition> unplacedItems = AddItemsToGrid(_itemContainer.GetItems().ToList());

            if (unplacedItems.Any())
            {
                foreach (var item in unplacedItems)
                {
                    Debug.LogWarning($"[{GetType().Name}.LoadInventory] Не удалось разместить предмет {item.name} в инвентаре после загрузки. Возможно, нет места.");
                    _itemContainer.RemoveItem(item); // Удаляем неразмещенные предметы из контейнера
                }
            }
            yield break;
        }

        protected virtual void CalculateGridRect()
        {
            // Получаем оригинальные границы
            Rect originalRect = _inventoryGrid.worldBound;
            // Нахожу реальный размер ячейки
            var firstBox = _inventoryGrid.ElementAt(0);
            _cellSize = firstBox.localBound;
            // Считаю кол-во в ширину и высоту
            int widthCount = (int)(_inventoryGrid.localBound.width / _cellSize.width);
            int heightCount = (int)(_inventoryGrid.localBound.height / _cellSize.height);
            Debug.Log($"{_root.name} : x={originalRect.x}   y={originalRect.y}   w={originalRect.width}   h={originalRect.height}   wc={widthCount}   hc={heightCount}");

            _gridHoverSnapToBoundaryPixels = _cellSize.width / 2;
            _gridRect = new Rect(originalRect.x - _gridHoverSnapToBoundaryPixels, originalRect.y - _gridHoverSnapToBoundaryPixels, originalRect.width + _cellSize.width, originalRect.height + _cellSize.width);
            //_gridRect = new Rect(originalRect.x, originalRect.y, originalRect.width, originalRect.height);
            // Создаю матрицу проходимости
            _gridOccupancy = new bool[widthCount, heightCount];

            var test = new VisualElement();
            test.name = "Test";
            test.style.width = _gridRect.width;
            test.style.height = _gridRect.height;
            test.style.left = _gridRect.x;
            test.style.top = _gridRect.y;
            test.SetBorderColor(Color.blue);
            test.SetBorderWidth(5);
            test.style.position = Position.Absolute;
            test.pickingMode = PickingMode.Ignore;
            _document.rootVisualElement.Add(test);
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            _inventoryGrid.Add(item);
        }

        /// <summary>
        /// Размещает список предметов в сетке инвентаря. Сортирует по размеру, ищет место,
        /// устанавливает GridPosition для размещенных предметов и создает ItemVisual, если UI активен.
        /// </summary>
        /// <param name="itemsToAdd">Список предметов для добавления.</param>
        /// <returns>Список предметов, которые не удалось разместить.</returns>
        public List<ItemBaseDefinition> AddItemsToGrid(List<ItemBaseDefinition> itemsToAdd)
        {
            Debug.Log($"[{GetType().Name}.AddItemsToGrid] Вызван AddItemsToGrid с {itemsToAdd.Count} предметами.");
            List<ItemBaseDefinition> unplacedItems = new List<ItemBaseDefinition>();

            // Сортируем предметы по размеру (от большего к меньшему)
            var sortedItems = itemsToAdd.OrderByDescending(item =>
                item.Dimensions.CurrentWidth * item.Dimensions.CurrentHeight).ToList();

            foreach (var item in sortedItems)
            {
                Vector2Int itemGridSize = new Vector2Int(item.Dimensions.DefaultWidth, item.Dimensions.DefaultHeight);
                Vector2Int foundPosition;

                if (TryFindPlacement(item, out foundPosition))
                {
                    Debug.Log($"[{GetType().Name}.AddItemsToGrid] Для предмета '{item.name}' ({itemGridSize.x}x{itemGridSize.y}) найдена позиция: {foundPosition}.");
                    // Место найдено, устанавливаем позицию в предмете и занимаем ячейки
                    item.GridPosition = foundPosition;
                    Debug.Log($"[{GetType().Name}.AddItemsToGrid] После присвоения, GridPosition предмета '{item.name}' = {item.GridPosition}.");
                    OccupyGridCells(new ItemGridData(item, foundPosition), true); // Занимаем ячейки в логической матрице

                    // Если UI активен, создаем и размещаем ItemVisual
                    if (_inventoryGrid != null && _inventoryGrid.resolvedStyle.display != DisplayStyle.None) // Проверяем, что UI-сетка существует и видна
                    {
                        ItemGridData newGridData = new ItemGridData(item, foundPosition);
                        ItemVisual newItemVisual = new ItemVisual(
                            itemsPage: _itemsPage,
                            ownerInventory: this,
                            itemDefinition: item,
                            gridPosition: foundPosition,
                            gridSize: newGridData.GridSize);

                        _placedItemsGridData.Add(newItemVisual, newGridData); // Добавляем в список размещенных визуальных элементов
                        AddItemToInventoryGrid(newItemVisual); // Добавляем в UI-сетку
                        newItemVisual.SetPosition(new Vector2(foundPosition.x * _cellSize.width, foundPosition.y * _cellSize.height));
                    }
                }
                else
                {
                    Debug.LogWarning($"[{GetType().Name}.AddItemsToGrid] Не удалось найти место для предмета '{item.name}' ({itemGridSize.x}x{itemGridSize.y}).");
                    // Место не найдено
                    unplacedItems.Add(item);
                }
            }
            return unplacedItems;
        }

        public void AddLoot(LutContainer sourceLut)
        {
            if (sourceLut == null || !sourceLut.Items.Any()) return;

            // 1. Клонируем все предметы из исходного контейнера
            var clonedItems = sourceLut.Items.Select(item => Object.Instantiate(item)).ToList();

            // 2. Сразу очищаем исходный контейнер
            sourceLut.Items.Clear();

            // Сортируем клоны для оптимального размещения
            var sortedClones = clonedItems.OrderByDescending(item =>
                item.Dimensions.DefaultWidth * item.Dimensions.DefaultHeight).ToList();

            foreach (var clone in sortedClones)
            {
                if (clone == null) continue;

                // 3. Пытаемся забрать клон
                bool wasFullyPlaced = TryAddItemInternal(clone);

                if (!wasFullyPlaced)
                {
                    // 4. Если не влезло - возвращаем остаток в (теперь уже пустой) LutContainer
                    sourceLut.Items.Add(clone);
                }
                else if (clone.Stack <= 0)
                {
                    // Если предмет был полностью поглощен (например, ушел в стак),
                    // а сам объект-клон остался с нулевым количеством, уничтожаем его, чтобы не мусорить в сцене.
                    Object.Destroy(clone);
                }
            }
        }

        private bool TryAddItemInternal(ItemBaseDefinition itemToAdd)
        {
            if (itemToAdd.Stackable)
            {
                foreach (var visual in _placedItemsGridData.Keys.ToList())
                {
                    if (itemToAdd.Stack <= 0)
                        break;

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
                return true; // Предмет полностью стакнулся
            }

            // Используем новый AddItemsToGrid для размещения оставшегося предмета
            List<ItemBaseDefinition> unplacedItems = AddItemsToGrid(new List<ItemBaseDefinition> { itemToAdd });

            return !unplacedItems.Any(); // Возвращаем true, если предмет был размещен
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
            if (start.x < 0 || start.y < 0 || start.x + size.x > _gridOccupancy.GetLength(0) || start.y + size.y > _gridOccupancy.GetLength(1))
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
            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            Vector2Int itemGridSize = new Vector2Int(draggedItem.ItemDefinition.Dimensions.CurrentWidth, draggedItem.ItemDefinition.Dimensions.CurrentHeight);

            //Debug.Log($"[ЛОГ] ShowPlacementTarget в '{_root.name}'. Мышь в коорд. сетки: {currentHoverGridPosition}. Размер предмета: {itemGridSize}. Границы сетки _gridRect: {_gridRect}");

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
                //Debug.Log($"[ЛОГ] Причина конфликта: {ReasonConflict.beyondTheGridBoundary}");
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
                //Debug.Log($"[ЛОГ] Причина конфликта: {_placementResults.Conflict}");

                _placementResults.OverlapItem = overlapItem;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }
            else if (overlappingItems.Count > 1)
            {
                // Пересечение с несколькими предметами - конфликт
                _placementResults.Conflict = ReasonConflict.intersectsObjects;
                //Debug.Log($"[ЛОГ] Причина конфликта: {ReasonConflict.intersectsObjects} (Пересечение с {overlappingItems.Count} предметами)");
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }
            // 3. Если нет пересечений с предметами, проверяем, свободно ли место
            else if (IsGridAreaFree(currentHoverGridPosition, itemGridSize))
            {
                _placementResults.Conflict = ReasonConflict.None;
                //Debug.Log($"[ЛОГ] Причина конфликта: {ReasonConflict.None}");
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }
            // 4. Если место занято, но нет пересечений с предметами (например, занято "пустотой")
            else
            {
                _placementResults.Conflict = ReasonConflict.intersectsObjects;
                //Debug.Log($"[ЛОГ] Причина конфликта: {ReasonConflict.intersectsObjects} (Место занято)");
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }

            if (_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary || _placementResults.Conflict == ReasonConflict.intersectsObjects)
            {
                _telegraph.Hide();
            }
            else
            {
                var pos = new Vector2(_placementResults.SuggestedGridPosition.x * _cellSize.width, _placementResults.SuggestedGridPosition.y * _cellSize.height);
                _telegraph.SetPosition(pos);
                _telegraph.SetPlacement(_placementResults.Conflict, itemGridSize.x * _cellSize.width, itemGridSize.y * _cellSize.height);
            }

            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x * _cellSize.width, _placementResults.SuggestedGridPosition.y * _cellSize.height),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }

        protected Vector2Int CalculateCurrentHoverGridPosition()
        {
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);

            // --- Оригинальная реализация CalculateCurrentHoverGridPosition (закомментирована) ---
            // int gridX = Mathf.FloorToInt(mouseLocalPosition.x / _cellSize.width);
            // int gridY = Mathf.FloorToInt(mouseLocalPosition.y / _cellSize.height);
            // return new Vector2Int(gridX, gridY);
            // ----------------------------------------------------------------------------------

            // Корректируем mouseLocalPosition так, чтобы она "притягивалась" к границам сетки,
            // если находится в пределах буферной зоны
            float adjustedX = mouseLocalPosition.x;
            float adjustedY = mouseLocalPosition.y;

            // Если курсор чуть-чуть слева от сетки (в буферной зоне), притягиваем его к 0
            if (adjustedX < 0 && adjustedX > -_gridHoverSnapToBoundaryPixels)
            {
                adjustedX = 0;
            }
            // Если курсор чуть-чуть справа от сетки, притягиваем его к краю
            else if (adjustedX > _inventoryGrid.localBound.width && adjustedX < _inventoryGrid.localBound.width + _gridHoverSnapToBoundaryPixels)
            {
                adjustedX = _inventoryGrid.localBound.width;
            }

            // Аналогично для Y
            if (adjustedY < 0 && adjustedY > -_gridHoverSnapToBoundaryPixels)
            {
                adjustedY = 0;
            }
            else if (adjustedY > _inventoryGrid.localBound.height && adjustedY < _inventoryGrid.localBound.height + _gridHoverSnapToBoundaryPixels)
            {
                adjustedY = _inventoryGrid.localBound.height;
            }

            int gridX = Mathf.FloorToInt(adjustedX / _cellSize.width);
            int gridY = Mathf.FloorToInt(adjustedY / _cellSize.height);
            return new Vector2Int(gridX, gridY);
        }


        protected List<ItemVisual> FindOverlappingItems(Vector2Int start, Vector2Int size, ItemVisual draggedItem)
        {
            List<ItemVisual> overlappingItems = new List<ItemVisual>();
            RectInt targetRect = new RectInt(start.x, start.y, size.x, size.y);

            foreach (var entry in _placedItemsGridData)
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

            AddItemToInventoryGrid(storedItem);

            storedItem.SetPosition(new Vector2(gridPosition.x * _cellSize.width, gridPosition.y * _cellSize.height));
            storedItem.SetOwnerInventory(this);
        }

        public virtual void RemoveStoredItem(ItemVisual storedItem)
        {
            if (_placedItemsGridData.TryGetValue(storedItem, out ItemGridData gridData))
            {
                OccupyGridCells(gridData, false);
                _placedItemsGridData.Remove(storedItem);

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
            if (!_placedItemsGridData.ContainsKey(visual))
                _placedItemsGridData.Add(visual, gridData);
        }

        public virtual void UnregisterVisual(ItemVisual visual)
        {
            if (_placedItemsGridData.ContainsKey(visual))
                _placedItemsGridData.Remove(visual);
        }
    }
}