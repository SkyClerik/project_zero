using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Utils
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField]
        private GlobalGameState _globalGameState = new GlobalGameState();

        public GlobalGameState GlobalGameState => _globalGameState;
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
