using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Inventory
{
    public class Telegraph : VisualElement
    {
        private readonly Color _validColor = new Color(0, 1, 0, 0.5f);
        private readonly Color _invalidColor = new Color(1, 0, 0, 0.5f);

        public Telegraph()
        {
            this.pickingMode = PickingMode.Ignore;
            this.style.position = Position.Absolute;
            this.style.backgroundColor = _validColor;
        }

        public void SetPlacement(bool isValid)
        {
            style.backgroundColor = isValid ? _validColor : _invalidColor;
            style.visibility = Visibility.Visible;
        }

        public void Hide()
        {
            style.visibility = Visibility.Hidden;
        }
    }
}