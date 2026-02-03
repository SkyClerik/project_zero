using System;

namespace SkyClerik.Data
{
    /// <summary>
    /// Представляет размер прямоугольника в целочисленных значениях высоты и ширины.
    /// </summary>
    [Serializable]
    public struct RectangleSize
    {
        /// <summary>
        /// Высота прямоугольника.
        /// </summary>
        public int height;
        /// <summary>
        /// Ширина прямоугольника.
        /// </summary>
        public int width;
    }
}