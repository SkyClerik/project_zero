using System;
using System.Collections.Generic;

namespace UnityEngine.Toolbox
{
    /// <summary>
    /// Простой статический Service Locator для регистрации и получения сервисов по их типу.
    /// </summary>
    public static class ServiceProvider
    {
        // Словарь для хранения всех зарегистрированных сервисов.
        // Ключ - это тип сервиса (интерфейс), Значение - его реализация (объект).
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Регистрирует экземпляр сервиса в локаторе.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса сервиса.</typeparam>
        /// <param name="service">Конкретная реализация сервиса.</param>
        public static void Register<T>(T service)
        {
            var serviceType = typeof(T);
            if (_services.ContainsKey(serviceType))
            {
                // Если сервис уже есть, лучше предупредить об этом, чтобы избежать неожиданного поведения.
                Debug.LogWarning($"[ServiceProvider] Сервис типа '{serviceType.Name}' уже зарегистрирован. Предыдущий экземпляр будет перезаписан.");
                _services[serviceType] = service;
            }
            else
            {
                _services.Add(serviceType, service);
            }
        }

        /// <summary>
        /// Получает зарегистрированный сервис.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемого интерфейса сервиса.</typeparam>
        /// <returns>Возвращает реализацию сервиса, если она найдена.</returns>
        public static T Get<T>()
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                return (T)service;
            }

            // Если сервис не найден, возвращаем null и выводим ошибку.
            // Можно было бы бросить исключение, но возврат null более безопасен в Unity.
            Debug.LogError($"[ServiceProvider] Сервис типа '{serviceType.Name}' не найден.");
            return default; // default для обобщенного типа T будет null для ссылочных типов.
        }

        /// <summary>
        /// Отменяет регистрацию сервиса.
        /// Полезно вызывать в OnDestroy() для сервисов, которые являются MonoBehaviour.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса сервиса, регистрацию которого нужно отменить.</typeparam>
        public static void Unregister<T>()
        {
            var serviceType = typeof(T);
            if (!_services.Remove(serviceType))
            {
                // Это не критичная ошибка, но может помочь в отладке.
                Debug.LogWarning($"[ServiceProvider] Попытка отменить регистрацию для незарегистрированного сервиса типа '{serviceType.Name}'.");
            }
        }

        /// <summary>
        /// Полностью очищает все зарегистрированные сервисы.
        /// Вызывается автоматически при выходе из режима PlayMode в редакторе.
        /// </summary>
        public static void ClearAllServices()
        {
            _services.Clear();
            Debug.Log("[ServiceProvider] Все сервисы были сброшены.");
        }
    }
}
