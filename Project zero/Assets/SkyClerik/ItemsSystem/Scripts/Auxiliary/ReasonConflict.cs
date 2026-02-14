namespace SkyClerik.Inventory
{
    /// <summary>
    /// Перечисление, определяющее возможные причины конфликтов при размещении предмета в сетке.
    /// </summary>
    public enum ReasonConflict : byte
    {
        /// <summary>
        /// Нет конфликта, размещение возможно.
        /// </summary>
        None = 0,
        /// <summary>
        /// Предмет выходит за границы сетки.
        /// </summary>
        beyondTheGridBoundary = 1,
        /// <summary>
        /// Предмет пересекается с другими объектами в сетке.
        /// </summary>
        intersectsObjects = 2,
        /// <summary>
        /// Тип слота не подходит для размещения предмета.
        /// </summary>
        invalidSlotType = 3,
        /// <summary>
        /// Возможность обмена предмета.
        /// </summary>
        SwapAvailable = 4,
        /// <summary>
        /// Возможность объединения предмета со стаком.
        /// </summary>
        StackAvailable = 5,
    }
}