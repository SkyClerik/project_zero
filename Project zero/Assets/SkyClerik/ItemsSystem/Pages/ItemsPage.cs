using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    public class ItemsPage : MonoBehaviour
    {
        private UIDocument _document;
        private InventoryPageElement _inventoryPage;
        private CraftPageElement _craftPage;
        private bool _craftAccessible = false;
        private Vector2 _mouseUILocalPosition;
        private Vector2 _mousePositionOffset;
        private static ItemVisual _currentDraggedItem = null;

        private ItemTooltip _itemTooltip;
        private Coroutine _tooltipShowCoroutine;
        private const float _tooltipDelay = 0.5f;
        private ItemBaseDefinition _givenItem = null;

        [SerializeField]
        private ItemContainer _inventoryItemContainer;
        [SerializeField]
        private ItemContainer _craftItemContainer;

        public delegate void OnItemGivenDelegate(ItemBaseDefinition item);
        public event OnItemGivenDelegate OnItemGiven;

        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }
        public InventoryPageElement InventoryPage => _inventoryPage;
        public CraftPageElement CraftPage => _craftPage;
        public ItemBaseDefinition GiveItem => _givenItem;
        public bool IsInventoryVisible { get => _inventoryPage.Root.enabledSelf; set => _inventoryPage.Root.SetEnabled(value); }
        public bool IsCraftVisible { get => _craftPage.Root.enabledSelf; set => _craftPage.Root.SetEnabled(value); }
        public bool MakeCraftAccessible { get => _craftAccessible; set => _craftAccessible = value; }
        public Vector2 MouseUILocalPosition => _mouseUILocalPosition;
        public ItemContainer InventoryItemContainer => _inventoryItemContainer;
        public ItemContainer CraftItemContainer => _craftItemContainer;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
            _inventoryPage?.Dispose();
            _craftPage?.Dispose();
        }

        protected void Start()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: false);
            _document.enabled = true;

            _inventoryPage = new InventoryPageElement(
                itemsPage: this,
                document: _document,
                itemContainer: _inventoryItemContainer);

            _craftPage = new CraftPageElement(
                itemsPage: this,
                document: _document,
                itemContainer: _craftItemContainer);

            _itemTooltip = new ItemTooltip();
            _document.rootVisualElement.Add(_itemTooltip);
            CloseInventory();
            CloseCraft();
        }

        private void OnRootMouseMove(MouseMoveEvent evt)
        {
            _mouseUILocalPosition = evt.localMousePosition;
        }

        private void Update()
        {
            if (!_document.isActiveAndEnabled)
                return;

            if (_currentDraggedItem == null)
                return;

            if (Input.GetMouseButtonDown(1))
                _currentDraggedItem.Rotate();

            SetDraggedItemPosition();
        }

        public void SetDraggedItemPosition()
        {
            _mousePositionOffset.x = _mouseUILocalPosition.x - (_currentDraggedItem.resolvedStyle.width / 2);
            _mousePositionOffset.y = _mouseUILocalPosition.y - (_currentDraggedItem.resolvedStyle.height / 2);
            _currentDraggedItem.SetPosition(_mousePositionOffset);
        }

        public PlacementResults HandleItemPlacement(ItemVisual draggedItem)
        {
            //Debug.Log($"[ЛОG] Проверяю страницу инвентаря ({_inventoryPage.Root.name}).");
            PlacementResults resultsPage = _inventoryPage.ShowPlacementTarget(draggedItem);

            if (resultsPage.Conflict == ReasonConflict.None || resultsPage.Conflict == ReasonConflict.StackAvailable || resultsPage.Conflict == ReasonConflict.SwapAvailable)
            {
                //Debug.Log($"[ЛОГ] Страница инвентаря активна. Конфликт: {resultsPage.Conflict}. Скрываю телеграф крафта.");
                _craftPage.Telegraph.Hide();
                return resultsPage.Init(resultsPage.Conflict, resultsPage.Position, resultsPage.SuggestedGridPosition, resultsPage.OverlapItem, _inventoryPage);
            }

            if (!_craftAccessible)
            {
                //Debug.Log($"[ЛОГ] Крафт не видимый и мы пропускаем размещение в него");
                _inventoryPage.Telegraph.Hide();
                _craftPage.Telegraph.Hide();
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }
            else
            {
                PlacementResults resultsTwo = _craftPage.ShowPlacementTarget(draggedItem);
                if (resultsTwo.Conflict != ReasonConflict.beyondTheGridBoundary)
                {
                    //Debug.Log($"[ЛОГ] Страница крафта активна. Конфликт: {resultsTwo.Conflict}. Скрываю телеграф инвентаря.");
                    _inventoryPage.Telegraph.Hide();
                    return resultsTwo.Init(resultsTwo.Conflict, resultsTwo.Position, resultsTwo.SuggestedGridPosition, resultsTwo.OverlapItem, _craftPage);
                }
            }

            //Debug.Log("[ЛОГ] Ни одна страница не подходит. Скрываю оба телеграфа.");
            _inventoryPage.Telegraph.Hide();
            _craftPage.Telegraph.Hide();
            return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
        }

        public void FinalizeDragOfItem(ItemVisual draggedItem)
        {
            _inventoryPage.FinalizeDrag();
            _craftPage.FinalizeDrag();
        }

        public void TransferItemBetweenContainers(ItemVisual draggedItem, IDropTarget sourceInventory, IDropTarget targetInventory, Vector2Int gridPosition)
        {
            var itemToMove = draggedItem.ItemDefinition;
            var sourceContainer = (sourceInventory as GridPageElementBase)?.ItemContainer;
            var targetContainer = (targetInventory as GridPageElementBase)?.ItemContainer;

            if (sourceContainer == null || targetContainer == null)
            {
                //Debug.LogError("Не удалось найти контейнеры для перемещения предмета!");
                return;
            }

            // 1. Удаляем предмет из исходного контейнера (это вызовет OnItemRemoved в UI)
            sourceContainer.RemoveItem(itemToMove, destroy: false); 

            // 2. Пытаемся добавить предмет в целевой контейнер на указанную позицию Это вызовет OnItemAdded в UI, если успешно
            bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToMove, gridPosition);

            if (!addedToTarget)
            {
                //Debug.LogWarning($"Не удалось переместить предмет '{itemToMove.name}' в целевой контейнер на позицию {gridPosition}. Возвращаем в исходный контейнер.");
                // Если не удалось добавить в целевой, возвращаем предмет в исходный контейнер
                // Это может вызвать OnItemAdded в UI исходного контейнера, ItemContainer сам найдет место
                sourceContainer.AddItems(new List<ItemBaseDefinition>{ itemToMove }); 
            }
        }

        public void StartTooltipDelay(ItemVisual itemVisual)
        {
            if (CurrentDraggedItem != null)
                return;

            StopTooltipDelayAndHideTooltip();
            _tooltipShowCoroutine = StartCoroutine(ShowTooltipCoroutine(itemVisual));
        }

        public void StopTooltipDelayAndHideTooltip()
        {
            if (_tooltipShowCoroutine != null)
            {
                StopCoroutine(_tooltipShowCoroutine);
                _tooltipShowCoroutine = null;
            }
            _itemTooltip.HideTooltip();
        }

        private IEnumerator ShowTooltipCoroutine(ItemVisual itemVisual)
        {
            yield return new WaitForSeconds(_tooltipDelay);
            _itemTooltip.ShowTooltip(itemVisual.ItemDefinition, itemVisual.worldBound.center);
        }

        public void OpenInventoryGiveItem(int itemId)
        {
            //TODO стоит заглушка, нужно написать скрипт поиска предмета по ID
            //_giveItem = itemId;
            _givenItem = null;
            OpenInventoryNormal();
        }

        public void OpenInventoryGiveItem(ItemBaseDefinition item)
        {
            _givenItem = item;
            OpenInventoryNormal();
        }

        public void OpenInventoryNormal()
        {
            _inventoryPage.Root.SetEnabled(true);
            _document.rootVisualElement.SetVisibility(true);
            _document.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
            _inventoryPage.SetLogicalGridVisualizerActive(false);
        }

        public void CloseInventory()
        {
            _givenItem = null;
            _inventoryPage.Root.SetEnabled(false);
            _document.rootVisualElement.SetVisibility(false);
            _document.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);
            _inventoryPage.SetLogicalGridVisualizerActive(false);
        }

        public void OpenCraft()
        {
            _craftPage.Root.SetVisibility(false);
            if (_craftAccessible)
            {
                _craftPage.Root.SetVisibility(true);
                _craftPage.Root.SetEnabled(true);
                _craftPage.SetLogicalGridVisualizerActive(false);
            }
        }

        public void CloseCraft()
        {
            _craftPage.Root.SetVisibility(false);
            _craftPage.Root.SetEnabled(false);
            _craftPage.SetLogicalGridVisualizerActive(false);
        }

        public void TriggerItemGiveEvent(ItemBaseDefinition item)
        {
            Debug.Log($"TriggerItemGiveEvent: {item}");
            OnItemGiven?.Invoke(item);
        }
    }
}