using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.Toolbox
{
    public static partial class LayerMaskExt
    {
        /// <summary>
        /// Проверяет, содержит ли LayerMask указанный слой.
        /// </summary>
        /// <param name="mask">Исходный LayerMask.</param>
        /// <param name="layer">Номер слоя для проверки (0-31).</param>
        /// <returns>Возвращает `true`, если LayerMask содержит слой, иначе `false`.</returns>
        public static bool Contain(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        /// <summary>
        /// Инвертирует LayerMask, возвращая все слои, которые не были включены в исходную маску.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <returns>Инвертированный LayerMask.</returns>
        public static LayerMask Inverse(this LayerMask original)
        {
            return ~original;
        }

        /// <summary>
        /// Добавляет один или несколько слоев по их именам к существующему LayerMask.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <param name="layerNames">Массив имен слоев для добавления.</param>
        /// <returns>Новый LayerMask с добавленными слоями.</returns>
        public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
        {
            return original | NamesToMask(layerNames);
        }

        /// <summary>
        /// Удаляет один или несколько слоев по их именам из существующего LayerMask.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <param name="layerNames">Массив имен слоев для удаления.</param>
        /// <returns>Новый LayerMask без указанных слоев.</returns>
        public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
        {
            LayerMask invertedOriginal = ~original;
            return ~(invertedOriginal | NamesToMask(layerNames));
        }

        /// <summary>
        /// Удаляет один или несколько слоев по их номерам из существующего LayerMask.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <param name="layers">Массив номеров слоев для удаления.</param>
        /// <returns>Новый LayerMask без указанных слоев.</returns>
        public static LayerMask RemoveFromMask(this LayerMask original, params int[] layers)
        {
            LayerMask invertedOriginal = ~original;
            return ~(invertedOriginal | LayerNumbersToMask(layers));
        }

        /// <summary>
        /// Удаляет слои, содержащиеся в другом LayerMask, из исходной маски.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <param name="layerMask">LayerMask, слои которого нужно удалить из исходной маски.</param>
        /// <returns>Новый LayerMask без слоев из `layerMask`.</returns>
        public static LayerMask RemoveFromMask(this LayerMask original, LayerMask layerMask)
        {
            return original & ~layerMask;
        }

        /// <summary>
        /// Преобразует LayerMask в массив имен слоев, которые в него входят.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <returns>Массив строковых имен слоев.</returns>
        public static string[] MaskToNames(this LayerMask original)
        {
            var output = new List<string>();

            for (int i = 0; i < 32; ++i)
            {
                int shifted = 1 << i;
                if ((original.value & shifted) == shifted) // Исправлено: original.value
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        output.Add(layerName);
                    }
                }
            }
            return output.ToArray();
        }

        /// <summary>
        /// Преобразует LayerMask в строку, содержащую имена слоев, разделенных запятыми.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <returns>Строка с именами слоев, разделенными запятыми.</returns>
        public static string MaskToString(this LayerMask original)
        {
            return MaskToString(original, ", ");
        }

        /// <summary>
        /// Преобразует LayerMask в строку, содержащую имена слоев, разделенных указанным разделителем.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>
        /// <param name="delimiter">Строка-разделитель для имен слоев.</param>
        /// <returns>Строка с именами слоев, разделенными указанным разделителем.</returns>
        public static string MaskToString(this LayerMask original, string delimiter)
        {
            return string.Join(delimiter, MaskToNames(original));
        }

        /// <summary>
        /// Конвертирует LayerMask в строковое представление в двоичном формате.
        /// </summary>
        /// <param name="original">Исходный LayerMask.</param>

        /// <returns>Строка, представляющая LayerMask в двоичном формате.</returns>
        public static string BinaryFormat(this LayerMask original)
        {
            return System.Convert.ToString(original.value, 2);
        }

        #region LayerMaskExt static methods

        /// <summary>
        /// Создает LayerMask из массива имен слоев.
        /// </summary>
        /// <param name="layerNames">Массив имен слоев.</param>
        /// <returns>Новый LayerMask, содержащий указанные слои.</returns>
        public static LayerMask Create(params string[] layerNames)
        {
            return NamesToMask(layerNames);
        }

        /// <summary>
        /// Создает LayerMask из массива номеров слоев.
        /// </summary>
        /// <param name="layerNumbers">Массив номеров слоев.</param>
        /// <returns>Новый LayerMask, содержащий указанные слои.</returns>
        public static LayerMask Create(params int[] layerNumbers)
        {
            return LayerNumbersToMask(layerNumbers);
        }

        /// <summary>
        /// Конвертирует массив имен слоев в LayerMask.
        /// </summary>
        /// <param name="layerNames">Массив имен слоев.</param>
        /// <returns>LayerMask, соответствующий указанным именам слоев.</returns>
        public static LayerMask NamesToMask(params string[] layerNames)
        {
            LayerMask ret = (LayerMask)0;
            foreach (var name in layerNames)
            {
                int layer = LayerMask.NameToLayer(name);
                if (layer != -1) // Проверяем, что слой существует
                {
                    ret |= (1 << layer);
                }
                else
                {
                    Debug.LogWarning($"Layer '{name}' not found. Skipping.");
                }
            }
            return ret;
        }

        /// <summary>
        /// Конвертирует массив номеров слоев в LayerMask.
        /// </summary>
        /// <param name="layerNumbers">Массив номеров слоев.</param>
        /// <returns>LayerMask, соответствующий указанным номерам слоев.</returns>
        public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
        {
            LayerMask ret = (LayerMask)0;
            foreach (var layer in layerNumbers)
            {
                if (layer >= 0 && layer < 32) // Проверяем, что номер слоя корректен
                {
                    ret |= (1 << layer);
                }
                else
                {
                    Debug.LogWarning($"Layer number '{layer}' is out of range (0-31). Skipping.");
                }
            }
            return ret;
        }
        #endregion
    }
}
