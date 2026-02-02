using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        /// Использует сериализацию/десериализацию Newtonsoft.Json для глубокого копирования только помеченных полей.
        /// </summary>
        /// <typeparam name="T">Тип объекта ScriptableObject.</typeparam>
        /// <param name="source">Исходный объект, из которого копируются данные.</param>
        /// <param name="destination">Целевой объект, в который копируются данные.</param>
        public static void CopyJsonProperties<T>(T source, T destination) where T : ScriptableObject
        {
            if (source == null || destination == null)
            {
                Debug.LogWarning("Попытка скопировать свойства между null объектами.");
                return;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                // Используем TypeNameHandling.Objects, чтобы корректно копировать полиморфные типы, если они присутствуют в полях
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = _resolver,
                // Игнорируем ReferenceLoopHandling, так как мы копируем свойства, а не ссылки
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Сериализуем исходный объект в JSON
            string json = JsonConvert.SerializeObject(source, settings);

            // Десериализуем JSON поверх целевого объекта
            // Это обновит только те поля, которые присутствуют в JSON (т.е., помечены [JsonProperty])
            JsonConvert.PopulateObject(json, destination, settings);
        }
    }
}
