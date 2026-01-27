using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Inventory
{
    public class CharacterPages : MonoBehaviour
    {
        private UIDocument _document;
        private InventoryPage _inventoryPage;
        private VisualElement _inventoryPageRoot;
        private Vector2 _mousePositionNormal;
        private static ItemVisual _currentDraggedItem = null;

        [SerializeField]
        private ItemContainer _playerInventory;

        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

        protected void Start()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: false);
            _document.enabled = true;

            _inventoryPage = new InventoryPage(
                document: _document,
                coroutineRunner: this,
                inventoryPageRoot: out _inventoryPageRoot,
                itemContainer: _playerInventory);

            //_inventoryPage.Hide();
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
    }
}