using UnityEngine.DataEditor;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace UnityEditor.DataEditor
{
    public class SkillsTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;

        private ListViewPanel<SkillBaseDefinition, SkillDatabase> _listView;
        private SkillDetailPanel _detailPanel;

        public SkillsTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "skills-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _detailPanel = new SkillDetailPanel(() => _listView.RebuildList());
            splitView.Add(_detailPanel);

            // Create the ListViewPanel without the default Add/New buttons
            _listView = new ListViewPanel<SkillBaseDefinition, SkillDatabase>(
                leftPanel,
                null, // No synchronous creation handler
                false // Do not show the default button panel
            );
            _listView.OnDefinitionSelected += _detailPanel.DisplayDetails;
            _listView.RebuildList();

            // Add our custom button
            var addButton = new Button(ShowAddSkillMenu) { text = "Добавить Навык" };
            leftPanel.Add(addButton);
        }

        private void ShowAddSkillMenu()
        {
            TypeSelectionWindow.ShowWindow(typeof(SkillBaseDefinition), (selectedType) =>
            {
                CreateAndAddNewAsset(selectedType);
            });
        }

        private void CreateAndAddNewAsset(Type assetType)
        {
            // Use reflection to call the generic CreateNewAsset method
            MethodInfo method = typeof(DataEditorWindow).GetMethod(nameof(DataEditorWindow.CreateNewAsset), BindingFlags.Public | BindingFlags.Static);
            MethodInfo genericMethod = method.MakeGenericMethod(assetType);
            var newAsset = genericMethod.Invoke(null, new object[] { "Skills" }) as SkillBaseDefinition;

            if (newAsset != null)
            {
                // The ListViewPanel needs to know how to add this to its database
                // This is a bit of a hack, but it's the cleanest way without majorly refactoring ListViewPanel
                var database = DataEditorWindow.LoadOrCreateDatabase<SkillDatabase>();
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
