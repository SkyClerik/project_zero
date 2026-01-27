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
        private static StoredItem _currentDraggedItem = null;

        [SerializeField]
        private ItemContainer _playerInventory;

        public static StoredItem CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

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

            if (_currentDraggedItem == null || _currentDraggedItem.ItemVisual == null)
                return;

            _mousePositionNormal = Input.mousePosition;
            _mousePositionNormal.x = _mousePositionNormal.x - (_currentDraggedItem.ItemVisual.layout.width / 2);
            _mousePositionNormal.y = (Screen.height - _mousePositionNormal.y) - (_currentDraggedItem.ItemVisual.layout.height / 2);
            _currentDraggedItem.ItemVisual.SetPosition(_mousePositionNormal);
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