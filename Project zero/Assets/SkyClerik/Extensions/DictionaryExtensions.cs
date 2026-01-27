using System.Collections.Generic;

namespace UnityEngine.Toolbox
{
    public static partial class DictionaryExtensions
    {
        /// <summary>
        /// Перемешивает ключи словаря в случайном порядке, используя алгоритм Фишера-Йетса.
        /// </summary>
        /// <remarks>
        /// Важно: этот метод не перемешивает сам словарь, а возвращает новый массив со случайно перемешанными ключами.
        /// </remarks>
        /// <typeparam name="TKey">Тип ключей в словаре.</typeparam>
        /// <typeparam name="TValue">Тип значений в словаре.</typeparam>
        /// <param name="source">Исходный словарь, ключи которого нужно перемешать.</param>
        /// <returns>Новый массив с ключами словаря в случайном порядке.</returns>
        public static TKey[] Shuffle<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            System.Random r = new System.Random();
            TKey[] wviTKey = new TKey[source.Count];
            source.Keys.CopyTo(wviTKey, 0);

            for (int i = wviTKey.Length; i > 1; i--)
            {
                int k = r.Next(i);
                TKey temp = wviTKey[k];
                wviTKey[k] = wviTKey[i - 1];
                wviTKey[i - 1] = temp;
            }

            return wviTKey;
        }
    }
}
