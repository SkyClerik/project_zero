using System;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

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
        /// <summary>
        /// Ссылка на предмет, экипированный в данный слот. Null, если слот пуст.
        /// </summary>
        public ItemBaseDefinition EquippedItem { get => _equippedItem; private set => _equippedItem = value; }

        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private Rect _rect;
        public Rect Rect => _rect;

        // События для уведомления об изменении состояния слота
        public event Action<EquipmentSlot, ItemBaseDefinition> OnItemEquipped;
        public event Action<EquipmentSlot, ItemBaseDefinition> OnItemUnequipped;

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
        public void Equip(ItemBaseDefinition item)
        {
            if (item == null) return;
            _equippedItem = item;
            OnItemEquipped?.Invoke(this, item);
        }

        /// <summary>
        /// Снимает предмет из этого слота.
        /// </summary>
        /// <returns>Снятый предмет, или null, если слот был пуст.</returns>
        public ItemBaseDefinition Unequip()
        {
            ItemBaseDefinition unequipped = _equippedItem;
            _equippedItem = null;
            if (unequipped != null)
            {
                OnItemUnequipped?.Invoke(this, unequipped);
            }
            return unequipped;
        }

        /// <summary>
        /// Проверяет, свободен ли слот экипировки.
        /// </summary>
        public bool IsEmpty => _equippedItem == null;
    }
}