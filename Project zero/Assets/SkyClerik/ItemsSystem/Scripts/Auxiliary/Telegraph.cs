using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Визуальный элемент, представляющий "телеграф" - индикатор возможного места размещения
    /// перетаскиваемого предмета в сетке инвентаря. Изменяет цвет в зависимости от возможности размещения.
    /// </summary>
    public class Telegraph : VisualElement
    {
        private readonly Color _validColor = new Color(0, 1, 0, 0.5f);
        private readonly Color _invalidColor = new Color(1, 0, 0, 0.5f);
        private readonly Color _swapColor = new Color(1, 0.92f, 0.016f, 0.5f);

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Telegraph"/>.
        /// Устанавливает базовые стили и скрывает телеграф по умолчанию.
        /// </summary>
        public Telegraph()
        {
            this.pickingMode = PickingMode.Ignore;
            this.style.position = Position.Absolute;
            this.style.backgroundColor = _validColor;
            this.style.width = 0;
            this.style.height = 0;
            this.name = "telegraph";
            Hide();
        }

        /// <summary>
        /// Устанавливает позицию телеграфа на UI.
        /// </summary>
        /// <param name="position">Новая позиция телеграфа (top, left).</param>
        public void SetPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
        }

        /// <summary>
        /// Устанавливает параметры отображения телеграфа в зависимости от результата размещения.
        /// Изменяет цвет и размеры телеграфа.
        /// </summary>
        /// <param name="conflict">Тип конфликта размещения.</param>
        /// <param name="width">Ширина телеграфа в пикселях.</param>
        /// <param name="height">Высота телеграфа в пикселях.</param>
        public void SetPlacement(ReasonConflict conflict, float width, float height)
        {
            switch (conflict)
            {
                case ReasonConflict.None:
                    style.backgroundColor = _validColor;
                    break;
                case ReasonConflict.StackAvailable:
                case ReasonConflict.SwapAvailable:
                    style.backgroundColor = _swapColor;
                    break;
                default:
                    style.backgroundColor = _invalidColor;
                    break;
            }
            
            this.style.width = width;
            this.style.height = height;
            style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Скрывает телеграф.
        /// </summary>
        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
    }
}