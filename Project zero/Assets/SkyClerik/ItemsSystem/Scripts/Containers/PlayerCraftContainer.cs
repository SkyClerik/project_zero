using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой контейнер для крафта игрока, наследующий базовую функциональность <see cref="ItemContainer"/>.
    /// Регистрируется и отменяет регистрацию в <see cref="ServiceProvider"/>.
    /// </summary>
    public class PlayerCraftContainer : ItemContainer
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