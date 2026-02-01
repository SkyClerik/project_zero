using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class PlayerItemContainer : ItemContainer
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