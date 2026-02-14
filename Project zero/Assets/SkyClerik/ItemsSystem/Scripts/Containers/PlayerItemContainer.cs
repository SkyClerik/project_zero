using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using static UnityEditor.Progress;

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
                var inventoryAPI = ServiceProvider.Get<InventoryAPI>();
                foreach (var clonedItem in unplacedClones)
                {
                    Debug.Log($"clonedItem {clonedItem.ID} : {clonedItem.DefinitionName}");
                    inventoryAPI.RaisePlayerAddItemFailed(clonedItem);
                }
                itemsList.Items.AddRange(unplacedClones);
            }
        }

        internal bool AddItems(int itemID, out ItemBaseDefinition itemBaseDefinition)
        {
            var storege = ServiceProvider.Get<GlobalItemStorage>();
            itemBaseDefinition = null;
            if (storege == null)
            {
                Debug.Log($"Потерялось хранилище предметов");
                return false;
            }
            else
            {
                itemBaseDefinition = storege.GlobalItemsStorageDefinition.GetClonedItem(itemID);
                if (itemBaseDefinition == null)
                {
                    Debug.Log($"{itemBaseDefinition.ID} не найден в хранилище");
                    return false;
                }
                else
                {
                    List<ItemBaseDefinition> items = new List<ItemBaseDefinition>();
                    items.Add(itemBaseDefinition);
                    var unplacedClones = AddClonedItems(items);
                    items.Clear();

                    if (unplacedClones.Any())
                    {
                        Debug.Log($"Не удалось разместить {unplacedClones.Count} предметов.");
                        return false;
                    }

                    return true;
                }
            }
        }
    }
}
