using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
	/// <summary>                                                                                                                              
	/// Представляет элемент UI страницы сундука, управляющий отображением предметов                                                           
	/// и взаимодействием с пользователем. Наследует функциональность базовой страницы сетки.                                                  
	/// </summary> 
	public class CheastPageElement : GridPageElementBase
	{
		private const string _titleText = "Хранилище предметов";
		private VisualElement _body;
		private const string _bodyID = "body";

		public CheastPageElement(InventoryContainer itemsPage, UIDocument document, ItemContainer itemContainer)
	: base(itemsPage, document, itemContainer, itemContainer.RootPanelName)
		{
			_body = _root.Q(_bodyID);
		}
	}
}