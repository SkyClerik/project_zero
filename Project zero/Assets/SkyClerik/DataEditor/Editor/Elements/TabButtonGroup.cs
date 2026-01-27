using System;
using System.Collections.Generic;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace UnityEngine.DataEditor
{
    public struct Border4
    {
        public byte Left;
        public byte Right;
        public byte Top;
        public byte Bottom;

        public Border4(byte l, byte r, byte t, byte b)
        {
            Left = l;
            Right = r;
            Top = t;
            Bottom = b;
        }
    }

    // Универсальный компонент VisualElement для управления группой выбираемых кнопок (вкладок).
    public class TabButtonGroup : VisualElement
    {
        private List<Button> _buttons;
        private Button _selectedButton;
        private Color _highlightColor;
        private Border4 _border4;

        public event Action<Button> OnSelectedButtonChanged;

        public TabButtonGroup(List<Button> buttons, Color highlightColor, Border4 border4, FlexDirection defaultDirection = FlexDirection.Row)
        {
            _buttons = buttons;
            _highlightColor = highlightColor;
            _border4 = border4;
            style.flexDirection = defaultDirection;

            foreach (var button in _buttons)
            {
                var currentButton = button; // Захватываем для лямбда-выражения
                currentButton.clicked += () => SetSelected(currentButton);
                Add(currentButton);
            }
        }

        public void SetSelected(Button buttonToSelect)
        {
            if (_selectedButton == buttonToSelect) 
                return; 

            if (_selectedButton != null)
                SetBorder(_selectedButton, Color.clear, new Border4(0, 0, 0, 0));

            _selectedButton = buttonToSelect;

            if (_selectedButton != null)
                SetBorder(_selectedButton, _highlightColor, _border4);

            OnSelectedButtonChanged?.Invoke(_selectedButton);
        }

        private void SetBorder(Button button, Color color, Border4 border4)
        {
            button.SetBorderColor(Color.clear);
            button.SetBorderWidth(0);

            if (border4.Left > 0)
            {
                button.style.borderLeftColor = color;
                button.style.borderLeftWidth = border4.Left;
            }
            if (border4.Right > 0)
            {
                button.style.borderRightColor = color;
                button.style.borderRightWidth = border4.Right;
            }
            if (border4.Top > 0)
            {
                button.style.borderTopColor = color;
                button.style.borderTopWidth = border4.Top;
            }
            if (border4.Bottom > 0)
            {
                button.style.borderBottomColor = color;
                button.style.borderBottomWidth = border4.Bottom;
            }

        }
    }
}

