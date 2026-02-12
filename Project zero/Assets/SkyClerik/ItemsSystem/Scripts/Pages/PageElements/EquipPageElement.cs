using System.Collections;
using UnityEngine;
using UnityEngine.DataEditor;
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

        private VisualElement _style;
        private const string _styleID = "style";

        private Coroutine _draggedItemCoroutine;

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

            _style = _body.Q(_styleID);

            Debug.Log($"_style : {_style.name}");

            ServiceProvider.Get<InventoryAPI>().OnItemPickUp += EquipPageElement_OnItemPickUp;
            ServiceProvider.Get<InventoryAPI>().OnItemDrop += EquipPageElement_OnItemDrop;
        }

        private void EquipPageElement_OnItemPickUp(ItemVisual item, GridPageElementBase gridPage)
        {
            _style.SetBorderWidth(3);
            _style.SetBorderRadius(3);
            _style.SetBorderColor(Color.red);
        }

        private void EquipPageElement_OnItemDrop(ItemVisual item, GridPageElementBase gridPage)
        {
            _style.SetBorderWidth(0);
            _style.SetBorderRadius(0);
            _style.SetBorderColor(Color.red);
        }

        public override void Dispose()
        {
            base.Dispose();
            ServiceProvider.Get<InventoryAPI>().OnItemPickUp -= EquipPageElement_OnItemPickUp;
            ServiceProvider.Get<InventoryAPI>().OnItemPickUp -= EquipPageElement_OnItemDrop;
        }
    }
}
