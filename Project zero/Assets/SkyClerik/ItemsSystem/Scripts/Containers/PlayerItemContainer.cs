using System.Linq;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет собой контейнер инвентаря игрока, наследующий базовую функциональность <see cref="ItemContainer"/>.
    /// Регистрируется и отменяет регистрацию в <see cref="ServiceProvider"/>.
    /// </summary>
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

        /// <summary>
        /// Добавляет предметы из указанного контейнера лута в текущий контейнер.
        /// </summary>
        /// <param name="sourceLut">Контейнер лута, из которого будут взяты предметы.</param>
        internal void AddItems(ItemsList itemsList)
        {
            if (itemsList.Items.Count <= 0)
                return;

            var unplacedClones = AddClonedItems(itemsList.Items);

            itemsList.Items.Clear();

            if (unplacedClones.Any())
            {
                Debug.Log($"Не удалось разместить {unplacedClones.Count} предметов. Возвращаем в LutContainer.");
                foreach (var clonedItem in unplacedClones)
                {
                    Debug.Log($"clonedItem {clonedItem.ID} : {clonedItem.DefinitionName}");
                }
                itemsList.Items.AddRange(unplacedClones);
            }
        }
    }
}