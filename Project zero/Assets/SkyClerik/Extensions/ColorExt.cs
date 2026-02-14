namespace UnityEngine.Toolbox
{
    public static partial class ColorExt
    {
        /// <summary>
        /// Возвращает случайный цвет с полной непрозрачностью (альфа = 1).
        /// </summary>
        public static Color GetRandomColor
        {
            get
            {
                float r = Random.Range(0f, 1f);
                float g = Random.Range(0f, 1f);
                float b = Random.Range(0f, 1f);
                return new Color(r, g, b, 1);
            }
        }

        /// <summary>
        /// Возвращает полностью прозрачный белый цвет (RGBA: 1, 1, 1, 0).
        /// </summary>
        /// <returns>Полностью прозрачный белый цвет.</returns>
        public static Color GetColorTransparent()
        {
            return new Color(1f, 1f, 1f, 0f);
        }

        /// <summary>
        /// Устанавливает и возвращает копию указанного цвета с заданным уровнем прозрачности.
        /// </summary>
        /// <param name="color">Исходный цвет.</param>
        /// <param name="alpha">Целевой уровень альфа-канала (прозрачности) от 0 (полностью прозрачный) до 1 (полностью непрозрачный).</param>
        /// <returns>Копия исходного цвета с измененным альфа-каналом.</returns>
        public static Color GetColorTransparent(Color color, float alpha = 0)
        {
            color.a = alpha;
            return color;
        }
    }
}
