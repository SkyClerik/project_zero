using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class LogicalGridVisualizer : VisualElement
    {
        private ItemContainer _itemContainer;
        private const int _minVisualCallSize = 20;

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                style.display = _isEnabled ? DisplayStyle.Flex : DisplayStyle.None;
                if (_isEnabled && _itemContainer != null) UpdateVisualizer();
            }
        }

        public LogicalGridVisualizer()
        {
            name = "LogicalGridVisualizer";
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.width = Length.Percent(100);
            style.height = Length.Percent(100);
            style.overflow = Overflow.Hidden;
            style.display = DisplayStyle.Flex;
        }

        public void Init(ItemContainer itemContainer)
        {
            if (_itemContainer != null)
                _itemContainer.OnGridOccupancyChanged -= UpdateVisualizer;

            _itemContainer = itemContainer;
            if (_itemContainer != null)
                _itemContainer.OnGridOccupancyChanged += UpdateVisualizer;
        }

        private void UpdateVisualizer()
        {
            if (!_isEnabled)
            {
                Clear();
                return;
            }
            Clear();

            if (_itemContainer == null || _itemContainer.GridDimensions.x <= 0 || _itemContainer.GridDimensions.y <= 0)
                return;

            Vector2 cellSizePx = _itemContainer.CellSize;
            Rect gridWorldRect = _itemContainer.GridWorldRect;
            bool[,] occupancy = _itemContainer.GetGridOccupancy;

            for (int y = 0; y < _itemContainer.GridDimensions.y; y++)
            {
                for (int x = 0; x < _itemContainer.GridDimensions.x; x++)
                {
                    var cellVisual = new VisualElement();
                    cellVisual.name = $"Cell_{x}_{y}";
                    cellVisual.style.position = Position.Absolute;
                    cellVisual.pickingMode = PickingMode.Ignore;

                    float actualCellWidth = Mathf.Max(cellSizePx.x, _minVisualCallSize);
                    float actualCellHeight = Mathf.Max(cellSizePx.y, _minVisualCallSize);

                    cellVisual.style.width = actualCellWidth;
                    cellVisual.style.height = actualCellHeight;

                    cellVisual.style.left = gridWorldRect.x + x * cellSizePx.x;
                    cellVisual.style.top = gridWorldRect.y + y * cellSizePx.y;


                    if (x < occupancy.GetLength(0) && y < occupancy.GetLength(1) && occupancy[x, y])
                        cellVisual.style.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.3f);
                    else
                        cellVisual.style.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.3f);

                    cellVisual.SetBorderColor(Color.black);
                    cellVisual.SetBorderWidth(1);

                    Add(cellVisual);
                }
            }
        }
    }
}
