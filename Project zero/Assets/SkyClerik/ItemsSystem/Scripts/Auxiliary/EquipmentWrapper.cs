using UnityEngine;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Обертка для слота экипировки, связывающая логический контейнер предмета с его UI-представлением.
    /// </summary>
    [System.Serializable]
    public class EquipmentWrapper
    {
        [SerializeField]
        private ItemContainer _itemContainer;
        [SerializeField]
        private EquipPageElement _equipPageElement;

        public ItemContainer ItemContainer { get => _itemContainer; set => _itemContainer = value; }
        public EquipPageElement EquipPageElement { get => _equipPageElement; set => _equipPageElement = value; }
    }
}
