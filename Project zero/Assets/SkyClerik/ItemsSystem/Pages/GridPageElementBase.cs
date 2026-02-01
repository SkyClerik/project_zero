
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
        protected Rect _cellSize;

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

        // --- Свойства IDropTarget и прочие ---
        public UIDocument GetDocument => _document;
        public ItemContainer ItemContainer => _itemContainer;
        public Vector2 CellSize => new Vector2(_cellSize.width, _cellSize.height);
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
            yield return _coroutineRunner.StartCoroutine(Configure());
            yield return new WaitForEndOfFrame();
            CalculateCellSize();

            SubscribeToContainerEvents();

            // Загружаем визуальные элементы для предметов, которые уже есть в контейнере
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

        protected IEnumerator Configure()
        {
            _telegraph = new Telegraph();
            // Возвращаем логику, так как Telegraph должен быть частью UI
            AddItemToInventoryGrid(_telegraph);
            yield break;
        }

        // --- Обработчики событий от ItemContainer ---

        private void HandleItemAdded(ItemBaseDefinition item)
        {
            var existingVisual = _visuals.Keys.FirstOrDefault(visual => GetItemDefinition(visual) == item);
            if (existingVisual != null)
            {
                existingVisual.UpdatePcs();
                // Обновляем позицию, если она изменилась (важно для MoveItem)
                existingVisual.SetPosition(new Vector2(item.GridPosition.x * _cellSize.width, item.GridPosition.y * _cellSize.height));
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

        // --- Логика UI ---

        private void LoadInitialVisuals()
        {
            foreach (var item in _itemContainer.GetItems())
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
                gridSize: new Vector2Int(item.Dimensions.CurrentWidth, item.Dimensions.CurrentHeight));

            RegisterVisual(newItemVisual, newGridData);
            AddItemToInventoryGrid(newItemVisual);
            newItemVisual.SetPosition(new Vector2(item.GridPosition.x * _cellSize.width, item.GridPosition.y *
_cellSize.height));
        }

        protected virtual void CalculateCellSize()
        {
            if (_itemContainer.GridDimensions.x > 0 && _itemContainer.GridDimensions.y > 0)
            {
                _cellSize.width = _inventoryGrid.resolvedStyle.width / _itemContainer.GridDimensions.x;
                _cellSize.height = _inventoryGrid.resolvedStyle.height / _itemContainer.GridDimensions.y;
            }
            else
            {
                Debug.LogWarning($"Не удалось рассчитать размер ячейки для '{_root.name}', т.к. размеры сетки в ItemContainer не заданы.", _coroutineRunner);
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
            Vector2Int currentHoverGridPosition = CalculateCurrentHoverGridPosition();
            Vector2Int itemGridSize = new Vector2Int(draggedItem.ItemDefinition.Dimensions.CurrentWidth, draggedItem.ItemDefinition.Dimensions.CurrentHeight);
            _placementResults = new PlacementResults();
            _placementResults.OverlapItem = null;

            if (_itemContainer.IsGridAreaFree(currentHoverGridPosition, itemGridSize))
            {
                _placementResults.Conflict = ReasonConflict.None;
                _placementResults.SuggestedGridPosition = currentHoverGridPosition;
            }
            else
            {
                List<ItemVisual> overlappingItems = FindOverlappingItems(currentHoverGridPosition, itemGridSize,
draggedItem);
                if (overlappingItems.Count == 1)
                {
                    ItemVisual overlapItem = overlappingItems[0];
                    bool isSameStackableType = draggedItem.ItemDefinition.Stackable &&
                                               overlapItem.ItemDefinition.Stackable &&
                                               draggedItem.ItemDefinition.DefinitionName ==
overlapItem.ItemDefinition.DefinitionName;
                    _placementResults.Conflict = isSameStackableType ? ReasonConflict.StackAvailable :
ReasonConflict.SwapAvailable;
                    _placementResults.OverlapItem = overlapItem;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                }
                else
                {
                    _placementResults.Conflict = ReasonConflict.intersectsObjects;
                    _placementResults.SuggestedGridPosition = currentHoverGridPosition;
                }
            }

            if (!_itemContainer.IsGridAreaFree(currentHoverGridPosition, itemGridSize) &&
_placementResults.Conflict == ReasonConflict.None)
            {
                _placementResults.Conflict = ReasonConflict.beyondTheGridBoundary;
            }

            if (_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary || _placementResults.Conflict
== ReasonConflict.intersectsObjects)
            {
                _telegraph.Hide();
            }
            else
            {
                var pos = new Vector2(_placementResults.SuggestedGridPosition.x * _cellSize.width,
_placementResults.SuggestedGridPosition.y * _cellSize.height);
                _telegraph.SetPosition(pos);
                _telegraph.SetPlacement(_placementResults.Conflict, itemGridSize.x * _cellSize.width,
itemGridSize.y * _cellSize.height);
            }

            return _placementResults.Init(conflict: _placementResults.Conflict,
                                          position: new Vector2(_placementResults.SuggestedGridPosition.x *
_cellSize.width, _placementResults.SuggestedGridPosition.y * _cellSize.height),
                                          suggestedGridPosition: _placementResults.SuggestedGridPosition,
                                          overlapItem: _placementResults.OverlapItem,
                                          targetInventory: this);
        }

        protected Vector2Int CalculateCurrentHoverGridPosition()
        {
            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);
            int gridX = Mathf.FloorToInt(mouseLocalPosition.x / _cellSize.width);
            int gridY = Mathf.FloorToInt(mouseLocalPosition.y / _cellSize.height);
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
                _itemContainer.OccupyGridCells(itemDef, false);
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