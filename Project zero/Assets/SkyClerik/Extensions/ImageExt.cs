using UnityEngine;

namespace UnityEngine.Toolbox
{
    public static partial class ImageExt
    {
        /// <summary>
        /// Конвертирует Sprite в новый Texture2D.
        /// </summary>
        /// <remarks>
        /// **Важно:** Для корректной работы этого метода, у исходной текстуры Sprite должна быть включена опция 'Read/Write Enabled' в настройках импорта Unity.
        /// </remarks>
        /// <param name="sprite">Исходный Sprite для конвертации.</param>
        /// <returns>Новый Texture2D, содержащий данные Sprite.</returns>
        public static Texture2D ToTexture(this Sprite sprite)
        {
            Rect tRect = sprite.textureRect;
            Texture2D icon = new Texture2D((int)tRect.width, (int)tRect.height);
            Color[] newTexture = sprite.texture.GetPixels((int)tRect.x, (int)tRect.y, (int)tRect.width, (int)tRect.height);
            icon.SetPixels(newTexture);
            icon.Apply();
            return icon;
        }
    }
}