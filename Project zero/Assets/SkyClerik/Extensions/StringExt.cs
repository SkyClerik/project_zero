using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UnityEngine.Toolbox
{
    public static partial class StringExt
    {
        /// <summary>
        /// Проверяет, являются ли *все* переданные строки null или пустыми.
        /// </summary>
        /// <param name="array">Массив строк для проверки.</param>
        /// <returns>Возвращает `true`, если все строки в массиве `null` или пустые; иначе `false`.</returns>
        public static bool AreAllNullOrEmpty(params string[] array)
        {
            foreach (var item in array)
            {
                if (!string.IsNullOrEmpty(item))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, имеют ли *все* переданные строки значение (то есть ни одна не является null или пустой).
        /// </summary>
        /// <param name="array">Массив строк для проверки.</param>
        /// <returns>Возвращает `true`, если все строки в массиве не `null` и не пустые; иначе `false`.</returns>
        public static bool AreAllNotNullOrEmpty(params string[] array)
        {
            foreach (var item in array)
            {
                if (string.IsNullOrEmpty(item))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет наличие текущей строки в переданном наборе значений.
        /// </summary>
        /// <param name="value">Строка, наличие которой нужно проверить.</param>
        /// <param name="array">Набор строк, в котором производится поиск.</param>
        /// <returns>Возвращает `true`, если `value` найдено в `array`, иначе `false`.</returns>
        public static bool In(this string value, params string[] array)
        {
            return array.Contains(value);
        }

        /// <summary>
        /// Возвращает значение, указывающее, содержит ли данная строка *хотя бы одну* из переданных подстрок.
        /// </summary>
        /// <param name="str">Исходная строка для проверки.</param>
        /// <param name="values">Массив подстрок для поиска.</param>
        /// <returns>Возвращает `true`, если исходная строка содержит любую из подстрок, иначе `false`.</returns>
        public static bool ContainsAny(this string str, params string[] values)
        {
            return values.Any(s => str.Contains(s));
        }

        /// <summary>
        /// Форматирует целое число, добавляя пробелы в качестве разделителей тысяч.
        /// </summary>
        /// <param name="value">Исходное целое число.</param>
        /// <returns>Строка с числом, отформатированным "под цену" (например, "1 000 000").</returns>
        public static string ToPriceStyle(int value)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return value.ToString("N0", nfi);
        }

        /// <summary>
        /// Форматирует строку, предполагая, что она является числом, добавляя пробелы в качестве разделителей тысяч.
        /// </summary>
        /// <param name="str">Исходная строка, представляющая число.</param>
        /// <returns>Строка с числом, отформатированным "под цену", или исходная строка, если её не удалось преобразовать в число.</returns>
        public static string ToPriceStyle(this string str)
        {
            if (int.TryParse(str, out int value))
            {
                var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = " ";
                return value.ToString("N0", nfi);
            }
            return str;
        }

        /// <summary>
        /// Проверяет, соответствует ли строка формату электронной почты.
        /// </summary>
        /// <param name="email">Строка для проверки.</param>
        /// <returns>Возвращает `true`, если строка является валидным адресом электронной почты, иначе `false`.</returns>
        public static bool IsValidEmail(this string email)
        {
            return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        /// <summary>
        /// Преобразует объект `TimeSpan` в удобочитаемую строку, отображая дни, часы, минуты и секунды.
        /// </summary>
        /// <param name="timeSpan">Объект `TimeSpan` для преобразования.</param>
        /// <returns>Удобочитаемая строка, например "1д. 5ч. 30м. 15с.". Если `TimeSpan` меньше секунды, возвращается "0с.".</returns>
        public static string ToReadableString(this System.TimeSpan timeSpan)
        {
            string result = "";
            if (timeSpan.Days > 0)
            {
                result += $"{timeSpan.Days}д. ";
            }
            if (timeSpan.Hours > 0)
            {
                result += $"{timeSpan.Hours}ч. ";
            }
            if (timeSpan.Minutes > 0)
            {
                result += $"{timeSpan.Minutes}м. ";
            }
            if (timeSpan.Seconds > 0 || result == "") // Если нет дней, часов, минут, то показываем секунды
            {
                result += $"{timeSpan.Seconds}с.";
            }
            return result.Trim();
        }
    }
}