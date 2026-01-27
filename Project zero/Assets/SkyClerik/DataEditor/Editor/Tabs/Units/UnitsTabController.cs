using UnityEngine.DataEditor;
using UnityEngine.UIElements;

namespace UnityEditor.DataEditor
{
    public class UnitsTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;

        private ListViewPanel<UnitBaseDefinition, UnitDatabase> _unitsListView;
        private UnitDetailPanel _unitDetailPanel;

        public UnitsTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "units-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _unitDetailPanel = new UnitDetailPanel(() => _unitsListView.RebuildList());
            splitView.Add(_unitDetailPanel);

            _unitsListView = new ListViewPanel<UnitBaseDefinition, UnitDatabase>(
                leftPanel,
                () => DataEditorWindow.CreateNewAsset<UnitBaseDefinition>("Units")
            );
            _unitsListView.OnDefinitionSelected += _unitDetailPanel.DisplayDetails;
            _unitsListView.RebuildList();
        }

        public void Unload()
        {
            _unitsListView = null;
            _unitDetailPanel = null;
            _container.Clear();
        }
    }
}
