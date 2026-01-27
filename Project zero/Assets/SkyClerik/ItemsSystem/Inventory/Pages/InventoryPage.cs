using Gameplay.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;

namespace Gameplay.Inventory
{
    [System.Serializable]
    public class InventoryPage : IDropTarget
    {
        [SerializeField]
        private List<StoredItem> _storedItems = new List<StoredItem>();
        private ItemContainer _itemContainer;

        private UIDocument _document;
        private VisualElement _root;
        private const string _inventoryRootID = "inventory_root";

        private readonly MonoBehaviour _coroutineRunner;
        private VisualElement _inventoryGrid;
        private const string _gridID = "grid";
        private Telegraph _telegraph;

        private PlacementResults _placementResults;
        private StoredItem _overlapItem = null;

        private RectangleSize _inventoryDimensions;
        private Rect _cellSize;
        private Rect _gridRect;
        private Vector2 _mousePositionNormal;

        public UIDocument GetDocument => _document;
        public Telegraph Telegraph => _telegraph;

        public InventoryPage(UIDocument document, MonoBehaviour coroutineRunner, out VisualElement inventoryPageRoot, ItemContainer itemContainer)
        {
            _document = document;
            _coroutineRunner = coroutineRunner;
            _root = _document.rootVisualElement.Q<VisualElement>(_inventoryRootID);
            inventoryPageRoot = _root;
            _inventoryGrid = _root.Q<VisualElement>(_gridID);
            _itemContainer = itemContainer;
            _coroutineRunner.StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            yield return _coroutineRunner.StartCoroutine(Configure());
            yield return _coroutineRunner.StartCoroutine(LoadInventory());
        }

        private IEnumerator Configure()
        {
            ConfigureInventoryTelegraph();

            yield return new WaitForEndOfFrame();

            ConfigureInventoryDimensions();
            CalculateGridRect();
        }

        private IEnumerator LoadInventory()
        {
            foreach (ItemDefinition item in _itemContainer.GetItems())
            {
                var loadedItem = new StoredItem(item);
                ItemVisual inventoryItemVisual = new ItemVisual(
                    ownerInventory: this,
                    ownerStored: loadedItem,
                    rect: ConfigureSlotDimensions);

                AddItemToInventoryGrid(inventoryItemVisual);

                bool inventoryHasSpace = false;
                yield return _coroutineRunner.StartCoroutine(GetPositionForItem(inventoryItemVisual, loadedItem, result => inventoryHasSpace = result));

                if (!inventoryHasSpace)
                {
                    Debug.Log("No space - Cannot pick up the item");
                    //RemoveItemFromInventoryGrid(inventoryItemVisual);
                    continue;
                }

                ConfigureInventoryItem(loadedItem, inventoryItemVisual);
            }
        }

        public void Show()
        {
            _root.SetDisplay(true);
        }

        public void Hide()
        {
            _root.SetDisplay(false);
        }

        private void ConfigureInventoryTelegraph()
        {
            _telegraph = new Telegraph();
            _telegraph.name = "telegraph";
            AddItemToInventoryGrid(_telegraph);
            Debug.Log($"tel {_telegraph}");
        }

        private Rect ConfigureSlotDimensions
        {
            get
            {
                Debug.Log($"_inventoryGrid.Children().Count(): {_inventoryGrid.Children().Count()}");
                VisualElement firstSlot = _inventoryGrid.Children().First();
                Debug.Log($"firstSlot.worldBound: {firstSlot.worldBound}");
                return firstSlot.worldBound;
            }
        }

