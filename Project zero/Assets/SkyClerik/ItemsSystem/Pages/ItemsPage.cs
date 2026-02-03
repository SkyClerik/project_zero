using SkyClerik.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    public class ContainerAndPage
    {
        [SerializeField]
        private ItemContainer _container;
        [SerializeField]
        private GridPageElementBase _page;

        public ItemContainer Container => _container;
        public GridPageElementBase Page => _page;

        public ContainerAndPage(ItemContainer itemContainer, GridPageElementBase gridPageElementBase)
        {
            _container = itemContainer;
            _page = gridPageElementBase;
        }
    }

    public class ItemsPage : MonoBehaviour
    {
        private UIDocument _document;
        private Vector2 _mousePositionOffset;

        private Vector2 _mouseUILocalPosition;
        public Vector2 MouseUILocalPosition => _mouseUILocalPosition;

        private static ItemVisual _currentDraggedItem = null;
        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

        private ItemTooltip _itemTooltip;
        private Coroutine _tooltipShowCoroutine;
        private const float _tooltipDelay = 0.5f;

        [SerializeField]
        private ItemContainer _inventoryItemContainer;
        private InventoryPageElement _inventoryPage;
        private ItemBaseDefinition _givenItem = null;
        public ItemBaseDefinition GiveItem => _givenItem;
        public bool IsInventoryVisible { get => _inventoryPage.Root.enabledSelf; set => _inventoryPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _craftItemContainer;
        private CraftPageElement _craftPage;
        public bool IsCraftVisible { get => _craftPage.Root.enabledSelf; set => _craftPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _cheastItemContainer;
        private CheastPageElement _cheastPage;
        public bool IsCheastVisible { get => _cheastPage.Root.enabledSelf; set => _cheastPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _lutItemContainer;
        private LutPageElement _lutPage;
        public bool IsLutVisible { get => _lutPage.Root.enabledSelf; set => _lutPage.Root.SetEnabled(value); }

        private List<ContainerAndPage> _containersAndPages = new List<ContainerAndPage>();
        public List<ContainerAndPage> ContainersAndPages => _containersAndPages;

        private GlobalGameProperty _globalGameProperty;

        public delegate void OnItemGivenDelegate(ItemBaseDefinition item);
        public event OnItemGivenDelegate OnItemGiven;
        public void RiseItemGiveEvent(ItemBaseDefinition item)
        {
            Debug.Log($"TriggerItemGiveEvent: {item}");
            OnItemGiven?.Invoke(item);
        }

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
            _inventoryPage?.Dispose();
            _craftPage?.Dispose();
            _cheastPage?.Dispose();
            _lutPage?.Dispose();
        }

        protected void Start()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: false);
            _document.enabled = true;

            _inventoryPage = new InventoryPageElement(itemsPage: this, document: _document, itemContainer: _inventoryItemContainer);
            var inventoryCA = new ContainerAndPage(_inventoryItemContainer, _inventoryPage);
            _containersAndPages.Add(inventoryCA);

            _craftPage = new CraftPageElement(itemsPage: this, document: _document, itemContainer: _craftItemContainer);
            var craftCA = new ContainerAndPage(_craftItemContainer, _craftPage);
            _containersAndPages.Add(craftCA);

            _cheastPage = new CheastPageElement(itemsPage: this, document: _document, itemContainer: _cheastItemContainer);
            var cheastCA = new ContainerAndPage(_cheastItemContainer, _cheastPage);
            _containersAndPages.Add(cheastCA);

            _lutPage = new LutPageElement(itemsPage: this, document: _document, itemContainer: _lutItemContainer);
            var lutCA = new ContainerAndPage(_lutItemContainer, _lutPage);
            _containersAndPages.Add(lutCA);

            _itemTooltip = new ItemTooltip();
            _document.rootVisualElement.Add(_itemTooltip);

            _globalGameProperty = ServiceProvider.Get<GlobalManager>()?.GlobalGameProperty;

            CloseAll();
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

            // -----
            if (_cheastPage.Root.enabledSelf)
            {
                //Debug.Log($"[ЛОG] Проверяю страницу сундука ({_cheastPage.Root.name}).");
                PlacementResults resultsCheast = _cheastPage.ShowPlacementTarget(draggedItem);
                if (resultsCheast.Conflict != ReasonConflict.beyondTheGridBoundary)
                {
                    //Debug.Log($"[ЛОГ] Страница сундука активна. Конфликт: {resultsCheast.Conflict}. Скрываю телеграф инвентаря.");
                    _inventoryPage.Telegraph.Hide();
                    return resultsCheast.Init(resultsCheast.Conflict, resultsCheast.Position, resultsCheast.SuggestedGridPosition, resultsCheast.OverlapItem, _cheastPage);
                }
            }
            // -----
            if (_lutPage.Root.enabledSelf)
            {
                //Debug.Log($"[ЛОG] Проверяю страницу лута ({_lutPage.Root.name}).");
                PlacementResults lutCheast = _lutPage.ShowPlacementTarget(draggedItem);
                if (lutCheast.Conflict != ReasonConflict.beyondTheGridBoundary)
                {
                    //Debug.Log($"[ЛОГ] Страница лута активна. Конфликт: {lutCheast.Conflict}. Скрываю телеграф инвентаря.");
                    _inventoryPage.Telegraph.Hide();
                    return lutCheast.Init(lutCheast.Conflict, lutCheast.Position, lutCheast.SuggestedGridPosition, lutCheast.OverlapItem, _lutPage);
                }
            }

            // -----
            if (_craftPage.Root.enabledSelf)
            {
                if (_globalGameProperty != null && _globalGameProperty.MakeCraftAccessible)
                {
                    PlacementResults resultsTwo = _craftPage.ShowPlacementTarget(draggedItem);
                    if (resultsTwo.Conflict != ReasonConflict.beyondTheGridBoundary)
                    {
                        //Debug.Log($"[ЛОГ] Страница крафта активна. Конфликт: {resultsTwo.Conflict}. Скрываю телеграф инвентаря.");
                        _inventoryPage.Telegraph.Hide();
                        return resultsTwo.Init(resultsTwo.Conflict, resultsTwo.Position, resultsTwo.SuggestedGridPosition, resultsTwo.OverlapItem, _craftPage);
                    }
                }
            }

            // -----

            //Debug.Log("[ЛОГ] Ни одна страница не подходит. Скрываю оба телеграфа.");
            _inventoryPage.Telegraph.Hide();
            _craftPage.Telegraph.Hide();
            _cheastPage.Telegraph.Hide();
            _lutPage.Telegraph.Hide();
            return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
        }

        public void FinalizeDragOfItem(ItemVisual draggedItem)
        {
            _inventoryPage.FinalizeDrag();
            _craftPage.FinalizeDrag();
            _cheastPage.FinalizeDrag();
            _lutPage.FinalizeDrag();
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
                sourceContainer.AddItems(new List<ItemBaseDefinition> { itemToMove });
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

        /// <summary>
        /// Откроет инвентарь для выбора предмета который найдет по индексу, если не найдет то и не откроет инвентарь
        /// </summary>
        /// <param name="wrapperIndex"></param>
        public void OpenInventoryFromGiveItem(int wrapperIndex)
        {
            _givenItem = _inventoryItemContainer.GetItemByWrapperIndex(wrapperIndex);
            if (_givenItem != null)
                OpenInventoryNormal();
        }

        /// <summary>
        /// Откроет инвентарь для выбора указанного предмета. Не откроет если ссылка null
        /// </summary>
        /// <param name="wrapperIndex"></param>
        public void OpenInventoryGiveItem(ItemBaseDefinition item)
        {
            _givenItem = item;
            if (_givenItem != null)
                OpenInventoryNormal();
        }

        public void OpenInventoryNormal()
        {
            _document.rootVisualElement.SetVisibility(true);
            _inventoryPage.Root.SetEnabled(true);

            _document.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
            //_inventoryPage.SetLogicalGridVisualizerActive(false);
        }

        public void CloseInventory()
        {
            _givenItem = null;
            _document.rootVisualElement.SetVisibility(false);
            _inventoryPage.Root.SetEnabled(false);

            _document.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);
        }

        public void OpenCraft()
        {
            _craftPage.Root.SetDisplay(true);
            _craftPage.Root.SetVisibility(false);
            if (_globalGameProperty != null && _globalGameProperty.MakeCraftAccessible)
                SetDisplaySelfPage(_craftPage);
        }

        public void CloseCraft()
        {
            _craftPage.Root.SetVisibility(false);
            _craftPage.Root.SetEnabled(false);
        }

        public void OpenCheast()
        {
            SetDisplaySelfPage(_cheastPage);
        }

        public void CloseCheast()
        {
            _cheastPage.Root.SetVisibility(false);
            _cheastPage.Root.SetEnabled(false);
        }

        public void OpenLut()
        {
            SetDisplaySelfPage(_lutPage);
        }

        public void CloseLut()
        {
            _lutPage.Root.SetVisibility(false);
            _lutPage.Root.SetEnabled(false);
        }

        public void CloseAll()
        {
            CloseInventory();
            CloseCraft();
            CloseCheast();
            CloseLut();
        }

        private void SetDisplaySelfPage(GridPageElementBase page)
        {
            _craftPage.Root.SetDisplay(false);
            _cheastPage.Root.SetDisplay(false);
            _lutPage.Root.SetDisplay(false);

            page.Root.SetVisibility(true);
            page.Root.SetDisplay(true);
            page.Root.SetEnabled(true);
        }
    }
}