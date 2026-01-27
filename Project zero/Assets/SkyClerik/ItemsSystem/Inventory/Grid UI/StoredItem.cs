using System;
using UnityEngine;
using UnityEngine.DataEditor;

namespace Gameplay.Inventory
{
    [Serializable]
    public class StoredItem
    {
        [SerializeField]
        private ItemBaseDefinition _itemDefinition;
        [SerializeField]
        private ItemVisual _itemVisual;

        //public ItemBaseDefinition ItemDefinition { get => _itemDefinition; set => _itemDefinition = value; }
        public ItemBaseDefinition ItemDefinition => _itemDefinition;
        public ItemVisual ItemVisual { get => _itemVisual; set => _itemVisual = value; }

        [NonSerialized]
        private IDropTarget _owner;
        public IDropTarget Owner
        {
            get => _owner;
            set => _owner = value;
        }

        public StoredItem(ItemBaseDefinition itemBaseDefinition)
        {
            _itemDefinition = itemBaseDefinition;
        }
    }

    //[Serializable]
    //public class EquipSlot
    //{
    //	[SerializeField]
    //	private StoredItem _storedItem;
    //	[SerializeField]
    //	[Tooltip("Границы слота (позиция и размер в мировых координатах)")]
    //	private Rect _slotRect;
    //	[SerializeField]
    //	private string _slotName;

    //	public StoredItem StoredItem { get => _storedItem; set => _storedItem = value; }
    //	public Rect SlotRect { get => _slotRect; set => _slotRect = value; }
    //	public string SlotName { get => _slotName; set => _slotName = value; }
    //}
}