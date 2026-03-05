using System;

namespace UnityEngine.Toolbox
{
    public static partial class IntExt
    {
        /// <summary>
        /// Вычисляет указанный процент от целого числа.
        /// </summary>
        /// <param name="value">Исходное целое число.</param>
        /// <param name="percentage">Процент, который нужно вычислить (от 0 до 100).</param>
        /// <returns>Результат вычисления процента.</returns>
        public static int GetPercent(this int value, byte percentage)
        {
            return (value * percentage) / 100;
        }

        /// <summary>
        /// Сколько НЕ хватает до максимума.
        /// current = 2, max = 3  -> 1
        /// current = 3, max = 3  -> 0
        /// current = 5, max = 3  -> 0  (уже переполнено, не не хватает)
        /// </summary>
        public static int LackToMax(this int current, int max)
        {
            return current < max ? max - current : 0;
        }

        /// <summary>
        /// Насколько СВЕРХ максимума.
        /// current = 2, max = 3  -> 0
        /// current = 3, max = 3  -> 0
        /// current = 5, max = 3  -> 2
        /// </summary>
        public static int OverflowFromMax(this int current, int max)
        {
            return current > max ? current - max : 0;
        }

        /// <summary>
        /// Подписанная разница относительно максимума:
        /// current = 2, max = 3  ->  1  (не хватает 1)
        /// current = 3, max = 3  ->  0  (ровно)
        /// current = 5, max = 3  -> -2  (перебор на 2)
        /// </summary>
        public static int SignedDiffToMax(this int current, int max)
        {
            return max - current;
        }

        /// <summary>
        /// Абсолютная разница:
        /// current = 2, max = 3  -> 1
        /// current = 3, max = 3  -> 0
        /// current = 5, max = 3  -> 2
        /// </summary>
        public static int AbsDiffToMax(this int current, int max)
        {
            return Math.Abs(max - current);
        }
    }
}
