using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;
using SkyClerik.Inventory;
using System;

namespace SkyClerik.EquipmentSystem
{
    [Serializable]
    public class EquipmentSlot
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [SerializeField]
        [SerializeReference]
        private ItemBaseDefinition _equippedItem;

        [JsonProperty]
        [SerializeField]
        [ReadOnly] // Заполняется методом CalculateGridDimensionsFromUI
        private Rect _rect;

        [SerializeField]
        [ReadOnly]
        private ItemVisual _itemVisual;
        [SerializeField]
        [ReadOnly]
        private string _cellNameDebug;
        private VisualElement _cell;

        public Rect Rect => _rect;
        public bool IsEmpty => _itemVisual == null;
        public ItemBaseDefinition EquippedItem => _equippedItem;

        public VisualElement Cell
        {
            get => _cell;
            set
            {
                _cell = value;
                _cellNameDebug = _cell.name;
            }
        }

        public EquipmentSlot(Rect rect)
        {
            _rect = rect;
        }

        public void CreateItemVisualAndEquip(ItemsPage itemsPage, ItemBaseDefinition itemBaseDefinition)
        {
            if (_equippedItem != null)
            {
                var itemVisual = new ItemVisual(
                           itemsPage: itemsPage,
                           ownerInventory: itemsPage.ContainersAndPages[0].Page,
                           itemDefinition: itemBaseDefinition,
                           gridPosition: Vector2Int.zero,
                           gridSize: Vector2Int.zero
                           );
                Equip(itemVisual);
            }
        }

        /// <summary>
        /// Проверяет, подходит ли данный предмет для экипировки в этот слот.
        /// </summary>
        /// <param name="item">Предмет для проверки.</param>
        /// <returns>True, если предмет подходит; иначе false.</returns>
        public bool CanEquip(ItemBaseDefinition item)
        {
            if (item == null)
                return false;
            // TODO заглушка на проверке экипируемого предмета, надо решить на что проверять
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

            _equippedItem = itemVisual.ItemDefinition;
            _itemVisual = itemVisual;
            ItemsPage.CurrentDraggedItem = null;

            _cell.Add(itemVisual);
        }

        /// <summary>
        /// Снимает предмет из этого слота.
        /// </summary>
        /// <returns>Снятый предмет, или null, если слот был пуст.</returns>
        public void Unequip(UIDocument document)
        {
            ItemsPage.CurrentDraggedItem = _itemVisual;
            document.rootVisualElement.Add(ItemsPage.CurrentDraggedItem);
            _itemVisual = null;

            ItemBaseDefinition itemBaseDefinition = _equippedItem;
            _equippedItem = null;
        }
    }
}