        private void ConfigureInventoryDimensions()
        {
            var children = _inventoryGrid.Children().ToList();
            Debug.Log($"ConfigureInventoryDimensions - children.Count: {children.Count}");

            _inventoryDimensions.width = 0;
            _inventoryDimensions.height = 1;

            if (children.Count > 0)
            {
                float tempY = children[0].worldBound.y;
                bool row = false;
                foreach (VisualElement box in children)
                {
                    if (box.worldBound.y > tempY)
                    {
                        row = true;
                        _inventoryDimensions.height++;
                        tempY = box.worldBound.y;
                    }

                    if (!row)
                        _inventoryDimensions.width++;
                }
            }
            Debug.Log($"_inventoryDimensions.width: {_inventoryDimensions.width}, _inventoryDimensions.height: {_inventoryDimensions.height}");
        }

        public void AddItemToInventoryGrid(VisualElement item)
        {
            Debug.Log($"add {item.name} to inventory grid");
            _inventoryGrid.Add(item);
        }

        private void RemoveItemFromInventoryGrid(VisualElement item)
        {
            Debug.Log($"Remove {item.name} to inventory grid");
            _inventoryGrid.Remove(item);
        }

        private static void ConfigureInventoryItem(StoredItem item, ItemVisual visual) => item.ItemVisual = visual;

        private static void SetItemPosition(VisualElement element, Vector2 vector)
        {
            element.style.left = vector.x;
            element.style.top = vector.y;
        }

        private IEnumerator GetPositionForItem(VisualElement newItem, StoredItem storedItem, System.Action<bool> callback)
        {
            // Проходим по каждой ячейке сетки, чтобы найти свободное место.
            for (int y = 0; y < _inventoryDimensions.height; y++)
            {
                for (int x = 0; x < _inventoryDimensions.width; x++)
                {
                    // Устанавливаем позицию предмета в текущую проверяемую ячейку.
                    var newPos = new Vector2(_cellSize.width * x, _cellSize.height * y);
                    Debug.Log($"newPos ({newPos.x},{newPos.y})");
                    SetItemPosition(newItem, newPos);
                    Debug.Log($"newPos1 ({newItem.style.left},{newItem.style.right})");
                    // Ждем конца кадра, чтобы UI успел обновить worldBound предмета после смены позиции.
                    yield return new WaitForEndOfFrame();

                    // Проверяем, находится ли предмет полностью в границах сетки.
                    bool parentOverlapping = GridRectOverlap(newItem);
                    // Ищем, не пересекается ли предмет с другими, уже размещенными предметами.
                    StoredItem overlappingItem = _storedItems.FirstOrDefault(s => s.ItemVisual != null && s.ItemVisual.worldBound.Overlaps(newItem.worldBound));

                    Debug.Log($"Checking slot ({x},{y}). Position: {newPos}. In grid bounds: {parentOverlapping}. Overlapping item: {(overlappingItem != null ? overlappingItem.ItemDefinition.name : "null")}");

                    // Если предмет не пересекает другие предметы И находится в границах сетки...
                    if (overlappingItem == null && parentOverlapping)
                    {
                        Debug.Log($"Found a valid spot at ({x},{y})");
                        // ...то мы нашли подходящее место. Возвращаем true.
                        callback(true);
                        yield break; // Выходим из корутины.
                    }
                }
            }

            Debug.LogWarning("Could not find any valid spot for the item.");
            // Если мы прошли весь цикл и не нашли места, возвращаем false.
            callback(false);
        }

        //Проверка пересечения с границами сетки
        private bool IsWithinGridBounds(ItemVisual draggedItem) => GridRectOverlap(draggedItem);

        //Поиск ближайшего слота, который пересекается с перемещаемым элементом
        private VisualElement FindTargetSlot(ItemVisual draggedItem)
        {
            return _inventoryGrid.Children()
                .Where(x => x.worldBound.Overlaps(draggedItem.worldBound) && x != draggedItem)
                .OrderBy(x => Vector2.Distance(x.worldBound.position, draggedItem.worldBound.position))
                .FirstOrDefault();
        }

        //Логика обновления размера и позиции телеграфа
        private void UpdateTelegraphPosition(VisualElement targetSlot, ItemVisual draggedItem)
        {
            _telegraph.style.width = draggedItem.style.width;
            _telegraph.style.height = draggedItem.style.height;

            SetItemPosition(_telegraph, new Vector2(targetSlot.layout.position.x, targetSlot.layout.position.y));
        }

