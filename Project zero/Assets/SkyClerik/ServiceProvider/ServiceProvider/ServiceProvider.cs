using System;
using System.Collections.Generic;

namespace UnityEngine.Toolbox
{
    /// <summary>
    /// Простой статический Service Locator для регистрации и получения сервисов по их типу.
    /// </summary>
    public static class ServiceProvider
    {
        private static bool _isShuttingDown = false;
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
        /// <returns>Возвращает реализацию сервиса, если она найдена; иначе <c>default</c> (null для ссылочных типов).</returns>
        public static T Get<T>()
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                return (T)service;
            }

            // Проверяем наш собственный флаг завершения работы
            if (!_isShuttingDown)
            {
                Debug.LogError($"[ServiceProvider] Сервис типа '{serviceType.Name}' не найден.");
            }
            return default;
        }

        /// <summary>
        /// Отменяет регистрацию сервиса по его типу.
        /// Полезно вызывать в OnDestroy() для сервисов, которые являются MonoBehaviour.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса сервиса, регистрацию которого нужно отменить.</typeparam>
        public static void Unregister<T>()
        {
            var serviceType = typeof(T);
            if (!_services.Remove(serviceType))
            {
                // Выводим предупреждение только если сервис не был найден И словарь не пуст.
                // Если словарь пуст, это, скорее всего, означает, что ServiceProviderCleaner уже очистил все.
                if (_services.Count > 0)
                {
                    Debug.LogWarning($"[ServiceProvider] Попытка отменить регистрацию для незарегистрированного сервиса типа '{serviceType.Name}'.");
                }
            }
        }

        /// <summary>
        /// Отменяет регистрацию конкретного экземпляра сервиса.
        /// Этот метод ищет все регистрации, которые ссылаются на данный экземпляр сервиса, и удаляет их.
        /// Полезно, когда один и тот же объект может быть зарегистрирован под разными типами/интерфейсами.
        /// </summary>
        /// <typeparam name="T">Тип экземпляра сервиса.</typeparam>
        /// <param name="serviceInstance">Конкретный экземпляр сервиса для отмены регистрации.</param>
        public static void Unregister<T>(T serviceInstance) where T : class
        {
            if (serviceInstance == null)
            {
                Debug.LogWarning("[ServiceProvider] Попытка отменить регистрацию для null-экземпляра сервиса.");
                return;
            }

            List<Type> keysToRemove = new List<Type>();

            foreach (var entry in _services)
            {
                if (ReferenceEquals(entry.Value, serviceInstance))
                    keysToRemove.Add(entry.Key);
            }

            foreach (Type key in keysToRemove)
            {
                _services.Remove(key);
                Debug.Log($"[ServiceProvider] Отменена регистрация сервиса '{serviceInstance.GetType().Name}' под типом '{key.Name}'.");
            }

            if (keysToRemove.Count == 0)
            {
                if (_services.Count > 0)
                {
                    Debug.LogWarning($"[ServiceProvider] Экземпляр сервиса '{serviceInstance.GetType().Name}' не был найден ни под одним зарегистрированным типом.");
                }
            }
        }

        /// <summary>
        /// Полностью очищает все зарегистрированные сервисы.
        /// Вызывается автоматически при выходе из режима PlayMode в редакторе.
        /// </summary>
        public static void ClearAllServices()
        {
            _isShuttingDown = true; // Устанавливаем флаг, что началось завершение работы
            _services.Clear();
            Debug.Log("[ServiceProvider] Все сервисы были сброшены.");
        }
    }
}
