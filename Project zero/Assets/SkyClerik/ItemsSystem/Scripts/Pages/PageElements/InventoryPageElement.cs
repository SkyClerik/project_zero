using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет элемент UI страницы инвентаря, управляющий отображением предметов
    /// и взаимодействием с пользователем. Наследует функциональность базовой страницы сетки.
    /// </summary>
    public class InventoryPageElement : GridPageElementBase
    {
        private const string _titleText = "Инвентарь";
        private VisualElement _itemImage;
        private const string _itemImageID = "item_image";
        private VisualElement _descriptionBackground;
        private const string _descriptionBackgroundID = "description_background";
        private Label _lDescription;
        private const string _lDescriptionID = "l_description";
        private Button _bClose;
        private const string _bCloseID = "b_close";
        private Button _bRotate;
        private const string _bRotateID = "b_rotate";

        private VisualElement _body;
        private const string _bodyID = "body";

        //private ItemsPage _itemsPage;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InventoryPageElement"/>.
        /// </summary>
        /// <param name="itemsPage">Ссылка на главную страницу предметов.</param>
        /// <param name="document">UIDocument, содержащий корневой визуальный элемент.</param>
        /// <param name="itemContainer">Контейнер предметов, связанный с этой страницей инвентаря.</param>
        public InventoryPageElement(ItemsPage itemsPage, UIDocument document, ItemContainer itemContainer)
            : base(itemsPage, document, itemContainer, itemContainer.RootPanelName)
        {
            _itemsPage = itemsPage;

            _body = _root.Q(_bodyID);
            _descriptionBackground = _root.Q(_descriptionBackgroundID);
            _bClose = _root.Q<Button>(_bCloseID);
            _bRotate = _root.Q<Button>(_bRotateID);
            _itemImage = _root.Q(_itemImageID);
            _lDescription = _root.Q<Label>(_lDescriptionID);

            _bClose.clicked += CloseClicked;
            _bRotate.RegisterCallback<PointerDownEvent>(OnRotatePointerDown);

            SetRotateButtonEnable(false);
        }

        public override void Dispose()
        {
            _bClose.clicked -= CloseClicked;
            _bRotate.UnregisterCallback<PointerDownEvent>(OnRotatePointerDown);
            base.Dispose();
        }

        private void OnRotatePointerDown(PointerDownEvent evt)
        {
            if (evt.pointerId == 1) // Мне нужно второе касание под id 1 и только так
            {
                RotateClicked();
            }
        }

        private void RotateClicked()
        {
            ItemsPage.CurrentDraggedItem.Rotate();
        }

        private void SetRotateButtonEnable(bool enabled)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    _bRotate.SetDisplay(enabled);
                    break;
                case RuntimePlatform.WindowsEditor:
                    _bRotate.SetDisplay(enabled);
                    break;
                case RuntimePlatform.WindowsPlayer:
                    _bRotate.SetDisplay(enabled);
                    break;
            }
        }

        private void CloseClicked()
        {
            _itemsPage.CloseAll();
        }

        public void SetItemDescription(ItemBaseDefinition itemBaseDefinition)
        {
            _itemImage.SetBackgroundImage(itemBaseDefinition.Icon);
            _lDescription.text = itemBaseDefinition.Description;
            _descriptionBackground.SetVisibility(true);
            _itemImage.SetVisibility(true);
        }

        public void DisableItemDescription()
        {
            _itemImage.SetVisibility(false);
            _descriptionBackground.SetVisibility(false);
        }

        public override void PickUp(ItemVisual storedItem)
        {
            base.PickUp(storedItem);

            if (storedItem.ItemDefinition.Dimensions.Width != storedItem.ItemDefinition.Dimensions.Height)
                SetRotateButtonEnable(true);
        }

        public override void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            base.Drop(storedItem, gridPosition);
            SetRotateButtonEnable(false);
        }
    }
}
