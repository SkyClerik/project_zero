using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой визуальный элемент для отображения всплывающей подсказки о предмете.
    /// Содержит иконку, название, описание и цену предмета.
    /// </summary>
    public class ItemTooltip : VisualElement
    {
        private Image _icon;
        private Label _nameLabel;
        private Label _descriptionLabel;
        private Label _priceLabel;

        private const string _iconName = "item-icon";
        private const string _nameLabelName = "item-name";
        private const string _descriptionLabelName = "item-description";
        private const string _priceLabelName = "item-price";

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemTooltip"/>.
        /// Устанавливает базовые стили и создает дочерние элементы для отображения информации о предмете.
        /// </summary>
        public ItemTooltip()
        {
            style.position = Position.Absolute;
            style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = Color.gray;
            style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = 1;
            style.paddingTop = style.paddingBottom = style.paddingLeft = style.paddingRight = 5;
            style.width = 250;

            this.SetDisplay(false);

            // Иконка
            _icon = new Image { name = _iconName };
            _icon.style.width = 64;
            _icon.style.height = 64;
            _icon.style.marginBottom = 5;
            Add(_icon);

            // Название предмета
            _nameLabel = new Label { name = _nameLabelName };
            _nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _nameLabel.style.whiteSpace = WhiteSpace.Normal;
            _nameLabel.style.color = Color.white;
            _nameLabel.style.marginBottom = 3;
            Add(_nameLabel);

            // Описание предмета
            _descriptionLabel = new Label { name = _descriptionLabelName };
            _descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            _descriptionLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            _descriptionLabel.style.fontSize = 12;
            _descriptionLabel.style.marginBottom = 3;
            Add(_descriptionLabel);

            // Цена предмета
            _priceLabel = new Label { name = _priceLabelName };
            _priceLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _priceLabel.style.color = new Color(1f, 0.8f, 0f); // Золотой цвет
            Add(_priceLabel);
        }

        /// <summary>
        /// Отображает всплывающую подсказку с информацией о предмете.
        /// </summary>
        /// <param name="item">Предмет, информацию о котором нужно отобразить.</param>
        /// <param name="mousePosition">Позиция мыши, относительно которой будет показана подсказка.</param>
        public void ShowTooltip(ItemBaseDefinition item, Vector2 mousePosition)
        {
            if (item == null)
            {
                HideTooltip();
                return;
            }

            _icon.image = item.Icon?.texture;
            _nameLabel.text = item.DefinitionName;
            _descriptionLabel.text = item.Description;
            _priceLabel.text = $"Цена: {item.Price} Золота";

            style.left = mousePosition.x;
            style.top = mousePosition.y;

            // Убеждаемся, что тултип не выходит за пределы экрана
            if (style.left.value.value + style.width.value.value > Screen.width)
                style.left = Screen.width - style.width.value.value - 10;
            if (style.top.value.value + style.height.value.value > Screen.height)
                style.top = Screen.height - style.height.value.value - 10;

            style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Скрывает всплывающую подсказку.
        /// </summary>
        public void HideTooltip()
        {
            style.display = DisplayStyle.None;
        }
    }
}