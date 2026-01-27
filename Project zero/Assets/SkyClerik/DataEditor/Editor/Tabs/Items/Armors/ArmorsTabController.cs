using UnityEngine.DataEditor;
using UnityEngine.UIElements;

namespace UnityEditor.DataEditor
{
    public class ArmorsTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;

        private ListViewPanel<ArmorDefinition, ArmorDatabase> _listView;
        private ArmorDetailPanel _detailPanel;

        public ArmorsTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "armors-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _detailPanel = new ArmorDetailPanel(() => _listView.RebuildList());
            splitView.Add(_detailPanel);

            _listView = new ListViewPanel<ArmorDefinition, ArmorDatabase>(
                leftPanel,
                () => DataEditorWindow.CreateNewAsset<ArmorDefinition>("Items/Armors")
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
