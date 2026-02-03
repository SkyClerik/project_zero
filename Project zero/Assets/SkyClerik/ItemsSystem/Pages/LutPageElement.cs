using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
	public class LutPageElement : GridPageElementBase
	{
		private const string _craftPageTitleText = "Трофейня";
		private VisualElement _header;
		private const string _headerID = "header";
		private Label _title;
		private const string _titleID = "l_title";
		private VisualElement _body;
		private const string _bodyID = "body";

		public LutPageElement(ItemsPage itemsPage, UIDocument document, ItemContainer itemContainer)
	: base(itemsPage, document, itemContainer, itemContainer.RootPanelName)
		{
			_header = _root.Q(_headerID);
			_title = _header.Q<Label>(_titleID);
			_body = _root.Q(_bodyID);

			_title.text = _craftPageTitleText;
		}
	}
}