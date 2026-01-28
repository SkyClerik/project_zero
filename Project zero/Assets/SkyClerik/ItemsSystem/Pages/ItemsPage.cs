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
        private ItemContainerBase _inventoryItemContainer;
        [SerializeField]
        private ItemContainerBase _craftItemContainer;

        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }
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
                inventoryPageRoot: out _,
                itemContainer: _inventoryItemContainer);

            _craftPage = new CraftPageElement(
                itemsPage: this,
                document: _document,
                inventoryTwoPageRoot: out _craftPageRoot,
                itemContainer: _craftItemContainer);

            _itemTooltip = new ItemTooltip();
            _document.rootVisualElement.Add(_itemTooltip);
            _document.rootVisualElement.SetVisibility(false);
            _craftPageRoot.SetVisibility(_craftElementVisible);
            //RootPosition(DisplayStyle.None);
        }

        //private void RootPosition(DisplayStyle display)
        //{
        //    int enumValue = (int)display;
        //    int offset = enumValue * 900000;
        //    _document.rootVisualElement.style.top = offset;
        //}

        //переместить под общий fixedUpdate после тестов
        private void FixedUpdate()
        {
            if (!_document.isActiveAndEnabled)
                return;

            if (_currentDraggedItem == null)
                return;

            _mousePositionNormal = Input.mousePosition;
            _mousePositionNormal.x = _mousePositionNormal.x - (_currentDraggedItem.layout.width / 2);
            _mousePositionNormal.y = (Screen.height - _mousePositionNormal.y) - (_currentDraggedItem.layout.height / 2);
            _currentDraggedItem.SetPosition(_mousePositionNormal);
        }

        public PlacementResults HandleItemPlacement(ItemVisual draggedItem)
        {
            // Проверяем первый инвентарь
            PlacementResults resultsPage = _inventoryPage.ShowPlacementTarget(draggedItem);
            Debug.Log($"[ItemsPage.HandleItemPlacement] _inventoryPage results: Conflict={resultsPage.Conflict}, Position={resultsPage.Position}, OverlapItem={resultsPage.OverlapItem?.name}");
            if (resultsPage.Conflict != ReasonConflict.beyondTheGridBoundary)
            {
                _craftPage.Telegraph.Hide(); // Скрываем телеграф второго инвентаря, если первый активен
                return resultsPage.Init(resultsPage.Conflict, resultsPage.Position, resultsPage.OverlapItem, _inventoryPage);
            }

            // Если первый инвентарь не активен, проверяем второй
            PlacementResults resultsTwo = _craftPage.ShowPlacementTarget(draggedItem);
            Debug.Log($"[ItemsPage.HandleItemPlacement] _craftPage results: Conflict={resultsTwo.Conflict}, Position={resultsTwo.Position}, OverlapItem={resultsTwo.OverlapItem?.name}");
            if (resultsTwo.Conflict != ReasonConflict.beyondTheGridBoundary)
            {
                _inventoryPage.Telegraph.Hide(); // Скрываем телеграф первого инвентаря, если второй активен
                return resultsTwo.Init(resultsTwo.Conflict, resultsTwo.Position, resultsTwo.OverlapItem, _craftPage);
            }

            // Если ни один инвентарь не является целью, скрываем оба телеграфа и возвращаем конфликт
            _inventoryPage.Telegraph.Hide();
            _craftPage.Telegraph.Hide();
            return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, null, null);
        }

        public void FinalizeDragOfItem(ItemVisual draggedItem)
        {
            _inventoryPage.FinalizeDrag();
            _craftPage.FinalizeDrag();
        }

        public void TransferItemBetweenContainers(ItemVisual draggedItem, IDropTarget sourceInventory, IDropTarget targetInventory, Vector2 placementPosition)
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

            targetInventory.Drop(draggedItem, placementPosition);
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