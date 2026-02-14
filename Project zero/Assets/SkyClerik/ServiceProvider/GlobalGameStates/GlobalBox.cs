using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Utils
{
    /// <summary>
    /// Глобальный менеджер, отвечающий за управление состоянием игры,
    /// сервисами сохранения и загрузки. Регистрируется в <see cref="ServiceProvider"/>.
    /// </summary>
    public class GlobalBox : MonoBehaviour
    {
        [SerializeField]
        private GlobalGameProperty _globalGameProperty = new GlobalGameProperty();

        /// <summary>
        /// Возвращает текущие глобальные свойства игры.
        /// </summary>
        public GlobalGameProperty GlobalGameProperty => _globalGameProperty;
        /// <summary>
        /// Возвращает экземпляр сервиса сохранения данных.
        /// </summary>
        public SaveService SaveService { get; } = new SaveService();
        /// <summary>
        /// Возвращает экземпляр сервиса загрузки данных.
        /// </summary>
        public LoadService LoadService { get; } = new LoadService();

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }
    }
}
