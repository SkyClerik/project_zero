using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Контейнер с предметами и логика взаимодействия
    /// </summary>
    public class ItemContainer : MonoBehaviour
    {
        [SerializeField]
        private ItemDataStorageSO _itemDataStorageSO;

        public ItemDataStorageSO ItemDataStorageSO => _itemDataStorageSO;

        protected virtual void Awake()
        {
            if (_itemDataStorageSO == null)
            {
                Debug.LogWarning("ItemDataStorageSO не назначен в ItemContainer. Создаем новый пустой ItemDataStorageSO.");
                _itemDataStorageSO = ScriptableObject.CreateInstance<ItemDataStorageSO>();
            }
            _itemDataStorageSO = ScriptableObject.Instantiate(_itemDataStorageSO);
            _itemDataStorageSO.ValidateGuid();
        }

        public ItemBaseDefinition AddItemAsClone(ItemBaseDefinition item)
        {
            if (item != null)
            {
                var copy = UnityEngine.Object.Instantiate(item);
                _itemDataStorageSO.Items.Add(copy);
                return copy;
            }
            return null;
        }

        public void AddItem(ItemBaseDefinition item)
        {
            if (item == null)
                return;

            if (item.Stackable)
            {
                ItemBaseDefinition existingStack = _itemDataStorageSO.Items.FirstOrDefault(i => i.DefinitionName == item.DefinitionName && i.Stackable && i.Stack < i.MaxStack);

                if (existingStack != null)
                {
                    existingStack.AddStack(item.Stack, out int remainder);
                    if (remainder == 0)
                    {
                        UnityEngine.Object.Destroy(item);
                        return;
                    }
                    else
                    {
                        item.Stack = remainder;
                    }
                }
            }
            _itemDataStorageSO.Items.Add(item);
        }

        public void AddItemReference(ItemBaseDefinition item)
        {
            if (item != null)
            {
                _itemDataStorageSO.Items.Add(item);
            }
        }

        public bool RemoveItem(ItemBaseDefinition item, bool destroy = true)
        {
            if (item == null)
                return false;

            bool removed = _itemDataStorageSO.Items.Remove(item);
            if (removed && destroy)
            {
                UnityEngine.Object.Destroy(item);
            }

            return removed;
        }

        public IReadOnlyList<ItemBaseDefinition> GetItems()
        {
            return _itemDataStorageSO.Items.AsReadOnly();
        }

        public void Clear()
        {
            _itemDataStorageSO.Items.Clear();
        }
    }
}