using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Inventory
{
    public class ItemsPage : MonoBehaviour
    {
        private UIDocument _document;
        private InventoryPageElement _inventoryPage;
        private CraftPageElement _craftPage;
        private VisualElement _inventoryPageRoot;
        private VisualElement _craftPageRoot;
        private Vector2 _mousePositionNormal;
        private static ItemVisual _currentDraggedItem = null;

        [SerializeField]
        private InventoryItemContainer _inventoryItemContainer;
        [SerializeField]
        private CraftItemsContainer _craftItemContainer;

        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

        protected void Start()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: false);
            _document.enabled = true;

            _inventoryPage = new InventoryPageElement(
                itemsPage: this,
                document: _document,
                inventoryPageRoot: out _inventoryPageRoot,
                itemContainer: _inventoryItemContainer);

            _craftPage = new CraftPageElement(
                itemsPage: this,
                document: _document,
                inventoryTwoPageRoot: out _craftPageRoot,
                itemContainer: _craftItemContainer);

            //_inventoryPage.Hide();
            //_craftPage.Hide();
        }

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

        public void OpenCloseInventory()
        {
            if (_inventoryPageRoot.style.display == DisplayStyle.Flex)
                _inventoryPage.Hide();
            else
                _inventoryPage.Show();
        }

        public void OpenCloseCraft()
        {
            if (_craftPageRoot.style.display == DisplayStyle.Flex)
                _craftPage.Hide();
            else
                _craftPage.Show();
        }

        public PlacementResults HandleItemPlacement(ItemVisual draggedItem)
        {
            // Проверяем первый инвентарь
            PlacementResults resultsPage = _inventoryPage.ShowPlacementTarget(draggedItem);
            if (resultsPage.Conflict != ReasonConflict.beyondTheGridBoundary)
            {
                _craftPage.Telegraph.Hide(); // Скрываем телеграф второго инвентаря, если первый активен
                return resultsPage.Init(resultsPage.Conflict, resultsPage.Position, resultsPage.OverlapItem, _inventoryPage);
            }

            // Если первый инвентарь не активен, проверяем второй
            PlacementResults resultsTwo = _craftPage.ShowPlacementTarget(draggedItem);
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
    }
}