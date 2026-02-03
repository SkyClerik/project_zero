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
        private VisualElement _header;
        private const string _headerID = "header";
        private Label _title;
        private const string _titleID = "l_title";
        private VisualElement _body;
        private const string _bodyID = "body";

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InventoryPageElement"/>.
        /// </summary>
        /// <param name="itemsPage">Ссылка на главную страницу предметов.</param>
        /// <param name="document">UIDocument, содержащий корневой визуальный элемент.</param>
        /// <param name="itemContainer">Контейнер предметов, связанный с этой страницей инвентаря.</param>
        public InventoryPageElement(ItemsPage itemsPage, UIDocument document, ItemContainer itemContainer)
            : base(itemsPage, document, itemContainer, itemContainer.RootPanelName)
        {
            _header = _root.Q(_headerID);
            _title = _header.Q<Label>(_titleID);
            _body = _root.Q(_bodyID);

            _title.text = _titleText;
        }
    }
}
