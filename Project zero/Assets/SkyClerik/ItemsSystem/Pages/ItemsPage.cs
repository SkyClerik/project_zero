using System.Collections;
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
        private bool _showInventory = false;
        private CraftPageElement _craftPage;
        private bool _craftElementVisible = false;
        private Vector2 _mouseUILocalPosition;
        private Vector2 _mousePositionOffset;
        private static ItemVisual _currentDraggedItem = null;

        private ItemTooltip _itemTooltip;
        private Coroutine _tooltipShowCoroutine;
        private const float _tooltipDelay = 0.5f;
        private ItemBaseDefinition _givenItem = null;

        [SerializeField]
        private ItemContainerBase _inventoryItemContainer;
        [SerializeField]
        private ItemContainerBase _craftItemContainer;

        public delegate void OnItemGivenDelegate(ItemBaseDefinition item);
        public event OnItemGivenDelegate OnItemGiven;

        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }
        public InventoryPageElement InventoryPage => _inventoryPage;
        public ItemBaseDefinition GiveItem => _givenItem;
        public bool IsCraftVisible { get => _craftElementVisible; set => _craftElementVisible = value; }

        public bool IsInventoryVisible => _showInventory;
        public Vector2 MouseUILocalPosition => _mouseUILocalPosition;
        public UIDocument Document { get => _document; set => _document = value; }

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
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
            _document.rootVisualElement.SetVisibility(false);
            _craftPage.Root.SetVisibility(_craftElementVisible);
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
            if (resultsPage.Conflict != ReasonConflict.beyondTheGridBoundary)
            {
                //Debug.Log($"[ЛОГ] Страница инвентаря активна. Конфликт: {resultsPage.Conflict}. Скрываю телеграф крафта.");
                _craftPage.Telegraph.Hide();
                return resultsPage.Init(resultsPage.Conflict, resultsPage.Position, resultsPage.SuggestedGridPosition, resultsPage.OverlapItem, _inventoryPage);
            }

            //Debug.Log($"[ЛОГ] Страница инвентаря не подходит. Проверяю страницу крафта ({_craftPage.Root.name}).");
            PlacementResults resultsTwo = _craftPage.ShowPlacementTarget(draggedItem);
            if (resultsTwo.Conflict != ReasonConflict.beyondTheGridBoundary)
            {
                //Debug.Log($"[ЛОГ] Страница крафта активна. Конфликт: {resultsTwo.Conflict}. Скрываю телеграф инвентаря.");
                _inventoryPage.Telegraph.Hide();
                return resultsTwo.Init(resultsTwo.Conflict, resultsTwo.Position, resultsTwo.SuggestedGridPosition, resultsTwo.OverlapItem, _craftPage);
            }

            //Debug.Log("[ЛОГ] Ни одна страница не подходит. Скрываю оба телеграфа.");
            _inventoryPage.Telegraph.Hide();
            _craftPage.Telegraph.Hide();
            return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null); // Изменили null на Vector2.zero для position
        }

        public void FinalizeDragOfItem(ItemVisual draggedItem)
        {
            _inventoryPage.FinalizeDrag();
            _craftPage.FinalizeDrag();
        }

        public void TransferItemBetweenContainers(ItemVisual draggedItem, IDropTarget sourceInventory, IDropTarget targetInventory, Vector2Int gridPosition)
        {
            var itemToMove = draggedItem.ItemDefinition;
            //Debug.Log($"[DIAGNOSTIC] TransferItemBetweenContainers: Moving '{itemToMove.name}'. Source is '{sourceInventory.GetType().Name}', Target is '{targetInventory.GetType().Name}'.");

            var sourceContainer = (sourceInventory as GridPageElementBase)?.ItemContainer;
            var targetContainer = (targetInventory as GridPageElementBase)?.ItemContainer;

            if (sourceContainer == null || targetContainer == null)
            {
                Debug.LogError("Could not find containers for transfer!");
                return;
            }

            sourceContainer.RemoveItem(itemToMove, destroy: false);
            targetContainer.AddItemReference(itemToMove);

            targetInventory.Drop(draggedItem, gridPosition);
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
            _itemTooltip.ShowTooltip(itemVisual.ItemDefinition, Input.mousePosition);
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
            _showInventory = true;
            _document.rootVisualElement.SetVisibility(true);
            _document.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
        }

        public void CloseInventory()
        {
            _givenItem = null;
            _showInventory = false;
            _document.rootVisualElement.SetVisibility(false);
            _document.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);
        }

        public void OpenCraft()
        {
            _craftPage.Root.SetVisibility(false);
            if (_craftElementVisible)
                _craftPage.Root.SetVisibility(true);
        }

        public void CloseCraft()
        {
            _craftPage.Root.SetVisibility(false);
        }

        public void TriggerItemGiveEvent(ItemBaseDefinition item)
        {
            Debug.Log($"TriggerItemGiveEvent: {item}");
            OnItemGiven?.Invoke(item);
        }
    }
}