using UnityEngine.DataEditor;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace UnityEditor.DataEditor
{
    public class StateTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;

        private ListViewPanel<StateBaseDefinition, StateDatabase> _listView;
        private StateDetailPanel _detailPanel;

        public StateTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "states-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _detailPanel = new StateDetailPanel(() => _listView.RebuildList());
            splitView.Add(_detailPanel);

            _listView = new ListViewPanel<StateBaseDefinition, StateDatabase>(
                leftPanel,
                null, // No synchronous creation handler
                false // Do not show the default button panel
            );
            _listView.OnDefinitionSelected += _detailPanel.DisplayDetails;
            _listView.RebuildList();
            
            var addButton = new Button(ShowAddStateMenu) { text = "Добавить Состояние" };
            leftPanel.Add(addButton);
        }
        
        private void ShowAddStateMenu()
        {
            TypeSelectionWindow.ShowWindow(typeof(StateBaseDefinition), (selectedType) =>
            {
                CreateAndAddNewAsset(selectedType);
            });
        }

        private void CreateAndAddNewAsset(Type assetType)
        {
            MethodInfo method = typeof(DataEditorWindow).GetMethod(nameof(DataEditorWindow.CreateNewAsset), BindingFlags.Public | BindingFlags.Static);
            MethodInfo genericMethod = method.MakeGenericMethod(assetType);
            var newAsset = genericMethod.Invoke(null, new object[] { "States" }) as StateBaseDefinition;

            if (newAsset != null)
            {
                var database = DataEditorWindow.LoadOrCreateDatabase<StateDatabase>();
                var serializedDatabase = new SerializedObject(database);
                var itemsProperty = serializedDatabase.FindProperty("_items");
                itemsProperty.InsertArrayElementAtIndex(itemsProperty.arraySize);
                itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1).objectReferenceValue = newAsset;
                serializedDatabase.ApplyModifiedProperties();

                _listView.RebuildList();
                _listView.SetSelection(newAsset);
            }
        }

        public void Unload()
        {
            _listView = null;
            _detailPanel = null;
            _container.Clear();
        }
    }
}
