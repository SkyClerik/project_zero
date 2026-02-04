using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.EquipmentSystem
{
    /// <summary>
    /// ScriptableObject, хранящий список EquipmentSlot и предоставляющий логику управления экипировкой.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentContainerDefinition", menuName = "SkyClerik/Inventory/Equipment Container Definition")]
    [System.Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class EquipmentContainerDefinition : ScriptableObject
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private string _containerGuid;
        /// <summary>
        /// Уникальный идентификатор контейнера экипировки.
        /// </summary>
        public string ContainerGuid { get => _containerGuid; private set => _containerGuid = value; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        [SerializeField]
        private List<EquipmentSlot> _equipmentSlots = new List<EquipmentSlot>();
        /// <summary>
        /// Список всех слотов экипировки.
        /// </summary>
        public List<EquipmentSlot> EquipmentSlots => _equipmentSlots;

        // События для уведомления об изменении состояния экипировки
        public event Action<EquipmentSlot, ItemBaseDefinition> OnItemEquipped;
        public event Action<EquipmentSlot, ItemBaseDefinition> OnItemUnequipped;

        /// <summary>
        /// Проверяет и генерирует GUID для контейнера, если он отсутствует.
        /// </summary>
        public void ValidateGuid()
        {
            if (string.IsNullOrEmpty(_containerGuid))
            {
                _containerGuid = Guid.NewGuid().ToString();
                Debug.Log($"Сгенерирован новый GUID для контейнера экипировки: {_containerGuid} (из ValidateGuid)");
            }
        }

        /// <summary>
        /// Возвращает слот экипировки, предназначенный для указанного типа предмета.
        /// Возвращает первый найденный слот, соответствующий типу.
        /// </summary>
        /// <param name="allowedType">Тип предмета, для которого ищется слот.</param>
        /// <returns>Найденный EquipmentSlot или null, если слот не найден.</returns>
        //public EquipmentSlot GetSlot(Type allowedType)
        //{
        //    return _equipmentSlots.FirstOrDefault(s => s.AllowedType == allowedType);
        //}

        /// <summary>
        /// Возвращает слот экипировки, соответствующий указанной позиции UI.
        /// </summary>
        /// <param name="mousePosition">Позиция курсора.</param>
        /// <returns>Найденный EquipmentSlot или null, если позиция не попадает ни в один слот.</returns>
        public EquipmentSlot GetSlot(Vector2 mousePosition)
        {
            foreach (var slot in _equipmentSlots)
            {
                if (slot.Rect.Contains(mousePosition))
                    return slot;
            }
            return null;
        }

        /// <summary>
        /// Пытается экипировать предмет в указанный слот.
        /// </summary>
        /// <param name="item">Предмет для экипировки.</param>
        /// <param name="targetSlot">Целевой слот экипировки.</param>
        /// <param name="unequippedItem">Предмет, который был снят из слота (если слот был занят).</param>
        /// <returns>True, если предмет успешно экипирован (или произошел свап); иначе false.</returns>
        public bool TryEquipItem(ItemBaseDefinition item, EquipmentSlot targetSlot, out ItemBaseDefinition unequippedItem)
        {
            unequippedItem = null;

            if (targetSlot == null || !targetSlot.CanEquip(item))
            {
                Debug.LogWarning($"[EquipmentContainerDefinition] Предмет '{item?.name}' не может быть экипирован в слот.");
                return false;
            }

            if (!targetSlot.IsEmpty)
            {
                // Слот занят, делаем свап
                unequippedItem = targetSlot.Unequip();
                Debug.Log($"[EquipmentContainerDefinition] Предмет '{unequippedItem.name}' снят из слота.");
            }

            targetSlot.Equip(item);
            Debug.Log($"[EquipmentContainerDefinition] Предмет '{item.name}' экипирован в слот.");

            // Вызываем внутренние события
            targetSlot.OnItemEquipped -= HandleSlotItemEquipped; // Предотвращаем дублирование
            targetSlot.OnItemEquipped += HandleSlotItemEquipped;
            targetSlot.OnItemUnequipped -= HandleSlotItemUnequipped; // Предотвращаем дублирование
            targetSlot.OnItemUnequipped += HandleSlotItemUnequipped;

            // Вызываем внешнее событие контейнера
            OnItemEquipped?.Invoke(targetSlot, item);

            return true;
        }

        /// <summary>
        /// Снимает предмет из указанного слота.
        /// </summary>
        /// <param name="slot">Слот, из которого нужно снять предмет.</param>
        /// <returns>Снятый предмет, или null, если слот был пуст.</returns>
        public ItemBaseDefinition UnequipItem(EquipmentSlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                Debug.LogWarning($"[EquipmentContainerDefinition] Попытка снять предмет из пустого или несуществующего слота.");
                return null;
            }

            ItemBaseDefinition unequipped = slot.Unequip();
            Debug.Log($"[EquipmentContainerDefinition] Предмет '{unequipped.name}' снят из слота.");

            // Вызываем внешнее событие контейнера
            OnItemUnequipped?.Invoke(slot, unequipped);

            return unequipped;
        }

        // Обработчики внутренних событий слотов для проброса во внешние события контейнера
        private void HandleSlotItemEquipped(EquipmentSlot slot, ItemBaseDefinition item)
        {
            OnItemEquipped?.Invoke(slot, item);
        }

        private void HandleSlotItemUnequipped(EquipmentSlot slot, ItemBaseDefinition item)
        {
            OnItemUnequipped?.Invoke(slot, item);
        }
    }
}