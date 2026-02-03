using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой контейнер для лута игрока, наследующий базовую функциональность <see cref="ItemContainer"/>.
    /// Регистрируется и отменяет регистрацию в <see cref="ServiceProvider"/>.
    /// </summary>
    public class PlayerLutContainer : ItemContainer
    {
        [SerializeField]
        private LutContainer _currentLutContainerTransfer;

        protected override void Awake()
        {
            base.Awake();
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        /// <summary>
        /// Добавляет предметы из указанного контейнера лута в текущий контейнер.
        /// </summary>
        /// <param name="sourceLut">Контейнер лута, из которого будут взяты предметы.</param>
        public void AddItems(ref List<ItemBaseDefinition> items, LutContainer container)
        {
            _currentLutContainerTransfer = container;

            if (items.Count <= 0)
                return;

            var unplacedClones = AddClonedItems(items);

            items.Clear();

            if (unplacedClones.Any())
            {
                //Debug.Log($"Не удалось разместить {unplacedClones.Count} предметов. Возвращаем в LutContainer.");
                items.AddRange(unplacedClones);
            }
        }
    }
}