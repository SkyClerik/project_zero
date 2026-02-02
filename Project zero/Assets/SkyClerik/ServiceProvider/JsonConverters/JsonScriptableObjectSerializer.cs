using System;
using Newtonsoft.Json;
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
        private static readonly ScriptableObjectContractResolver _resolver = new ScriptableObjectContractResolver();

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
    }
}
