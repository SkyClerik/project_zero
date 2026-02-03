using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class PlayerLutContainer : ItemContainer
    {
        protected override void Awake()
        {
            base.Awake();
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }
    }
}