using System;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;
using SkyClerik.Inventory;

namespace SkyClerik.EquipmentSystem
{
    /// <summary>
    /// Представляет единичную ячейку экипировки.
    /// Определяет тип предметов, которые могут быть экипированы в этот слот,
    /// и хранит ссылку на экипированный предмет.
    /// </summary>
    [System.Serializable]
    public class EquipmentSlot
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [SerializeField]
        [SerializeReference]
        private ItemBaseDefinition _equippedItem;

        [SerializeField]
        [ReadOnly]
        private VisualElement _cellPlace;

        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private Rect _rect;

        [SerializeField]
        [ReadOnly]
        private ItemVisual _itemVisual;

        // События для уведомления об изменении состояния слота
        public event Action<EquipmentSlot, ItemVisual> OnItemEquipped;
        public event Action<EquipmentSlot, ItemVisual> OnItemUnequipped;

        public VisualElement CallPlace { get => _cellPlace; set => _cellPlace = value; }
        public Rect Rect => _rect;
        public ItemVisual ItemVisual { get => _itemVisual; set => _itemVisual = value; }

        public bool IsEmpty => _itemVisual == null;

        public ItemBaseDefinition EquippedItem { get => _equippedItem; set => _equippedItem = value; }

        public EquipmentSlot(Rect rect)
        {
            _rect = rect;
        }

        /// <summary>
        /// Проверяет, подходит ли данный предмет для экипировки в этот слот.
        /// </summary>
        /// <param name="item">Предмет для проверки.</param>
        /// <returns>True, если предмет подходит; иначе false.</returns>
        public bool CanEquip(ItemBaseDefinition item)
        {
            if (item == null) return false;
            // Проверяем, является ли тип предмета совместимым с разрешенным типом слота.
            // AllowedType.IsAssignableFrom(item.GetType()) проверяет, можно ли назначить
            // экземпляр item.GetType() переменной типа AllowedType.
            // Это позволяет использовать наследование, например, в слот "Weapon"
            // можно будет поместить "Sword" или "Axe".
            return true;
        }

        /// <summary>
        /// Экипирует предмет в этот слот.
        /// </summary>
        /// <param name="item">Предмет для экипировки.</param>
        public void Equip(ItemVisual itemVisual)
        {
            if (itemVisual == null) 
                return;

            _itemVisual = itemVisual;
            OnItemEquipped?.Invoke(this, itemVisual);
        }

        /// <summary>
        /// Снимает предмет из этого слота.
        /// </summary>
        /// <returns>Снятый предмет, или null, если слот был пуст.</returns>
        public ItemVisual Unequip()
        {
            ItemVisual itemVisual = _itemVisual;
            _itemVisual = null;
            if (itemVisual != null)
            {
                OnItemUnequipped?.Invoke(this, itemVisual);
            }
            return itemVisual;
        }


    }
}