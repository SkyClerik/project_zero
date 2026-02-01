//using System;
//using System.IO;
//using Newtonsoft.Json;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;

//namespace SkyClerik.Inventory
//{
//    public class SpriteJsonConverter : JsonConverter<Sprite>
//    {
//        // Временный флаг для выбора способа загрузки. В реальном проекте это может быть настройка игры.
//        // false = Resources.Load (Path: Resources/...)
//        // true = Addressables (GUID: ...)
//        public static bool UseAddressables = false; 

//        public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
//        {
//            if (value == null)
//            {
//                writer.WriteNull();
//                return;
//            }

//            if (UseAddressables)
//            {
//                string identifierToSave = "";
//#if UNITY_EDITOR
//                // В Editor используем GUID как надежный идентификатор для Addressables
//                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(value);
//                if (!string.IsNullOrEmpty(assetPath))
//                {
//                    identifierToSave = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
//                }
//                else
//                {
//                    Debug.LogWarning($"[SpriteJsonConverter] Не удалось получить AssetPath для спрайта '{value.name}'. Невозможно сериализовать как Addressable.");
//                    writer.WriteNull();
//                    return;
//                }
//#else
//                // В билде AssetDatabase недоступен.
//                // Для надежной сериализации Addressable Assets рекомендуется использовать AssetReference.
//                // Без AssetDatabase невозможно надежно получить GUID из экземпляра Sprite.
//                // Если Sprite был загружен через Addressables, то в билде его GUID можно получить,
//                // если этот GUID явно связан со Sprite (например, через AssetReference или ScriptableObject).
//                // Без такой явной связи, сериализация Sprite в билде для Addressables невозможна.
//                Debug.LogError($"[SpriteJsonConverter] В билде невозможно надежно сериализовать Addressable Sprite '{value.name}' без AssetDatabase или явной связи с GUID. Записываем null. Используйте AssetReferenceSprite или храните GUID в ItemDefinition.");
//                writer.WriteNull();
//                return;
//#endif
//                writer.WriteValue(identifierToSave);
//            }
//            else // Используем Resources.Load
//            {
//#if UNITY_EDITOR
//                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(value);
//                if (assetPath.Contains("/Resources/"))
//                {
//                    string resourcePath = assetPath.Substring(assetPath.IndexOf("/Resources/") + "/Resources/".Length);
//                    resourcePath = Path.ChangeExtension(resourcePath, null); // Удаляем расширение файла
//                    writer.WriteValue(resourcePath);
//                }
//                else
//                {
//                    Debug.LogWarning($"[SpriteJsonConverter] Спрайт '{value.name}' находится не в папке Resources. Resources.Load не сможет его загрузить. Записываем null.");
//                    writer.WriteNull(); // Не можем сохранить, так как не в Resources
//                }
//#else
//                // В билде AssetDatabase недоступен.
//                // Без AssetDatabase невозможно надежно получить Resource Path из экземпляра Sprite.
//                // Сериализация Sprite для Resources.Load без явного пути крайне ненадежна.
//                // Для надежной сериализации следует сохранять Resource Path в ScriptableObject
//                // (например, в ItemDefinition) или использовать AssetReference.
//                Debug.LogError($"[SpriteJsonConverter] В билде невозможно надежно сериализовать Sprite '{value.name}' для Resources.Load. Требуется явное указание Resource Path. Записываем null. Используйте AssetReferenceSprite или храните Resource Path в ItemDefinition.");
//                writer.WriteNull();
//                return; // Не можем продолжить без надежного идентификатора
//#endif
//            }
//        }

//        public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite existingValue, bool hasExistingValue, JsonSerializer serializer)
//        {
//            if (reader.TokenType == JsonToken.Null)
//            {
//                return null;
//            }

//            string identifier = reader.Value?.ToString();
//            if (string.IsNullOrEmpty(identifier))
//            {
//                Debug.LogWarning($"[SpriteJsonConverter] Пустой или некорректный идентификатор при десериализации спрайта.");
//                return null;
//            }

//            if (UseAddressables)
//            {
//                // Загрузка через Addressables.
//                // Внимание: WaitForCompletion() блокирует поток и не рекомендуется для использования в продакшене.
//                // Для асинхронной загрузки используйте AssetReferenceSprite или загружайте асинхронно после десериализации.
//                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(identifier);
//                try
//                {
//                    handle.WaitForCompletion(); // Ожидаем завершения загрузки (синхронно)
//                }
//                catch (Exception e)
//                {
//                    Debug.LogError($"[SpriteJsonConverter] Исключение при синхронной загрузке Addressable Sprite '{identifier}': {e.Message}");
//                    Addressables.Release(handle); // Освобождаем хэндл в случае ошибки
//                    return null;
//                }

//                if (handle.Status == AsyncOperationStatus.Succeeded)
//                {
//                    // Важно: не вызываем Addressables.Release(handle) здесь,
//                    // так как спрайт будет использоваться. Release должен быть вызван,
//                    // когда спрайт больше не нужен.
//                    return handle.Result;
//                }
//                else
//                {
//                    Debug.LogError($"[SpriteJsonConverter] Не удалось загрузить спрайт через Addressables с идентификатором '{identifier}'. Статус: {handle.Status}. Исключение: {handle.OperationException?.Message}");
//                    Addressables.Release(handle); // Освобождаем хэндл в случае ошибки
//                    return null;
//                }
//            }
//            else // Загрузка через Resources.Load
//            {
//                Sprite sprite = Resources.Load<Sprite>(identifier);
//                if (sprite == null)
//                {
//                    Debug.LogError($"[SpriteJsonConverter] Не удалось загрузить спрайт через Resources.Load по пути '{identifier}'. Убедитесь, что спрайт находится в папке Resources.");
//                }
//                return sprite;
//            }
//        }
//    }
//}