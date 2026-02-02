using System.Linq;

namespace UnityEngine.Toolbox
{
    public static partial class UtilsExt
    {
        /// <summary>
        /// Проверяет, является ли *хотя бы один* элемент в массиве `null`.
        /// </summary>
        /// <remarks>
        /// Если массив `array` сам является `null`, метод возвращает `true`.
        /// </remarks>
        /// <typeparam name="T">Тип элементов в массиве.</typeparam>
        /// <param name="array">Массив для проверки.</param>
        /// <returns>Возвращает `true`, если массив `null` или содержит хотя бы один `null` элемент; иначе `false`.</returns>
        public static bool IsNullAny<T>(params T[] array)
        {
            if (array == null)
                return true;

            return array.Any(value => value == null);
        }

        /// <summary>
        /// Меняет местами значения двух переменных с помощью временной переменной.
        /// </summary>
        /// <typeparam name="T">Тип переменных.</typeparam>
        /// <param name="lhs">Первая переменная (левая сторона).</param>
        /// <param name="rhs">Вторая переменная (правая сторона).</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Creates and returns a clone of any given scriptable object.
        /// </summary>
        public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
        {
            if (scriptableObject == null)
            {
                Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
                return (T)ScriptableObject.CreateInstance(typeof(T));
            }

            T instance = Object.Instantiate(scriptableObject);
            instance.name = scriptableObject.name; // remove (Clone) from name
            return instance;
        }
    }
}
