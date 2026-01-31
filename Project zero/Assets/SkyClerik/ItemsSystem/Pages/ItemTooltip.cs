using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
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

        public ItemTooltip()
        {
            style.position = Position.Absolute;
            style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = Color.gray;
            style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = 1;
            style.paddingTop = style.paddingBottom = style.paddingLeft = style.paddingRight = 5;
            style.width = 250; // Фиксированная ширина тултипа

            // Скрываем по умолчанию
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
            _nameLabel.style.whiteSpace = WhiteSpace.Normal; // Перенос текста
            _nameLabel.style.color = Color.white;
            _nameLabel.style.marginBottom = 3;
            Add(_nameLabel);

            // Описание предмета
            _descriptionLabel = new Label { name = _descriptionLabelName };
            _descriptionLabel.style.whiteSpace = WhiteSpace.Normal; // Перенос текста
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

        public void ShowTooltip(ItemBaseDefinition item, Vector2 mousePosition)
        {
            if (item == null)
            {
                HideTooltip();
                return;
            }

            _icon.image = item.Icon?.texture; // Если иконка - спрайт, то item.Icon.texture
            _nameLabel.text = item.DefinitionName;
            _descriptionLabel.text = item.Description;
            _priceLabel.text = $"Цена: {item.Price} Золота"; // Предполагаем, что Price - это int

            style.left = mousePosition.x;
            style.top = mousePosition.y;
            //style.top = Screen.height - mousePosition.y;

            // Убеждаемся, что тултип не выходит за пределы экрана
            // (простая проверка, можно усложнить)
            if (style.left.value.value + style.width.value.value > Screen.width)
            {
                style.left = Screen.width - style.width.value.value - 10;
            }
            if (style.top.value.value + style.height.value.value > Screen.height)
            {
                style.top = Screen.height - style.height.value.value - 10;
            }

            style.display = DisplayStyle.Flex;
        }

        public void HideTooltip()
        {
            style.display = DisplayStyle.None;
        }
    }
}