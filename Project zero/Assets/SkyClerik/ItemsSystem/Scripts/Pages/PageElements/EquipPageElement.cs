using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Элемент страницы, представляющий собой слот экипировки.
    /// Наследует функциональность GridPageElementBase для управления визуалом одного слота.
    /// </summary>
    public class EquipPageElement : GridPageElementBase
    {
        private const string _titleText = "Хранилище предметов";
        private VisualElement _body;
        private const string _bodyID = "body";

        private List<VisualElement> _styles;
        private const string _styleID = "style";

        /// <summary>
        /// Инициализирует новый экземпляр класса EquipPageElement.
        /// </summary>
        /// <param name="inventoryStorage">Главный контроллер инвентаря.</param>
        /// <param name="document">UIDocument, к которому принадлежит страница.</param>
        /// <param name="itemContainer">Логический контейнер предметов, связанный с этим слотом экипировки.</param>
        /// <param name="rootID">ID корневого визуального элемента страницы/слота в UIDocument.</param>
        public EquipPageElement(InventoryStorage inventoryStorage, UIDocument document, ItemContainer itemContainer, string rootID)
            : base(inventoryStorage, document, itemContainer, rootID)
        {
            _body = _root.Q(rootID);

            Debug.Log($"rootID : {rootID} -  _body : {_body.name}");

            _styles = _body.Query<VisualElement>(name: _styleID).ToList();

            foreach (var style in _styles)
            {
                Debug.Log($"_style : {style.name}");
            }

            ServiceProvider.Get<InventoryAPI>().OnItemPickUp += EquipPageElement_OnItemPickUp;
            ServiceProvider.Get<InventoryAPI>().OnItemDrop += EquipPageElement_OnItemDrop;
        }

        private void EquipPageElement_OnItemPickUp(ItemVisual item, GridPageElementBase gridPage)
        {
            foreach (var style in _styles)
            {
                style.SetBorderWidth(3);
                style.SetBorderRadius(3);
                style.SetBorderColor(Color.red);
            }
        }

        private void EquipPageElement_OnItemDrop(ItemVisual item, GridPageElementBase gridPage)
        {
            foreach (var style in _styles)
            {
                style.SetBorderWidth(0);
                style.SetBorderRadius(0);
                style.SetBorderColor(Color.red);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            var inventoryAPI = ServiceProvider.Get<InventoryAPI>();
            if (inventoryAPI != null)
            {
                inventoryAPI.OnItemPickUp -= EquipPageElement_OnItemPickUp;
                inventoryAPI.OnItemDrop -= EquipPageElement_OnItemDrop;
            }
        }

        public override void FinalizeDrag()
        {
            base.FinalizeDrag();
        }

        public override PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            var equipContainer = _itemContainer as PlayerEquipContainer;
            if (equipContainer != null && equipContainer.AllowedItemType != UnityEngine.DataEditor.ItemType.Any && draggedItem.ItemDefinition.ItemType != equipContainer.AllowedItemType)
            {
                // Типы не совпадают, сразу возвращаем конфликт
                return new PlacementResults().Init(ReasonConflict.invalidSlotType, Vector2.zero, Vector2Int.zero, null, this);
            }

            return base.ShowPlacementTarget(draggedItem);
        }
    }
}
