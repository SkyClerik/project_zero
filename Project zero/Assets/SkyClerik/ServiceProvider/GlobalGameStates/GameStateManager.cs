using SkyClerik.Inventory;
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
            //   * Чтобы использовать Addressables: установи SkyClerik.Inventory.SpriteJsonConverter.UseAddressables = true;
            //   * Чтобы использовать Resources.Load: установи SkyClerik.Inventory.SpriteJsonConverter.UseAddressables = false;
            //   (и убедись, что все спрайты, которые ты хочешь грузить, находятся в папках Resources/ и их путь в identifier правильный).
            SpriteJsonConverter.UseAddressables = true;
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }
    }
}
