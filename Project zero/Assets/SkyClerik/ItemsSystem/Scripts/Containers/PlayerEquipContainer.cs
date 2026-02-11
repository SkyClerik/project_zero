using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой контейнер инвентаря игрока, наследующий базовую функциональность <see cref="ItemContainer"/>.
    /// Регистрируется и отменяет регистрацию в <see cref="ServiceProvider"/>.
    /// </summary>
    public class PlayerEquipContainer : ItemContainer
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }
}