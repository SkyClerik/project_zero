using SkyClerik.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using SkyClerik.CraftingSystem;

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public class CraftPageElement : IDropTarget
    {
        private UIDocument _document;
        private List<ItemVisual> _itemVisuals = new List<ItemVisual>();
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

        public CraftPageElement(ItemsPage itemsPage, UIDocument document, out VisualElement inventoryTwoPageRoot, ItemContainerBase itemContainer)
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
            var itemsInGrid = _itemVisuals.Select(v => v.ItemDefinition).ToList();
            
            if (craftSystem.TryFindRecipe(itemsInGrid, out var foundRecipe))
            {
                Debug.Log($"Найден рецепт! Результат: {foundRecipe.Result.Item.DefinitionName}");

                // 1. Уничтожаем визуальные элементы ингредиентов
                foreach (var visual in _itemVisuals)
                {
                    visual.RemoveFromHierarchy();
                }
                _itemVisuals.Clear();

                // 2. Очищаем контейнер данных от ингредиентов
                _itemContainer.Clear();

                // 3. Создаем результирующий предмет
                // TODO: Учесть количество из foundRecipe.Result.Quantity
                var resultItem = _itemContainer.AddItemAsClone(foundRecipe.Result.Item);
                resultItem.Stack = foundRecipe.Result.Quantity; // Учитываем количество из рецепта

                if (resultItem != null)
                {
                    // 4. Создаем и отображаем визуальный элемент для результата
                    ItemVisual resultVisual = new ItemVisual(
                        itemsPage: _itemsPage,
                        ownerInventory: this,
                        itemDefinition: resultItem,
                        rect: ConfigureSlotDimensions);
                    
                    resultVisual.UpdatePcs(); // Обновляем отображение количества
                    
                    AddItemToInventoryGrid(resultVisual);
                    
                    // Размещаем его в первой доступной ячейке
                    _coroutineRunner.StartCoroutine(GetPositionForItem(resultVisual, success =>
                    {
                        if (!success)
                        {
                            Debug.LogError("В сетке крафта нет места для результата! Хотя такого не может быть");
                            resultVisual.RemoveFromHierarchy();
                        }
                    }));
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
            foreach (var item in _itemContainer.GetItems())
            {
                ItemVisual inventoryItemVisual = new ItemVisual(
                    itemsPage: _itemsPage,
                    ownerInventory: this,
                    itemDefinition: item,
                    rect: ConfigureSlotDimensions);

                AddItemToInventoryGrid(inventoryItemVisual);

                bool inventoryHasSpace = false;
                yield return _coroutineRunner.StartCoroutine(GetPositionForItem(inventoryItemVisual, result => inventoryHasSpace = result));

                if (!inventoryHasSpace)
                {
                    //Debug.Log("Нет места - Невозможно поднять предмет");
                    inventoryItemVisual.RemoveFromHierarchy();
                    continue;
                }
            }
        }

        private Rect ConfigureSlotDimensions
        {
            get
            {
                VisualElement firstSlot = _inventoryGrid.Children().First();
                return firstSlot.worldBound;
            }
        }

        private void ConfigureInventoryDimensions()
        {
            var children = _inventoryGrid.Children().ToList();

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

        private IEnumerator GetPositionForItem(ItemVisual newItem, System.Action<bool> callback)
        {
            // Проходим по каждой ячейке сетки, чтобы найти свободное место.
            for (int y = 0; y < _inventoryDimensions.height; y++)
            {
                for (int x = 0; x < _inventoryDimensions.width; x++)
                {
                    // Устанавливаем позицию предмета в текущую проверяемую ячейку.
                    var newPos = new Vector2(_cellSize.width * x, _cellSize.height * y);
                    SetItemPosition(newItem, newPos);

                    // Ждем конца кадра, чтобы UI успел обновить worldBound предмета после смены позиции.
                    yield return new WaitForEndOfFrame();

                    // Проверяем, находится ли предмет полностью в границах сетки.
                    bool isInsideGrid = GridRectOverlap(newItem);
                    if (!isInsideGrid) continue; // Если не внутри, пропускаем эту ячейку

                    // Ищем, не пересекается ли предмет с другими, уже размещенными предметами.
                    bool overlapsAnotherItem = _itemVisuals.Any(itemInGrid => itemInGrid.worldBound.Overlaps(newItem.worldBound));

                    // Если предмет не пересекает другие предметы И находится в границах сетки...
                    if (!overlapsAnotherItem)
                    {
                        // ...то мы нашли подходящее место. Возвращаем true.
                        AddStoredItem(newItem); // Добавляем предмет в список, так как место для него найдено
                        callback(true);
                        yield break; // Выходим из корутины.
                    }
                }
            }

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
        private ItemVisual[] FindOverlappingItems(Rect rect)
        {
            var overlappingItems = new List<ItemVisual>();
            foreach (var itemInGrid in _itemVisuals)
            {
                // Используем layout, так как предметы в сетке статичны и их layout надежен
                bool overlap = itemInGrid.layout.Overlaps(rect);
                if (overlap)
                {
                    overlappingItems.Add(itemInGrid);
                }
            }
            return overlappingItems.ToArray();
        }

        //Логика определения конфликта и формирования результата
        private PlacementResults DeterminePlacementResult(ItemVisual[] overlappingItems, Vector2 position)
        {
            ReasonConflict conflict;
            ItemVisual overlapItem = null;

            if (overlappingItems.Length == 0)
            {
                conflict = ReasonConflict.None;
            }
            else if (overlappingItems.Length == 1)
            {
                conflict = ReasonConflict.SwapAvailable;
                overlapItem = overlappingItems[0];
            }
            else
            {
                conflict = ReasonConflict.intersectsObjects;
                overlapItem = overlappingItems[0]; // Можно оставить ссылку на первый пересеченный для информации
            }

            _telegraph.SetPlacement(conflict);
            return _placementResults.Init(conflict: conflict, position: position, overlapItem: overlapItem, targetInventory: this);
        }

        public PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            _overlapItem = null;

            // Проверяем, находится ли элемент в пределах сетки
            if (!IsWithinGridBounds(draggedItem))
            {
                _telegraph.Hide();
                return _placementResults.Init(conflict: ReasonConflict.beyondTheGridBoundary, overlapItem: _overlapItem, targetInventory: null);
            }

            // Находим целевой слот
            var targetSlot = FindTargetSlot(draggedItem);
            if (targetSlot == null)
            {
                _telegraph.Hide();
                return _placementResults.Init(conflict: ReasonConflict.beyondTheGridBoundary, overlapItem: _overlapItem, targetInventory: null);
            }

            // Обновляем позицию телеграфа
            UpdateTelegraphPosition(targetSlot, draggedItem);

            // Создаем Rect для проверки пересечений, основанный на компоновке, а не на worldBound
            var telegraphRect = new Rect(_telegraph.layout.position, new Vector2(_telegraph.layout.width, _telegraph.layout.height));

            // Ищем пересекающиеся объекты
            var overlappingItems = FindOverlappingItems(telegraphRect);

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

        public void AddStoredItem(ItemVisual storedItem)
        {
            _itemVisuals.Add(storedItem);
        }

        public void RemoveStoredItem(ItemVisual storedItem)
        {
            _itemVisuals.Remove(storedItem);
        }

        public void PickUp(ItemVisual storedItem)
        {
            RemoveStoredItem(storedItem);
            ItemsPage.CurrentDraggedItem = storedItem;
            storedItem.SetOwnerInventory(this);
        }

        public void Drop(ItemVisual storedItem, Vector2 position)
        {
            AddStoredItem(storedItem);
            AddItemToInventoryGrid(storedItem);
            storedItem.SetPosition(position - storedItem.parent.worldBound.position);

            storedItem.SetOwnerInventory(this);
        }

        public void FinalizeDrag()
        {
            _telegraph.Hide();
        }
    }
}