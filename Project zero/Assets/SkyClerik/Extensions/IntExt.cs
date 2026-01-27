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
    }
}
