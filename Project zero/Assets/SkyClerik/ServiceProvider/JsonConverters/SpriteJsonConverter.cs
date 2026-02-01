using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkyClerik.Inventory
{
    public class SpriteJsonConverter : JsonConverter<Sprite>
    {
        // Временный флаг для выбора способа загрузки. В реальном проекте это может быть настройка игры.
        // false = Resources.Load (Path: Resources/...)
        // true = Addressables (GUID: ...)
        public static bool UseAddressables = false; 

        public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (UseAddressables)
            {
                string identifierToSave = "";
#if UNITY_EDITOR
                // В Editor используем AssetPath как Addressable Key, как было договорено
                identifierToSave = AssetDatabase.GetAssetPath(value);
#else
                // В билде AssetDatabase недоступен.
                // Здесь мы полагаемся на то, что Addressable Key для спрайта - это его Asset Path.
                // Если спрайт был загружен через Addressables, можно попробовать value.name,
                // но Asset Path более универсален, если он использовался как ключ.
                // Для надежной работы в билде, Addressable Key для спрайта должен быть его Asset Path.
                // Или нужно иметь ResourcePathManager для получения Addressable Key по Sprite.
                // Временно используем name как заглушку, но это крайне ненадежно,
                // если Addressable Key не равен имени.
                Debug.LogError($"[SpriteJsonConverter] В билде невозможно надежно получить Asset Path для Addressable Sprite '{value.name}' без AssetDatabase. Убедитесь, что Addressable Key для всех сохраняемых спрайтов установлен на их Asset Path (путь к файлу). Если это не так, загрузка в билде будет некорректной. Либо используйте AssetReferenceSprite.");
                identifierToSave = value.name; // Fallback, крайне ненадежный.
#endif
                writer.WriteValue(identifierToSave);
            }
            else // Используем Resources.Load
            {
#if UNITY_EDITOR
                string assetPath = AssetDatabase.GetAssetPath(value);
                if (assetPath.Contains("/Resources/"))
                {
                    string resourcePath = assetPath.Substring(assetPath.IndexOf("/Resources/") + "/Resources/".Length);
                    resourcePath = Path.ChangeExtension(resourcePath, null); // Удаляем расширение файла
                    writer.WriteValue(resourcePath);
                }
                else
                {
                    Debug.LogWarning($"[SpriteJsonConverter] Спрайт '{value.name}' находится не в папке Resources. Resources.Load не сможет его загрузить.");
                    writer.WriteNull(); // Не можем сохранить, так как не в Resources
                }
#else
                // В билде у нас нет AssetDatabase, полагаемся на то, что путь до Resources правильный
                // Если Sprite создан динамически, его нельзя будет загрузить через Resources.Load
                writer.WriteValue(value.name); // В реальном проекте здесь должен быть путь, а не имя.
                Debug.LogWarning($"[SpriteJsonConverter] В билде используем имя спрайта '{value.name}' для Resources.Load. Это может быть ненадежно. Рекомендуется сохранять корректный Resource Path.");
#endif
            }
        }

        public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            string identifier = reader.Value?.ToString();
            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }

            if (UseAddressables)
            {
                // Загрузка через Addressables
                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(identifier);
                handle.WaitForCompletion(); // Ожидаем завершения загрузки
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }
                else
                {
                    Debug.LogError($"[SpriteJsonConverter] Не удалось загрузить спрайт через Addressables с идентификатором '{identifier}': {handle.OperationException?.Message}");
                    return null;
                }
            }
            else // Загрузка через Resources.Load
            {
                // Предполагаем, что identifier - это путь относительно папки Resources
                Sprite sprite = Resources.Load<Sprite>(identifier);
                if (sprite == null)
                {
                    Debug.LogError($"[SpriteJsonConverter] Не удалось загрузить спрайт через Resources.Load по пути '{identifier}'. Убедитесь, что спрайт находится в папке Resources.");
                }
                return sprite;
            }
        }
    }
}