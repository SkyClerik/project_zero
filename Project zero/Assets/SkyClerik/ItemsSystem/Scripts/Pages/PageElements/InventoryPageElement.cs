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
            _itemImage = _root.Q(_itemImageID);
            _lDescription = _root.Q<Label>(_lDescriptionID);

            _bClose.clicked += CloseClicked;
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
    }
}
