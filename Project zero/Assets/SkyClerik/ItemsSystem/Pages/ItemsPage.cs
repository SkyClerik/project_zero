using System.Collections;
using UnityEngine;
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
        private VisualElement _craftPageRoot;
        private bool _craftElementVisible = false;
        private Vector2 _mousePositionNormal;
        private static ItemVisual _currentDraggedItem = null;

        private ItemTooltip _itemTooltip;
        private Coroutine _tooltipShowCoroutine;
        private const float _tooltipDelay = 0.5f;

        [SerializeField]
        [Tooltip("Размер одной ячейки в пикселях")]
        private Vector2 _defaultCellSize = new Vector2(128, 128);
        [SerializeField]
        [Tooltip("Ширина и высота инвентаря в ячейках")]
        private Vector2Int _inventoryGridSize = new Vector2Int(7, 8);
        [SerializeField]
        [Tooltip("Ширина и высота стола крафта в ячейках")]
        private Vector2Int _craftGridSize = new Vector2Int(3, 4);
        [SerializeField]
        private ItemContainerBase _inventoryItemContainer;
        [SerializeField]
        private ItemContainerBase _craftItemContainer;

        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }
        public InventoryPageElement InventoryPage => _inventoryPage;
        public bool CraftElementVisible
        {
            get => _craftElementVisible;
            set
            {
                _craftElementVisible = value;
                _craftPageRoot.SetVisibility(_craftElementVisible);
            }
        }

        public bool ShowInventory
        {
            get => _showInventory;
            set
            {
                _showInventory = value;
                _document.rootVisualElement.SetVisibility(_showInventory);

                _craftPageRoot.SetVisibility(false);
                if (_craftElementVisible)
                    _craftPageRoot.SetVisibility(_showInventory);
            }
        }

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
                itemContainer: _inventoryItemContainer,
                cellSize: _defaultCellSize,
                inventoryGridSize: _inventoryGridSize);

            _craftPage = new CraftPageElement(
                itemsPage: this,
                document: _document,
                itemContainer: _craftItemContainer,
                cellSize: _defaultCellSize,
                inventoryGridSize: _craftGridSize);

            _craftPageRoot = _craftPage.Root;
            _itemTooltip = new ItemTooltip();
            _document.rootVisualElement.Add(_itemTooltip);
            _document.rootVisualElement.SetVisibility(false);
            _craftPageRoot.SetVisibility(_craftElementVisible);
        }

        //переместить под общий fixedUpdate после тестов
        private void FixedUpdate()
        {
            if (!_document.isActiveAndEnabled)
                return;

            if (_currentDraggedItem == null)
                return;

            _mousePositionNormal = Input.mousePosition;
            _mousePositionNormal.x = _mousePositionNormal.x - (_currentDraggedItem.resolvedStyle.width / 2);
            _mousePositionNormal.y = (Screen.height - _mousePositionNormal.y) - (_currentDraggedItem.resolvedStyle.height / 2);
            _currentDraggedItem.SetPosition(_mousePositionNormal);
        }

        private void Update()
        {
            if (!_document.isActiveAndEnabled)
                return;

            if (_currentDraggedItem == null)
                return;

            if (Input.GetMouseButtonDown(1))
                _currentDraggedItem.Rotate();
        }

        public PlacementResults HandleItemPlacement(ItemVisual draggedItem)
        {
            // Проверяем первый инвентарь
            PlacementResults resultsPage = _inventoryPage.ShowPlacementTarget(draggedItem);
            if (resultsPage.Conflict != ReasonConflict.beyondTheGridBoundary)
            {
                _craftPage.Telegraph.Hide(); // Скрываем телеграф второго инвентаря, если первый активен
                return resultsPage.Init(resultsPage.Conflict, resultsPage.Position, resultsPage.SuggestedGridPosition, resultsPage.OverlapItem, _inventoryPage);
            }

            // Если первый инвентарь не активен, проверяем второй
            PlacementResults resultsTwo = _craftPage.ShowPlacementTarget(draggedItem);
            if (resultsTwo.Conflict != ReasonConflict.beyondTheGridBoundary)
            {
                _inventoryPage.Telegraph.Hide(); // Скрываем телеграф первого инвентаря, если второй активен
                return resultsTwo.Init(resultsTwo.Conflict, resultsTwo.Position, resultsTwo.SuggestedGridPosition, resultsTwo.OverlapItem, _craftPage);
            }

            // Если ни один инвентарь не является целью, скрываем оба телеграфа и возвращаем конфликт
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

            // Удаляем предмет из исходного контейнера, НЕ уничтожая его
            if (sourceInventory is InventoryPageElement sourceInvElement)
                sourceInvElement.ItemContainer.RemoveItem(itemToMove, destroy: false);
            else if (sourceInventory is CraftPageElement sourceCraftElement)
                sourceCraftElement.ItemContainer.RemoveItem(itemToMove, destroy: false);

            // Добавляем ЭТОТ ЖЕ ЭКЗЕМПЛЯР предмета в целевой контейнер
            if (targetInventory is InventoryPageElement targetInvElement)
                targetInvElement.ItemContainer.AddItem(itemToMove);
            else if (targetInventory is CraftPageElement targetCraftElement)
                targetCraftElement.ItemContainer.AddItem(itemToMove);

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
    }
}