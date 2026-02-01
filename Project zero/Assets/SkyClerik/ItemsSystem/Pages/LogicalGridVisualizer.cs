using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox; // Для расширения SetBorderColor, SetBorderWidth

namespace SkyClerik.Inventory
{
    public class LogicalGridVisualizer : VisualElement
    {
        private ItemContainer _itemContainer;
        private const int MIN_VISUAL_CELL_SIZE = 20; // Твое пожелание, мой хороший!

        public LogicalGridVisualizer()
        {
            name = "LogicalGridVisualizer";
            pickingMode = PickingMode.Ignore; // Чтобы не мешать взаимодействию с UI
            style.position = Position.Absolute;
            style.width = Length.Percent(100); // Растягиваем на всю ширину родителя
            style.height = Length.Percent(100); // Растягиваем на всю высоту родителя
            style.overflow = Overflow.Hidden; // Обрезаем содержимое, выходящее за границы
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
            UpdateVisualizer(); // Вызываем сразу после инициализации, чтобы нарисовать текущее состояние
        }

        private void UpdateVisualizer()
        {
            Clear(); // Очищаем старые квадратики
            
            if (_itemContainer == null || _itemContainer.GridDimensions.x <= 0 || _itemContainer.GridDimensions.y <= 0)
            {
                return;
            }

            Vector2 cellSizePx = _itemContainer.CellSize;
            Rect gridWorldRect = _itemContainer.GridWorldRect;
            bool[,] occupancy = _itemContainer.GetGridOccupancy(); // Получаем состояние _gridOccupancy

            for (int y = 0; y < _itemContainer.GridDimensions.y; y++)
            {
                for (int x = 0; x < _itemContainer.GridDimensions.x; x++)
                {
                    var cellVisual = new VisualElement();
                    cellVisual.name = $"Cell_{x}_{y}";
                    cellVisual.style.position = Position.Absolute;
                    cellVisual.pickingMode = PickingMode.Ignore; // <-- Добавляем сюда!
                    
                    // Используем CellSize из ItemContainer для позиционирования и твое пожелание о минимальном размере
                    float actualCellWidth = Mathf.Max(cellSizePx.x, MIN_VISUAL_CELL_SIZE);
                    float actualCellHeight = Mathf.Max(cellSizePx.y, MIN_VISUAL_CELL_SIZE);

                    cellVisual.style.width = actualCellWidth;
                    cellVisual.style.height = actualCellHeight;
                    
                    // Позиционируем ячейки относительно gridWorldRect
                    cellVisual.style.left = gridWorldRect.x + x * cellSizePx.x;
                    cellVisual.style.top = gridWorldRect.y + y * cellSizePx.y;


                    // Если ячейка занята, красим в красный, иначе - в зеленый
                    if (x < occupancy.GetLength(0) && y < occupancy.GetLength(1) && occupancy[x, y])
                    {
                        cellVisual.style.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.3f); // Красный, полупрозрачный
                    }
                    else
                    {
                        cellVisual.style.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.3f); // Зеленый, полупрозрачный
                    }
                    cellVisual.SetBorderColor(Color.black); // Граница для каждой ячейки
                    cellVisual.SetBorderWidth(1);

                    Add(cellVisual);
                }
            }
        }
    }
}