using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox; // Для расширения SetBorderColor, SetBorderWidth

namespace SkyClerik.Inventory
{
    public class LogicalGridVisualizer : VisualElement
    {
        private ItemContainer _itemContainer;
        private const int MIN_VISUAL_CELL_SIZE = 20;

        private bool _isEnabled = true; // По умолчанию включен
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                style.display = _isEnabled ? DisplayStyle.Flex : DisplayStyle.None; // Скрываем или показываем
                if (_isEnabled && _itemContainer != null) UpdateVisualizer(); // Обновляем, если включили
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
            style.display = DisplayStyle.Flex; // Изначально показываем, если IsEnabled по умолчанию true
        }

        public void Init(ItemContainer itemContainer)
        {
            if (_itemContainer != null)
            {
                _itemContainer.OnGridOccupancyChanged -= UpdateVisualizer;
            }
            _itemContainer = itemContainer;
            if (_itemContainer != null)
            {
                _itemContainer.OnGridOccupancyChanged += UpdateVisualizer;
            }
            // UpdateVisualizer(); // Этот вызов теперь происходит через сеттер IsEnabled, если _isEnabled = true
            // Так как IsEnabled по умолчанию true, то сеттер вызовет UpdateVisualizer
            // Если IsEnabled будет установлен в false сразу после Init, то UpdateVisualizer не будет вызван.
        }

        private void UpdateVisualizer()
        {
            if (!_isEnabled)
            {
                Clear(); // Если отключен, очищаем все, чтобы не занимать ресурсы
                return;
            }
            Clear(); // Очищаем старые квадратики
            
            if (_itemContainer == null || _itemContainer.GridDimensions.x <= 0 || _itemContainer.GridDimensions.y <= 0)
            {
                return;
            }

            Vector2 cellSizePx = _itemContainer.CellSize;
            Rect gridWorldRect = _itemContainer.GridWorldRect;
            bool[,] occupancy = _itemContainer.GetGridOccupancy();

            for (int y = 0; y < _itemContainer.GridDimensions.y; y++)
            {
                for (int x = 0; x < _itemContainer.GridDimensions.x; x++)
                {
                    var cellVisual = new VisualElement();
                    cellVisual.name = $"Cell_{x}_{y}";
                    cellVisual.style.position = Position.Absolute;
                    cellVisual.pickingMode = PickingMode.Ignore;
                    
                    float actualCellWidth = Mathf.Max(cellSizePx.x, MIN_VISUAL_CELL_SIZE);
                    float actualCellHeight = Mathf.Max(cellSizePx.y, MIN_VISUAL_CELL_SIZE);

                    cellVisual.style.width = actualCellWidth;
                    cellVisual.style.height = actualCellHeight;
                    
                    cellVisual.style.left = gridWorldRect.x + x * cellSizePx.x;
                    cellVisual.style.top = gridWorldRect.y + y * cellSizePx.y;


                    if (x < occupancy.GetLength(0) && y < occupancy.GetLength(1) && occupancy[x, y])
                    {
                        cellVisual.style.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.3f);
                    }
                    else
                    {
                        cellVisual.style.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.3f);
                    }
                    cellVisual.SetBorderColor(Color.black);
                    cellVisual.SetBorderWidth(1);

                    Add(cellVisual);
                }
            }
        }
    }
}
