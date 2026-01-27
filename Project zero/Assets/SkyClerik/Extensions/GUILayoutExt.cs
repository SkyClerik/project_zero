using UnityEngine;

namespace UnityEngine.Toolbox
{
    public static partial class GUILayoutExt
    {
        /// <summary>
        /// Создает кнопку GUILayout с заданным текстом и размером (ширина/высота).
        /// </summary>
        /// <param name="text">Текст, отображаемый на кнопке.</param>
        /// <param name="size">Размер кнопки (используется для ширины и высоты).</param>
        /// <returns>Возвращает `true`, если кнопка была нажата, иначе `false`.</returns>
        public static bool BoxButton(string text, float size)
        {
            if (GUILayout.Button(text, GUILayout.Width(size), GUILayout.Height(size)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Создает кнопку GUILayout с заданным изображением (текстурой) и размером (ширина/высота).
        /// </summary>
        /// <param name="texture">Текстура, отображаемая на кнопке.</param>
        /// <param name="size">Размер кнопки (используется для ширины и высоты).</param>
        /// <returns>Возвращает `true`, если кнопка была нажата, иначе `false`.</returns>
        public static bool BoxButton(Texture texture, float size)
        {
            if (GUILayout.Button(texture, GUILayout.Width(size), GUILayout.Height(size)))
            {
                return true;
            }
            return false;
        }
    }
}
