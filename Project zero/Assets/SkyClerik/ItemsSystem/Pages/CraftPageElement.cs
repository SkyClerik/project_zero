using SkyClerik.Data;
using UnityEngine.DataEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using SkyClerik.CraftingSystem; // Добавляем эту строку

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public class CraftPageElement : IDropTarget
    {
        private UIDocument _document;
        private Dictionary<ItemVisual, ItemGridData> _placedItemsGridData = new Dictionary<ItemVisual, ItemGridData>();
        private bool[,] _gridOccupancy;
        private Dictionary<ItemVisual, ItemGridData> _visualToGridDataMap = new Dictionary<ItemVisual, ItemGridData>();
        private ItemContainerBase _itemContainer;
        private MonoBehaviour _coroutineRunner;
        private ItemsPage _itemsPage;
        private Telegraph _telegraph;
        private PlacementResults _placementResults;
        private ItemVisual _overlapItem = null;
        private RectangleSize _inventoryDimensions;
        private Rect _cellSize;
        private Rect _gridRect;
        private const string _craftPageTitleText = "Окно крафта предметов";
        private VisualElement _root;
        private const string _craftRootID = "craft_root";
        private VisualElement _header;
        private const string _headerID = "header";
        private Label _title;
        private const string _titleID = "l_title";
        private VisualElement _body;
        private const string _bodyID = "body";
        private VisualElement _inventoryGrid;
        private const string _gridID = "grid";
        private Button _craftButton;
        private const string _craftButtonID = "b_craft";

        public UIDocument GetDocument => _document;
        public Telegraph Telegraph => _telegraph;
        public ItemContainerBase ItemContainer => _itemContainer;

        public CraftPageElement(ItemsPage itemsPage, UIDocument document, out VisualElement inventoryTwoPageRoot, ItemContainerBase itemContainer, Vector2 cellSize, Vector2Int inventoryGridSize)
        {
            _itemsPage = itemsPage;
            _document = document;
            _coroutineRunner = itemsPage;
            _itemContainer = itemContainer;

            _root = _document.rootVisualElement.Q<VisualElement>(_craftRootID);
            //header
            _header = _root.Q(_headerID);
            _title = _header.Q<Label>(_titleID);
            //body
            _body = _root.Q(_bodyID);
            _inventoryGrid = _body.Q(_gridID);
            _craftButton = _body.Q<Button>(_craftButtonID);

            _title.text = _craftPageTitleText;
            _craftButton.clicked += _craftButton_clicked;
            inventoryTwoPageRoot = _root;
            _cellSize = new Rect(0, 0, cellSize.x, cellSize.y); // Инициализируем _cellSize переданным значением
            _inventoryDimensions.width = inventoryGridSize.x;
            _inventoryDimensions.height = inventoryGridSize.y;
            _gridOccupancy = new bool[_inventoryDimensions.width, _inventoryDimensions.height]; // Инициализируем массив занятости сетки
            _coroutineRunner.StartCoroutine(Initialize());
        }

        private void _craftButton_clicked()
        {
            var craftSystem = ServiceProvider.Get<ICraftingSystem>();
            if (craftSystem == null)
            {
                Debug.LogError("Система крафта не найдена!");
                return;
            }

            // Собираем все предметы, которые сейчас лежат в сетке крафта
            var itemsInGridDefinitions = _placedItemsGridData.Keys.Select(visual => visual.ItemDefinition).ToList(); // Используем ключи словаря, это ItemBaseDefinition
            
            if (craftSystem.TryFindRecipe(itemsInGridDefinitions, out var foundRecipe))
            {
                Debug.Log($"Найден рецепт! Результат: {foundRecipe.Result.Item.DefinitionName}");

                // 1. Уничтожаем визуальные элементы ингредиентов и освобождаем ячейки
                foreach (var entry in _visualToGridDataMap.ToList()) // Копируем список, чтобы избежать изменения коллекции во время итерации
                {
                    UnregisterVisual(entry.Key); // Отменяем регистрацию
                    OccupyGridCells(entry.Value, false); // Освобождаем ячейки
                    entry.Key.RemoveFromHierarchy(); // Удаляем из UI
                }
                _placedItemsGridData.Clear(); // Очищаем данные о размещенных предметах
                _itemContainer.Clear(); // Очищаем контейнер данных от ингредиентов

                // 2. Создаем результирующий предмет
                var resultItem = _itemContainer.AddItemAsClone(foundRecipe.Result.Item);
                resultItem.Stack = foundRecipe.Result.Quantity; // Учитываем количество из рецепта

                if (resultItem != null)
                {
                    // 3. Пытаемся разместить результат в сетке
                    if (TryFindPlacement(resultItem, out Vector2Int gridPosition))
                    {
                        ItemGridData newGridData = new ItemGridData(resultItem, gridPosition);
                        ItemVisual resultVisual = new ItemVisual(
                            itemsPage: _itemsPage,
                            ownerInventory: this,
                            itemDefinition: resultItem,
                            gridPosition: gridPosition,
                            gridSize: newGridData.GridSize);

                        _placedItemsGridData.Add(resultVisual, newGridData);
                        
                        resultVisual.UpdatePcs(); // Обновляем отображение количества
                        
                        AddItemToInventoryGrid(resultVisual); // Добавляем в UI
                        RegisterVisual(resultVisual, newGridData); // Регистрируем визуальный элемент
                        resultVisual.SetPosition(new Vector2(gridPosition.x * _cellSize.width, gridPosition.y * _cellSize.height)); // Устанавливаем визуальную позицию
                    }
                    else
                    {
                        Debug.LogError("В сетке крафта нет места для результата! Хотя такого не может быть при правильной логике рецептов.");
                        // Здесь можно обработать ситуацию, когда результат не поместился (например, вернуть ингредиенты)
                    }
                }
            }
            else
            {
                Debug.Log("Такого рецепта не существует.");
            }
        }

        private IEnumerator Initialize()
        {
            yield return _coroutineRunner.StartCoroutine(Configure());
            yield return _coroutineRunner.StartCoroutine(LoadInventory());
        }

        private IEnumerator Configure()
        {
            _telegraph = new Telegraph();
            AddItemToInventoryGrid(_telegraph);

            yield return new WaitForEndOfFrame();

            ConfigureInventoryDimensions();
            CalculateGridRect();
        }

        private IEnumerator LoadInventory()
        {
            if (!_itemContainer.GetItems().Any()) // Проверяем, есть ли вообще предметы
            {
                yield break; // Если нет, сразу завершаем корутину
            }

            foreach (var itemInContainer in _itemContainer.GetItems().ToList()) // Создаем копию списка
            {
                if (TryFindPlacement(itemInContainer, out Vector2Int gridPosition))
                {
                    ItemGridData newGridData = new ItemGridData(itemInContainer, gridPosition);

                    ItemVisual inventoryItemVisual = new ItemVisual(
                        itemsPage: _itemsPage,
                        ownerInventory: this,
                        itemDefinition: itemInContainer, // Используем уже существующий клон
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
                    Debug.LogWarning($"[CraftPageElement.LoadInventory] Не удалось разместить предмет {itemInContainer.name} в инвентаре. Возможно, нет места.");
                    _itemContainer.RemoveItem(itemInContainer);
                }
            }
            yield break; // LoadInventory больше не использует корутины для позиционирования
        }



        private void ConfigureInventoryDimensions()
        {
            // Размеры инвентаря (_inventoryDimensions) и массив занятости сетки (_gridOccupancy)
            // теперь инициализируются в конструкторе.
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            _inventoryGrid.Add(item);
        }

        private void RemoveItemFromInventoryGrid(VisualElement item)
        {
            _inventoryGrid.Remove(item);
        }

        private static void SetItemPosition(VisualElement element, Vector2 vector)
        {
            element.style.left = vector.x;
            element.style.top = vector.y;
        }

        private void OccupyGridCells(ItemGridData gridData, bool occupy)
        {
            for (int y = 0; y < gridData.GridSize.y; y++)
            {
                for (int x = 0; x < gridData.GridSize.x; x++)
                {
                    _gridOccupancy[gridData.GridPosition.x + x, gridData.GridPosition.y + y] = occupy;
                }
            }
        }

        private bool IsGridAreaFree(Vector2Int start, Vector2Int size)
        {
            Debug.Log($"[IsGridAreaFree] Start: {start}, Size: {size}, InventoryDimensions: {_inventoryDimensions.width}x{_inventoryDimensions.height}");
            Debug.Log($"[IsGridAreaFree] GridOccupancy Dimensions: {_gridOccupancy.GetLength(0)}x{_gridOccupancy.GetLength(1)}");

            // Проверка на выход за границы инвентаря
            if (start.x < 0 || start.y < 0 || start.x + size.x > _inventoryDimensions.width || start.y + size.y > _inventoryDimensions.height)
            {
                Debug.LogWarning($"[IsGridAreaFree] Out of bounds check failed: Start={start}, Size={size}, Bounds={_inventoryDimensions.width}x{_inventoryDimensions.height}");
                return false;
            }

            // Проверка на занятость ячеек
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int currentX = start.x + x;
                    int currentY = start.y + y;
                    Debug.Log($"[IsGridAreaFree] Checking cell: ({currentX}, {currentY})");
                    if (currentX >= _gridOccupancy.GetLength(0) || currentY >= _gridOccupancy.GetLength(1) || currentX < 0 || currentY < 0)
                    {
                         Debug.LogError($"[IsGridAreaFree] Accessing out of bounds during check: currentX={currentX}, currentY={currentY}, GridOccupancy Size={_gridOccupancy.GetLength(0)}x{_gridOccupancy.GetLength(1)}");
                         return false;
                    }
                    if (_gridOccupancy[currentX, currentY]) // Исправлено: поменяли x и y местами
                    {
                        Debug.Log($"[IsGridAreaFree] Cell ({currentX}, {currentY}) is OCCUPIED.");
                        return false; // Ячейка занята
                    }
                }
            }
            Debug.Log($"[IsGridAreaFree] Area from {start} with size {size} is FREE.");
            return true; // Вся область свободна
        }




        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
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











        public PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            Vector2Int itemGridSize = new Vector2Int(draggedItem.ItemDefinition.Dimensions.CurrentWidth, draggedItem.ItemDefinition.Dimensions.CurrentHeight);

            Debug.Log($"[CraftPageElement.ShowPlacementTarget] Start. CurrentHoverGridPosition: {currentHoverGridPosition}, ItemGridSize: {itemGridSize}");

            _placementResults = new PlacementResults();
            _placementResults.Conflict = ReasonConflict.None;
            _placementResults.OverlapItem = null;
            // Сначала проверяем, свободно ли место под курсором
            if (IsGridAreaFree(currentHoverGridPosition, itemGridSize))
            {
                _placementResults.Conflict = ReasonConflict.None; // Место под курсором свободно
                _placementResults.SuggestedGridPosition = currentHoverGridPosition; // <--- Здесь все хорошо
                Debug.Log($"[CraftPageElement.ShowPlacementTarget] Place under cursor is FREE. SuggestedGridPosition: {_placementResults.SuggestedGridPosition}");
            }
            else // Место под курсором не свободно, ищем другие варианты (Swap или другой конфликт)
            {
                List<ItemVisual> overlappingItems = FindOverlappingItems(currentHoverGridPosition, itemGridSize, draggedItem);
                Debug.Log($"[CraftPageElement.ShowPlacementTarget] Overlapping items count: {overlappingItems.Count}");

                if (overlappingItems.Count == 1)
                {
                    // Если пересекается только с одним предметом, это потенциальный Swap
                    ItemVisual overlapItem = overlappingItems[0];
                    ItemGridData potentialOverlapGridData = _visualToGridDataMap[overlapItem];

                    _placementResults.Conflict = ReasonConflict.SwapAvailable;
                    _placementResults.OverlapItem = overlapItem;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition; // ИЗМЕНЕНО: теперь используется currentHoverGridPosition
                    Debug.Log($"[CraftPageElement.ShowPlacementTarget] SwapAvailable detected. OverlapItem: {_placementResults.OverlapItem.name}, SuggestedGridPosition: {_placementResults.SuggestedGridPosition}");
                }
                else if (overlappingItems.Count > 1)
                {
                    // Если пересекается более чем с одним предметом, это конфликт
                    _placementResults.Conflict = ReasonConflict.intersectsObjects;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                    Debug.Log($"[CraftPageElement.ShowPlacementTarget] Intersects with multiple items. Conflict: {_placementResults.Conflict}");
                }
                else
                {
                    // Если overlappingItems.Count == 0, но IsGridAreaFree вернул false,
                    // это означает, что мы либо за границами, либо пересекаемся с пустыми, но "заблокированными" ячейками
                    if (currentHoverGridPosition.x < 0 || currentHoverGridPosition.y < 0 ||
                        currentHoverGridPosition.x + itemGridSize.x > _inventoryDimensions.width ||
                        currentHoverGridPosition.y + itemGridSize.y > _inventoryDimensions.height)
                    {
                        _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
                        Debug.Log($"[CraftPageElement.ShowPlacementTarget] Beyond the grid boundary. Conflict: {_placementResults.Conflict}");
                    }
                    else
                    {
                        _placementResults.Conflict = ReasonConflict.intersectsObjects; // Внутри сетки, но занято
                        _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                        Debug.Log($"[CraftPageElement.ShowPlacementTarget] Inside grid but occupied (no overlapping items found). Conflict: {_placementResults.Conflict}");
                    }
                }
            }

            // ...

            // Если место найдено или это потенциальный Swap, обновляем телеграф
            if (_placementResults.Conflict == ReasonConflict.None || _placementResults.Conflict == ReasonConflict.SwapAvailable || _placementResults.Conflict == ReasonConflict.intersectsObjects) // Добавляем intersectsObjects
            {
                _telegraph.style.left = _placementResults.SuggestedGridPosition.x * _cellSize.width;
                _telegraph.style.top = _placementResults.SuggestedGridPosition.y * _cellSize.height;
                _telegraph.style.width = itemGridSize.x * _cellSize.width;
                _telegraph.style.height = itemGridSize.y * _cellSize.height;
                _telegraph.style.display = DisplayStyle.Flex; // ИЗМЕНЕНО: теперь используем style.display
            }
            else // Если beyondTheGridBoundary или какой-то другой, то скрываем
            {
                _telegraph.Hide();
            }

            Debug.Log($"[CraftPageElement.ShowPlacementTarget] End. Conflict: {_placementResults.Conflict}, SuggestedGridPosition: {_placementResults.SuggestedGridPosition}, OverlapItem: {(_placementResults.OverlapItem != null ? _placementResults.OverlapItem.name : "None")}");
            Debug.Log($"[CraftPageElement.ShowPlacementTarget] Returning Init with Conflict: {_placementResults.Conflict}, SuggestedGridPosition: {_placementResults.SuggestedGridPosition}, OverlapItem: {(_placementResults.OverlapItem != null ? _placementResults.OverlapItem.name : "None")}, TargetInventory: {this.GetType().Name}");
            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x * _cellSize.width, _placementResults.SuggestedGridPosition.y * _cellSize.height),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }



        private void CalculateGridRect()
        {
            _gridRect = _inventoryGrid.worldBound; // Пока оставляем worldBound для _gridRect, если нет другого способа определить общий размер сетки.
            _gridRect.width = (_cellSize.width * _inventoryDimensions.width) + (_cellSize.width / 2);
            _gridRect.height = (_cellSize.height * _inventoryDimensions.height) + (_cellSize.height / 2);
            _gridRect.x -= (_cellSize.width / 4);
            _gridRect.y -= (_cellSize.height / 4);
        }

        private Vector2Int CalculateCurrentHoverGridPosition()
        {
            // Получаем позицию мыши в экранных координатах
            Vector2 mouseScreenPosition = Input.mousePosition;
            // Преобразуем ее в локальные координаты _inventoryGrid
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(mouseScreenPosition);
            
            // Вычисляем позицию в сетке
            int gridX = Mathf.FloorToInt(mouseLocalPosition.x / _cellSize.width);
            int gridY = Mathf.FloorToInt((_inventoryGrid.resolvedStyle.height - mouseLocalPosition.y) / _cellSize.height); // Инвертируем Y-координату
            
            Vector2Int currentHoverGridPosition = new Vector2Int(gridX, gridY);
            Debug.Log($"[CraftPageElement.CalculateCurrentHoverGridPosition] MouseScreenPosition: {mouseScreenPosition}, MouseLocalPosition: {mouseLocalPosition}, ResolvedHeight: {_inventoryGrid.resolvedStyle.height}, CellSize: {_cellSize.width}x{_cellSize.height}, CurrentHoverGridPosition: {currentHoverGridPosition}");
            
            return currentHoverGridPosition;
        }


        public void AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)
        {
            ItemGridData gridData;
            if (_placedItemsGridData.TryGetValue(storedItem, out var existingGridData)) // Используем storedItem
            {
                gridData = existingGridData;
                OccupyGridCells(gridData, false); // Сначала освобождаем старое место
                gridData.GridPosition = gridPosition; // Обновляем позицию
            }
            else
            {
                gridData = new ItemGridData(storedItem.ItemDefinition, gridPosition);
                _placedItemsGridData.Add(storedItem, gridData); // Используем storedItem
            }

            OccupyGridCells(gridData, true); // Помечаем ячейки как занятые
            RegisterVisual(storedItem, gridData); // Регистрируем визуальный элемент
            AddItemToInventoryGrid(storedItem); // Добавляем в UI
            
            storedItem.SetPosition(new Vector2(gridPosition.x * _cellSize.width, gridPosition.y * _cellSize.height));
            storedItem.SetOwnerInventory(this);
        }

        public void RemoveStoredItem(ItemVisual storedItem)
        {
            if (_placedItemsGridData.TryGetValue(storedItem, out ItemGridData gridData)) // Используем storedItem
            {
                OccupyGridCells(gridData, false); // Освобождаем ячейки
                _placedItemsGridData.Remove(storedItem); // Используем storedItem
                UnregisterVisual(storedItem); // Отменяем регистрацию визуального элемента
                storedItem.RemoveFromHierarchy(); // Удаляем из UI
            }
            else
            {
                Debug.LogWarning($"[CraftPageElement] Attempted to remove an item that was not in _placedItemsGridData: {storedItem.name}");
            }
        }

        public void PickUp(ItemVisual storedItem)
        {
            // Получаем ItemGridData, чтобы освободить ячейки
            if (_placedItemsGridData.TryGetValue(storedItem, out ItemGridData gridData)) // Используем storedItem
            {
                OccupyGridCells(gridData, false); // Освобождаем ячейки
                _placedItemsGridData.Remove(storedItem); // Используем storedItem
            }
            UnregisterVisual(storedItem); // Отменяем регистрацию визуального элемента

            ItemsPage.CurrentDraggedItem = storedItem;
            storedItem.SetOwnerInventory(this);
        }

        public void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            AddStoredItem(storedItem, gridPosition);
        }

        public void FinalizeDrag()
        {
            _telegraph.Hide();
        }

        // Explicit implementation for IDropTarget.AddStoredItem
        void IDropTarget.AddStoredItem(ItemVisual storedItem)
        {
            // Здесь должна быть логика по размещению предмета без явной позиции.
            // Возможно, поиск свободного места или обработка ошибок.
            // Сейчас оставляем пустым, чтобы компилятор не ругался, и обсудим с тобой, как лучше реализовать.
            Debug.LogWarning($"[CraftPageElement] IDropTarget.AddStoredItem(ItemVisual) called. " +
                             $"This method currently does not place the item in a specific grid position. " +
                             $"Consider calling AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition) instead.");
        }

        public ItemGridData GetItemGridData(ItemVisual itemVisual) // Изменена сигнатура
        {
            if (_placedItemsGridData.TryGetValue(itemVisual, out ItemGridData gridData)) // Используем itemVisual
            {
                return gridData;
            }
            return null;
        }

        public Vector2 CellSize => new Vector2(_cellSize.width, _cellSize.height);

        private ItemVisual FindItemAtGridPosition(Vector2Int gridPosition)
        {
            foreach (var entry in _visualToGridDataMap)
            {
                ItemGridData gridData = entry.Value;
                Vector2Int itemGridSize = gridData.GridSize;
                Vector2Int itemGridPos = gridData.GridPosition;

                // Проверяем, находится ли gridPosition внутри области, занимаемой предметом
                if (gridPosition.x >= itemGridPos.x && gridPosition.x < itemGridPos.x + itemGridSize.x &&
                    gridPosition.y >= itemGridPos.y && gridPosition.y < itemGridPos.y + itemGridSize.y)
                {
                    return entry.Key; // Возвращаем ItemVisual, который занимает эту ячейку
                }
            }
            return null; // Ничего не найдено
        }

        public void RegisterVisual(ItemVisual visual, ItemGridData gridData)
        {
            if (!_visualToGridDataMap.ContainsKey(visual))
            {
                _visualToGridDataMap.Add(visual, gridData);
            }
        }

        public void UnregisterVisual(ItemVisual visual)
        {
            if (_visualToGridDataMap.ContainsKey(visual))
            {
                _visualToGridDataMap.Remove(visual);
            }
        }

        private List<ItemVisual> FindOverlappingItems(Vector2Int start, Vector2Int size, ItemVisual draggedItem)
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
    }
}