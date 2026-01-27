using UnityEngine.DataEditor;
using UnityEngine.UIElements;

namespace UnityEditor.DataEditor
{
    public class WeaponsTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;

        private ListViewPanel<WeaponDefinition, WeaponDatabase> _listView;
        private WeaponDetailPanel _detailPanel;

        public WeaponsTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "weapons-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _detailPanel = new WeaponDetailPanel(() => _listView.RebuildList());
            splitView.Add(_detailPanel);

            _listView = new ListViewPanel<WeaponDefinition, WeaponDatabase>(
                leftPanel,
                () => DataEditorWindow.CreateNewAsset<WeaponDefinition>("Items/Weapons")
            );
            _listView.OnDefinitionSelected += _detailPanel.DisplayDetails;
            _listView.RebuildList();
        }

        public void Unload()
        {
            _listView = null;
            _detailPanel = null;
            _container.Clear();
        }
    }
}
