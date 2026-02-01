using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    public class Telegraph : VisualElement
    {
        private readonly Color _validColor = new Color(0, 1, 0, 0.5f);
        private readonly Color _invalidColor = new Color(1, 0, 0, 0.5f);
        private readonly Color _swapColor = new Color(1, 0.92f, 0.016f, 0.5f);

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

        public void SetPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
        }

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

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
    }
}