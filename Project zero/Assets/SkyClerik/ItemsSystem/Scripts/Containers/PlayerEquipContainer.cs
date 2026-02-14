using UnityEngine.Toolbox;
using UnityEngine.DataEditor;
using UnityEngine;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой контейнер инвентаря игрока, наследующий базовую функциональность <see cref="ItemContainer"/>.
    /// Регистрируется и отменяет регистрацию в <see cref="ServiceProvider"/>.
    /// </summary>
    public class PlayerEquipContainer : ItemContainer
    {
        [Header("Настройки слота экипировки")]
        [SerializeField]
        [Tooltip("Какой тип предметов может быть помещен в этот слот.")]
        private ItemType _allowedItemType = ItemType.Any;
        public ItemType AllowedItemType => _allowedItemType;
        
        protected override void Awake()
        {
            base.Awake();
        }
    }
}