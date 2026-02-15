using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Базовый класс для элементов страницы, которые представляют собой сетку инвентаря.
    /// Предоставляет основную функциональность для управления визуальным отображением предметов в сетке,
    /// обработки перетаскивания и взаимодействия с логическим контейнером предметов.
    /// </summary>
    [System.Serializable]
    public abstract class GridPageElementBase : IDropTarget, IDisposable, IItemContainerViewCallbacks
    {
        /// <summary>
        /// Словарь, хранящий визуальные элементы предметов и их логические данные в сетке.
        /// </summary>
        protected Dictionary<ItemVisual, ItemGridData> _visuals = new Dictionary<ItemVisual, ItemGridData>();
        /// <summary>
        /// Прямоугольник, представляющий границы сетки.
        /// </summary>
        protected Rect _gridRect;

        // Зависимости
        /// <summary>
        /// Ссылка на UIDocument, к которому принадлежит страница.
        /// </summary>
        protected UIDocument _document;
        /// <summary>
        /// Объект, используемый для запуска корутин.
        /// </summary>
        protected MonoBehaviour _coroutineRunner;
        /// <summary>
        /// Ссылка на основной контейнер инвентаря.
        /// </summary>
        protected InventoryStorage _itemsPage;
        /// <summary>
        /// Ссылка на логический контейнер предметов.
        /// </summary>
        protected ItemContainer _itemContainer;

        // UI-элементы
        /// <summary>
        /// Корневой визуальный элемент страницы.
        /// </summary>
        protected VisualElement _root;
        /// <summary>
        /// Визуальный элемент сетки инвентаря.
        /// </summary>
        private VisualElement _inventoryGrid;

        /// <summary>
        /// Возвращает визуальный элемент сетки инвентаря.
        /// </summary>
        public VisualElement InventoryGridElement => _inventoryGrid;

        [SerializeField]
        private string _inventoryGridID = "grid";

        /// <summary>
        /// Результаты размещения предмета в сетке.
        /// </summary>
        protected PlacementResults _placementResults;

        public UIDocument GetDocument => _document;
        public ItemContainer ItemContainer => _itemContainer;
        public Vector2 CellSize => _itemContainer.CellSize;
        public VisualElement Root => _root;

        /// <summary>
        /// Флаг, указывающий, нужно ли подавлять создание следующего визуального элемента.
        /// Используется для предотвращения дублирования визуала при перемещении предмета.
        /// </summary>
        public bool SuppressNextVisualCreation { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр базового класса GridPageElementBase.
        /// </summary>
        /// <param name="itemsPage">Главный контроллер инвентаря.</param>
        /// <param name="document">UIDocument, к которому принадлежит страница.</param>
        /// <param name="itemContainer">Логический контейнер предметов, связанный с этой страницей.</param>
        /// <param name="rootID">ID корневого визуального элемента страницы в UIDocument.</param>
        protected GridPageElementBase(InventoryStorage itemsPage, UIDocument document, ItemContainer itemContainer, string rootID)
        {
            _itemsPage = itemsPage;
            _document = document;
            _coroutineRunner = itemsPage;
            _itemContainer = itemContainer;

            _root = _document.rootVisualElement.Q<VisualElement>(rootID);
            _inventoryGrid = _root.Q<VisualElement>(_inventoryGridID);

            _coroutineRunner.StartCoroutine(Initialize());
        }

        /// <summary>
        /// Инициализирует страницу, настраивает UI, подписывается на события контейнера
        /// и загружает начальные визуальные элементы предметов.
        /// </summary>
        protected IEnumerator Initialize()
        {
            yield return new WaitForEndOfFrame();
            SubscribeToContainerEvents();
            LoadInitialVisuals();
        }
        /// <summary>
        /// Подписывает страницу на колбэки от логического контейнера предметов.
        /// </summary>
        protected virtual void SubscribeToContainerEvents()
        {
            if (_itemContainer == null) return;
            _itemContainer.SetViewCallbacks(this);
        }

        /// <summary>
        /// Отписывает страницу от колбэков логического контейнера предметов.
        /// </summary>
        protected virtual void UnsubscribeFromContainerEvents()
        {
            if (_itemContainer == null) return;
            _itemContainer.SetViewCallbacks(null);
        }

        /// <summary>
        /// Колбэк, вызываемый при добавлении предмета в логический контейнер.
        /// Обновляет существующий визуальный элемент или создает новый, если предмет не является перетаскиваемым.
        /// </summary>
        /// <param name="item">Добавленный предмет.</param>
        public void OnItemAddedCallback(ItemBaseDefinition item)
        {
            //Debug.Log($"[GridPageElementBase:{_root.name}] HandleItemAdded вызван для '{item.name}'. SuppressNextVisualCreation: {SuppressNextVisualCreation}");

            if (SuppressNextVisualCreation)
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] SuppressNextVisualCreation установлен. Пропускаем создание/обновление visual для '{item.name}'.");
                SuppressNextVisualCreation = false;
                return;
            }

            // NEW CHECK: If the added item is the one currently being dragged,
            // we do NOT create a new visual, because the existing dragged visual
            // will be adopted by AdoptExistingVisual later.
            if (InventoryStorage.CurrentDraggedItem != null && InventoryStorage.CurrentDraggedItem.ItemDefinition == item)
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] OnItemAddedCallback: Item '{item.name}' is the CurrentDraggedItem. Skipping visual creation.");
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

        /// <summary>
        /// Колбэк, вызываемый при удалении предмета из логического контейнера.
        /// Удаляет соответствующий визуальный элемент из сетки.
        /// </summary>
        /// <param name="item">Удаленный предмет.</param>
        public void OnItemRemovedCallback(ItemBaseDefinition item)
        {
            var visualToRemove = _visuals.Keys.FirstOrDefault(visual => GetItemDefinition(visual) == item);
            if (visualToRemove != null)
            {
                //Debug.Log($"[GridPageElementBase] HandleItemRemoved: Найден и удаляется visual для '{item.name}'. HashCode: {visualToRemove.GetHashCode()}");
                UnregisterVisual(visualToRemove);
                visualToRemove.RemoveFromHierarchy();
            }
        }

        /// <summary>
        /// Колбэк, вызываемый при полной очистке логического контейнера.
        /// Удаляет все визуальные элементы из сетки.
        /// </summary>
        public void OnClearedCallback()
        {
            foreach (var visual in _visuals.Keys.ToList())
            {
                visual.RemoveFromHierarchy();
            }
            _visuals.Clear();
        }

        /// <summary>
        /// Колбэк, вызываемый при изменении занятости ячеек сетки.
        /// Здесь можно реализовать логику обновления UI, если это необходимо.
        /// </summary>
        public void OnGridOccupancyChangedCallback()
        {

        }

        /// <summary>
        /// Колбэк, вызываемый при изменении стака у предмета.
        /// </summary>
        public void OnItemStackChangedCallback(ItemBaseDefinition item)
        {
            var visual = GetItemVisual(item);
            if (visual != null)
            {
                visual.UpdatePcs();
            }
        }

        /// <summary>
        /// Колбэк, вызываемый при перемещении предмета в контейнере.
        /// Обновляет позицию существующего визуального элемента предмета.
        /// </summary>
        /// <param name="item">Перемещенный предмет.</param>
        /// <param name="oldPosition">Старая позиция предмета в сетке.</param>
        public void OnItemMovedCallback(ItemBaseDefinition item, Vector2Int oldPosition)
        {
            var existingVisual = _visuals.Keys.FirstOrDefault(visual => GetItemDefinition(visual) == item);
            if (existingVisual != null)
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] OnItemMovedCallback: Перемещенный visual для '{item.name}'. Обновляем позицию.");
                existingVisual.SetPosition(new Vector2(item.GridPosition.x * CellSize.x, item.GridPosition.y * CellSize.y));
                _visuals[existingVisual] = new ItemGridData(item, item.GridPosition);
            }
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

        /// <summary>
        /// Загружает начальные визуальные элементы предметов из контейнера.
        /// </summary>
        private void LoadInitialVisuals()
        {
            foreach (var item in _itemContainer.GetItems())
            {
                CreateVisualForItem(item);
            }
        }
        /// <summary>
        /// Создает новый визуальный элемент для заданного предмета и добавляет его в сетку.
        /// </summary>
        /// <param name="item">Определение предмета, для которого создается визуал.</param>
        private void CreateVisualForItem(ItemBaseDefinition item)
        {
            //Debug.Log($"[GridPageElementBase:{_root.name}] CreateVisualForItem: Создание НОВОГО ItemVisual для '{item.name}' с данными: Angle={item.Dimensions.Angle}, Size=({item.Dimensions.Width},{item.Dimensions.Height}), Pos={item.GridPosition}");
            var newGridData = new ItemGridData(item, item.GridPosition);
            var newItemVisual = new ItemVisual(
                inventoryStorage: _itemsPage,
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
        /// Принимает существующий визуальный элемент предмета и адаптирует его к текущему контейнеру.
        /// Используется при перемещении предметов между контейнерами, чтобы избежать пересоздания визуала.
        /// </summary>
        /// <param name="visual">Существующий визуальный элемент предмета.</param>
        public void AdoptExistingVisual(ItemVisual visual)
        {
            var itemDef = visual.ItemDefinition;
            var newGridData = new ItemGridData(itemDef, itemDef.GridPosition);

            visual.RemoveFromHierarchy();

            AddItemToInventoryGrid(visual);
            RegisterVisual(visual, newGridData);
            visual.SetOwnerInventory(this);
            visual.SetPosition(new Vector2(itemDef.GridPosition.x * CellSize.x, itemDef.GridPosition.y * CellSize.y));
            visual.UpdatePcs();
        }

        /// <summary>
        /// Добавляет визуальный элемент предмета в сетку инвентаря.
        /// </summary>
        /// <param name="item">Визуальный элемент, который нужно добавить.</param>
        public void AddItemToInventoryGrid(VisualElement item)
        {
            //Debug.Log($"[GridPageElementBase:{Root.name}] AddItemToInventoryGrid: Добавляю visual '{item.name}' _inventoryGrid.name : {_inventoryGrid.name}.");
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
            //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Начало проверки для предмета '{draggedItem.ItemDefinition.name}'. Root enabledSelf: {_root.enabledSelf}, Display: {_root.resolvedStyle.display}, Visibility: {_root.resolvedStyle.visibility}. InventoryGrid name: {_inventoryGrid.name}, worldBound pos: {_inventoryGrid.worldBound.position}");

            if (!_root.enabledSelf || _root.resolvedStyle.display == DisplayStyle.None || _root.resolvedStyle.visibility == Visibility.Hidden)
            {
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Корневой элемент скрыт или неактивен. Conflict: beyondTheGridBoundary", _coroutineRunner);
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();

            if (currentHoverGridPosition.x < 0)
            {
                InventoryStorage.MainTelegraph.Hide();
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

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
                InventoryStorage.MainTelegraph.Hide();
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Telegraph скрыт из-за конфликта: {_placementResults.Conflict}", _coroutineRunner);
            }
            else
            {
                Vector2 globalTelegraphPos = GetGlobalCellPosition(_placementResults.SuggestedGridPosition);
                InventoryStorage.MainTelegraph.SetPosition(globalTelegraphPos);
                InventoryStorage.MainTelegraph.SetPlacement(_placementResults.Conflict, itemGridSize.x * CellSize.x, itemGridSize.y * CellSize.y);
                //Debug.Log($"[GridPageElementBase:{_root.name}] ShowPlacementTarget: Telegraph показан на позиции {globalTelegraphPos} с размером {itemGridSize.x * CellSize.x}x{itemGridSize.y * CellSize.y}. Conflict: {_placementResults.Conflict}", _coroutineRunner);
            }

            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x * CellSize.x, _placementResults.SuggestedGridPosition.y * CellSize.y),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }

        /// <summary>
        /// Рассчитывает текущую позицию курсора над сеткой в координатах сетки.
        /// </summary>
        /// <returns>Позиция в сетке (X, Y).</returns>
        protected Vector2Int CalculateCurrentHoverGridPosition()
        {
            _gridRect = _inventoryGrid.worldBound;
            Vector2 mouseInParentSpace = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);
            if (!_gridRect.Contains(_itemsPage.MouseUILocalPosition))
                return new Vector2Int(-1, -1);

            float adjustedX = Mathf.Clamp(_itemsPage.MouseUILocalPosition.x, _gridRect.xMin, _gridRect.xMax);
            float adjustedY = Mathf.Clamp(_itemsPage.MouseUILocalPosition.y, _gridRect.yMin, _gridRect.yMax);
            int gridX = Mathf.FloorToInt((adjustedX - _gridRect.xMin) / CellSize.x);
            int gridY = Mathf.FloorToInt((adjustedY - _gridRect.yMin) / CellSize.y);

            return new Vector2Int(gridX, gridY);
        }

        /// <summary>
        /// Находит визуальные элементы предметов, которые перекрываются с заданной областью сетки.
        /// </summary>
        /// <param name="start">Начальная позиция области в сетке.</param>
        /// <param name="size">Размер области.</param>
        /// <param name="draggedItem">Перетаскиваемый предмет, который исключается из проверки на перекрытие.</param>
        /// <returns>Список перекрывающихся визуальных элементов предметов.</returns>
        protected List<ItemVisual> FindOverlappingItems(Vector2Int start, Vector2Int size, ItemVisual draggedItem)
        {
            List<ItemVisual> overlappingItems = new List<ItemVisual>();
            RectInt targetRect = new RectInt(start.x, start.y, size.x, size.y);

            foreach (var entry in _visuals)
            {
                ItemVisual currentItem = entry.Key;
                if (currentItem == draggedItem) continue;

                ItemGridData gridData = entry.Value;
                RectInt currentItemRect = new RectInt(gridData.GridPosition.x, gridData.GridPosition.y, gridData.GridSize.x, gridData.GridSize.y);

                if (targetRect.Overlaps(currentItemRect))
                    overlappingItems.Add(currentItem);
            }
            return overlappingItems;
        }

        /// <summary>
        /// Завершает операцию перетаскивания, скрывая телеграф.
        /// </summary>
        public virtual void FinalizeDrag()
        {
            InventoryStorage.MainTelegraph.Hide();
            InventoryStorage.CurrentDraggedItem = null;
        }

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
            Debug.Log($"RemoveStoredItem : {ItemContainer.ItemRemoveReason.Destroy}");
            _itemContainer.RemoveItem(GetItemDefinition(storedItem), ItemContainer.ItemRemoveReason.Destroy);
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
            InventoryStorage.CurrentDraggedItem = storedItem;
            _document.rootVisualElement.Add(storedItem);
            storedItem.SetOwnerInventory(this);
            ServiceProvider.Get<InventoryAPI>().RaiseItemPickUp(storedItem, this);
        }

        /// <summary>
        /// Помещает предмет обратно в сетку на указанную позицию.
        /// </summary>
        /// <param name="storedItem">Визуальный элемент предмета, который нужно поместить.</param>
        /// <param name="gridPosition">Позиция в сетке для размещения.</param>
        public virtual void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            //Debug.Log($"[GridPageElementBase:{Root.name}] Drop: Вызываю MoveItem для '{storedItem.name}' (ID: {storedItem.ItemDefinition.ID}) в позицию {gridPosition}.");
            _itemContainer.MoveItem(storedItem.ItemDefinition, gridPosition);
            ServiceProvider.Get<InventoryAPI>().RiseItemDrop(storedItem, this);
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

        /// <summary>
        /// Конвертирует локальные координаты сетки в глобальные координаты относительно rootVisualElement.
        /// </summary>
        /// <param name="gridPosition">Позиция в сетке (X, Y).</param>
        /// <returns>Глобальная позиция в пикселях.</returns>
        public Vector2 GetGlobalCellPosition(Vector2Int gridPosition)
        {
            Vector2 inventoryGridGlobalPosition = _inventoryGrid.worldBound.position;
            //Debug.Log($"gridPosition '{gridPosition} . (rootGlobalPosition {inventoryGridGlobalPosition}");
            float globalX = inventoryGridGlobalPosition.x + gridPosition.x * CellSize.x;
            float globalY = inventoryGridGlobalPosition.y + gridPosition.y * CellSize.y;
            return new Vector2(globalX, globalY);
        }
    }
}