using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{

    public class PlayerItemContainer : ItemContainerBase
    {
        protected override void Awake()
        {
            base.Awake(); // Вызов базовой реализации Awake для инициализации ItemContainerBase
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }
    }
}