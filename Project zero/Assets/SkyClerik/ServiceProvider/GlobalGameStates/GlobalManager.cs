using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Utils
{
    public class GlobalManager : MonoBehaviour
    {
        [SerializeField]
        private GlobalGameProperty _globalGameState = new GlobalGameProperty();

        public GlobalGameProperty GlobalGameState => _globalGameState;
        public SaveService SaveService { get; } = new SaveService();
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
