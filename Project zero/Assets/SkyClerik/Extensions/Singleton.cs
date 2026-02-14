namespace UnityEngine.Toolbox
{
    /// <summary>
    /// Потокобезопасная и надежная реализация паттерна 'Одиночка' (Singleton) для MonoBehaviour.
    /// Гарантирует наличие только одного экземпляра объекта и его сохранность между сценами.
    /// </summary>
    /// <typeparam name="T">Тип компонента, для которого реализуется 'Одиночка'. Должен быть наследником MonoBehaviour.</typeparam>
    /// <remarks>
    /// Для использования, унаследуйте ваш класс от `Singleton<YourClassName>`.
    /// Экземпляр будет доступен через статическое свойство `YourClassName.Instance`.
    /// </remarks>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        /// <summary>
        /// Возвращает единственный потокобезопасный экземпляр 'Одиночки'.
        /// Если экземпляр не существует, он будет найден на сцене или создан новый.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of '{typeof(T)}' is being accessed after application quit. Returning null.");
                    return null;
                }

                // Потокобезопасная блокировка на случай, если несколько потоков одновременно обратятся к инстансу
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Пытаемся найти существующий экземпляр на сцене
                        _instance = FindObjectOfType<T>();

                        // Если не нашли, создаем новый
                        if (_instance == null)
                        {
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T)} (Singleton)";
                        }
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Обрабатывает логику 'Одиночки' при пробуждении объекта.
        /// Гарантирует, что существует только один экземпляр, и уничтожает дубликаты.
        /// Делает основной экземпляр "бессмертным" между сценами.
        /// Не забываем protected override void Awake() и вызов base.Awake();
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                // Если мы первый экземпляр, становимся главным
                _instance = this as T;

                // И делаем себя "бессмертным"

                if (gameObject.transform.parent != null)
                    Debug.LogError($"{gameObject.name} не должен быть потомком объекта", gameObject);

                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                // Если главный экземпляр уже существует, а мы - нет, то мы дубликат, и должны быть уничтожены.
                Debug.Log($"[Singleton] Instance of '{typeof(T)}' already exists. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// При выходе из приложения устанавливает флаг, чтобы предотвратить создание "призрачных" объектов.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }
    }
}