        //Логика поиска пересекающихся объектов
        private StoredItem[] FindOverlappingItems()
        {
            return _storedItems
                .Where(x => x.ItemVisual != null && x.ItemVisual.worldBound.Overlaps(_telegraph.worldBound))
                .ToArray();
        }

        //Логика определения конфликта и формирования результата
        private PlacementResults DeterminePlacementResult(StoredItem[] overlappingItems, Vector2 position)
        {
            if (overlappingItems.Length == 1)
            {
                _overlapItem = overlappingItems[0];
                _telegraph.SetPlacement(true);
                return _placementResults.Init(conflict: ReasonConflict.None, position: position, overlapItem: _overlapItem);
            }
            else if (overlappingItems.Length > 1)
            {
                _telegraph.SetPlacement(false);
                return _placementResults.Init(conflict: ReasonConflict.intersectsObjects, overlapItem: _overlapItem);
            }

            _telegraph.SetPlacement(true);
            return _placementResults.Init(conflict: ReasonConflict.None, position: position, overlapItem: _overlapItem);
        }

        public PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            _overlapItem = null;

            // Проверяем, находится ли элемент в пределах сетки
            if (!IsWithinGridBounds(draggedItem))
            {
                _telegraph.Hide();
                return _placementResults.Init(conflict: ReasonConflict.beyondTheGridBoundary, overlapItem: _overlapItem);
            }

            // Находим целевой слот
            var targetSlot = FindTargetSlot(draggedItem);
            if (targetSlot == null)
            {
                _telegraph.Hide();
                return _placementResults.Init(conflict: ReasonConflict.beyondTheGridBoundary, overlapItem: _overlapItem);
            }

            // Обновляем позицию телеграфа
            UpdateTelegraphPosition(targetSlot, draggedItem);

            // Ищем пересекающиеся объекты
            var overlappingItems = FindOverlappingItems();

            // Определяем результат размещения
            return DeterminePlacementResult(overlappingItems, targetSlot.worldBound.position);
        }

        private bool GridRectOverlap(VisualElement item)
        {
            if (item.worldBound.xMin >= _gridRect.xMin && item.worldBound.yMin >= _gridRect.yMin && item.worldBound.xMax <= _gridRect.xMax && item.worldBound.yMax <= _gridRect.yMax)
                return true;

            return false;
        }

        private void CalculateGridRect()
        {
            _cellSize = ConfigureSlotDimensions;
            _gridRect = _inventoryGrid.worldBound;
            _gridRect.width = (_cellSize.width * _inventoryDimensions.width) + (_cellSize.width / 2);
            _gridRect.height = (_cellSize.height * _inventoryDimensions.height) + (_cellSize.height / 2);
            _gridRect.x -= (_cellSize.width / 4);
            _gridRect.y -= (_cellSize.height / 4);
        }

        public void AddStoredItem(StoredItem storedItem)
        {
            _storedItems.Add(storedItem);
        }

        public void RemoveStoredItem(StoredItem storedItem)
        {
            _storedItems.Remove(storedItem);
        }

        public void PickUp(StoredItem storedItem)
        {
            Debug.Log($"Inventory PickUp Item");
            RemoveStoredItem(storedItem);
            CharacterPages.CurrentDraggedItem.Owner = this;
            storedItem.ItemVisual.SetOwnerInventory(this);
        }

        public void Drop(StoredItem storedItem, Vector2 position)
        {
            AddStoredItem(storedItem);
            storedItem.ItemVisual.SetPosition(position - storedItem.ItemVisual.parent.worldBound.position);

            AddItemToInventoryGrid(storedItem.ItemVisual);
            storedItem.ItemVisual.SetOwnerInventory(this);
        }

        public void FinalizeDrag()
        {
            _telegraph.Hide();
        }
    }
}