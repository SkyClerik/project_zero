using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class PlayerCraftContainer : ItemContainerBase
    {
        protected override void Awake()
        {
            ServiceProvider.Register(this);

        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }
    }
}