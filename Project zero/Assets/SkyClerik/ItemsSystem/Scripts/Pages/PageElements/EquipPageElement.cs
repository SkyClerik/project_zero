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

        /// <summary>
        /// Инициализирует новый экземпляр класса EquipPageElement.
        /// </summary>
        /// <param name="itemsPage">Главный контроллер инвентаря.</param>
        /// <param name="document">UIDocument, к которому принадлежит страница.</param>
        /// <param name="itemContainer">Логический контейнер предметов, связанный с этим слотом экипировки.</param>
        /// <param name="rootID">ID корневого визуального элемента страницы/слота в UIDocument.</param>
        public EquipPageElement(InventoryContainer itemsPage, UIDocument document, ItemContainer itemContainer, string rootID) 
            : base(itemsPage, document, itemContainer, rootID)
        {
            _body = _root.Q(_bodyID);
        }
    }
}
