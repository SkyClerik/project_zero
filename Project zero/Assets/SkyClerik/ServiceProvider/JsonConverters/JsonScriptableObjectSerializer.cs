using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using UnityEngine;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Вспомогательный класс для сериализации и десериализации ScriptableObject
    /// с использованием Newtonsoft.Json и ScriptableObjectContractResolver.
    /// </summary>
    public static class JsonScriptableObjectSerializer
    {
        // Рекомендуется кэшировать ContractResolver для лучшей производительности.
        private static readonly DefaultContractResolver _resolver = new DefaultContractResolver();

        /// <summary>
        /// Десериализует JSON-строку в экземпляр ScriptableObject,
        /// корректно обрабатывая абстрактные классы и создание экземпляров Unity.
        /// </summary>
        /// <typeparam name="T">Тип ScriptableObject для десериализации.</typeparam>
        /// <param name="json">JSON-строка.</param>
        /// <returns>Десериализованный экземпляр ScriptableObject.</returns>
        public static T DeserializeScriptableObject<T>(string json) where T : ScriptableObject
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                // TypeNameHandling.Auto добавляет $type только если тип отличается от объявленного.
                // TypeNameHandling.Objects всегда добавляет $type.
                // Будь осторожен с безопасностью при десериализации из недоверенных источников!
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = _resolver
            };

            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        /// <summary>
        /// Сериализует экземпляр ScriptableObject в JSON-строку,
        /// включая информацию о типе для корректной десериализации абстрактных классов.
        /// </summary>
        /// <typeparam name="T">Тип ScriptableObject для сериализации.</typeparam>
        /// <param name="obj">Экземпляр ScriptableObject.</param>
        /// <returns>JSON-строка.</returns>
        public static string SerializeScriptableObject<T>(T obj) where T : ScriptableObject
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                // Для сериализации лучше использовать TypeNameHandling.Objects,
                // чтобы информация о типе всегда присутствовала для десериализации.
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = _resolver
            };

            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// Копирует значения всех полей, помеченных [JsonProperty], из исходного объекта в целевой.
        /// Использует рефлексию для прямого копирования значений полей.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="source">Исходный объект, из которого копируются данные.</param>
        /// <param name="destination">Целевой объект, в который копируются данные.</param>
        public static void CopyJsonProperties<T>(T source, T destination) where T : class
        {
            if (source == null || destination == null)
            {
                Debug.LogWarning("Попытка скопировать свойства между null объектами.");
                return;
            }

            var type = source.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // Проходим по всем полям, включая поля базовых классов
            while (type != null)
            {
                foreach (var field in type.GetFields(flags))
                {
                    // Проверяем наличие атрибута [JsonProperty]
                    if (field.GetCustomAttribute<JsonPropertyAttribute>() != null)
                    {
                        try
                        {
                            var value = field.GetValue(source);
                            field.SetValue(destination, value);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"Не удалось скопировать поле '{field.Name}' из {source.GetType().Name} в {destination.GetType().Name}. Ошибка: {ex.Message}");
                        }
                    }
                }
                // Переходим к базовому типу
                type = type.BaseType;
            }
        }
    }
}
