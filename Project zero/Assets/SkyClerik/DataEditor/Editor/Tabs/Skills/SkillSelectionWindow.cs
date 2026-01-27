using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Toolbox;
using UnityEngine.DataEditor;

namespace UnityEditor.DataEditor
{
    public class SkillSelectionWindow : EditorWindow
    {
        private Action<SkillBaseDefinition> _onSkillSelected;
        private SerializedProperty _skillsProperty;

        // UI Elements
        private VisualElement _listPanel;
        private ScrollView _detailPanel;
        private Button _selectButton;
        private ListView _skillListView;

        // Data
        private SkillBaseDefinition _selectedSkill;
        private SerializedObject _currentSerializedObject;

        private List<SkillBaseDefinition> _allSkillDefinitions;

        // Let's keep this naming convention for consistency with TraitSelectionWindow
        private List<SkillBaseDefinition> _filteredSkillDefinitions;

        public static void Open(SerializedProperty skillsProperty)
        {
            var window = GetWindow<SkillSelectionWindow>("Select Skill");
            window._skillsProperty = skillsProperty;
            window.minSize = new Vector2(800, 600);
            window.titleContent = new GUIContent("Select Skill");
            window.Focus();
        }

        private void OnEnable()
        {
            LoadAllSkillDefinitions();
            _filteredSkillDefinitions = _allSkillDefinitions;
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var mainVerticalContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };
            root.Add(mainVerticalContainer);

            var listAndDetailSplitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal)
            {
                style = { flexGrow = 1 }
            };
            mainVerticalContainer.Add(listAndDetailSplitView);

            _listPanel = new VisualElement { name = "list-panel" };
            listAndDetailSplitView.Add(_listPanel);

            _detailPanel = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    paddingTop = 5, paddingBottom = 5, paddingLeft = 5, paddingRight = 5
                }
            };
            listAndDetailSplitView.Add(_detailPanel);

            var footer = new VisualElement { style = { flexShrink = 0, height = 30, flexDirection = FlexDirection.RowReverse, borderTopWidth = 1, borderTopColor = Color.gray } };
            _selectButton = new Button(OnSelectButtonClicked) { text = "Select", style = { width = 100 } };
            _selectButton.style.marginTop = 5;
            _selectButton.style.marginBottom = 5;
            _selectButton.style.marginLeft = 5;
            _selectButton.style.marginRight = 5;
            _selectButton.SetEnabled(false);
            footer.Add(_selectButton);
            mainVerticalContainer.Add(footer);

            SetupSkillListView();
            FilterAndRebuild();
            DisplayDetails(null);
        }

        private void SetupSkillListView()
        {
            _listPanel.Clear();
            _skillListView = new ListView
            {
                makeItem = () => new Label(),
                bindItem = (element, i) =>
                {
                    (element as Label).text = _filteredSkillDefinitions[i].DefinitionName;
                    element.tooltip = _filteredSkillDefinitions[i].Description;
                },
                selectionType = SelectionType.Single,
                fixedItemHeight = 22
            };
            _skillListView.selectionChanged += OnSkillSelectionChanged;
            _listPanel.Add(_skillListView);
            AddListViewButtons();
        }

        private void FilterAndRebuild()
        {
            _filteredSkillDefinitions = _allSkillDefinitions.OrderBy(s => s.DefinitionName).ToList();

            if (_skillListView != null)
            {
                _skillListView.itemsSource = _filteredSkillDefinitions;
                _skillListView.Rebuild();
            }
        }

        private void OnSkillSelectionChanged(IEnumerable<object> selectedItems)
        {
            _selectedSkill = selectedItems.FirstOrDefault() as SkillBaseDefinition;
            DisplayDetails(_selectedSkill);
        }

        private void DisplayDetails(SkillBaseDefinition selected)
        {
            _detailPanel.Clear();
            _selectButton.SetEnabled(selected != null);

            if (selected == null)
            {
                _detailPanel.Add(new Label("Select a Skill to view details.") { style = { unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
                _currentSerializedObject = null;
                return;
            }

            _currentSerializedObject = new SerializedObject(selected);

            var propertiesContainer = new VisualElement();
            propertiesContainer.Bind(_currentSerializedObject);
            _detailPanel.Add(propertiesContainer);

            var propertyIterator = _currentSerializedObject.GetIterator();
            bool hasVisibleProperties = propertyIterator.NextVisible(true);

            if (hasVisibleProperties)
            {
                do
                {
                    if (propertyIterator.name == "m_Script") continue;

                    var propertyField = new PropertyField(propertyIterator.Copy());
                    // All fields are read-only in this window
                    propertyField.SetEnabled(false);
                    propertiesContainer.Add(propertyField);

                } while (propertyIterator.NextVisible(false));
            }
        }

        private void LoadAllSkillDefinitions()
        {
            _allSkillDefinitions = new List<SkillBaseDefinition>();
            var guids = AssetDatabase.FindAssets("t:SkillDefinition");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var skillDef = AssetDatabase.LoadAssetAtPath<SkillBaseDefinition>(path);
                if (skillDef != null) _allSkillDefinitions.Add(skillDef);
            }
        }

        private void AddListViewButtons()
        {
            var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 5 } };
            var btnNew = new Button(CreateNewSkillAsset) { text = "New Skill" };
            buttonContainer.Add(btnNew);
            var btnDelete = new Button(DeleteSelectedSkillAsset) { text = "Delete" };
            buttonContainer.Add(btnDelete);
            _listPanel.Add(buttonContainer);
        }

        private void CreateNewSkillAsset()
        {
            string directory = "Assets/Definitions/Skills";
            if (!AssetDatabase.IsValidFolder("Assets/Definitions"))
            {
                AssetDatabase.CreateFolder("Assets", "Definitions");
            }
            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets/Definitions", "Skills");
            }

            // For now, we only have EffectSkillDefinition to create
            SkillBaseDefinition newAsset = ScriptableObject.CreateInstance<SkillBaseDefinition>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{directory}/New SkillDefinition.asset");
            AssetDatabase.CreateAsset(newAsset, path);

            var so = new SerializedObject(newAsset);
            so.FindProperty("_definitionName").stringValue = newAsset.name;
            so.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            LoadAllSkillDefinitions();
            FilterAndRebuild();

            // Select the new asset in the list
            int newIndex = _filteredSkillDefinitions.IndexOf(newAsset);
            if (newIndex >= 0)
            {
                _skillListView.selectedIndex = newIndex;
            }

            Selection.activeObject = newAsset;
        }

        private void DeleteSelectedSkillAsset()
        {
            if (_selectedSkill == null) return;
            if (EditorUtility.DisplayDialog("Delete Skill Definition?", $"Are you sure you want to delete '{_selectedSkill.Description}'?", "Delete", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(_selectedSkill);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();

                _selectedSkill = null;
                LoadAllSkillDefinitions();
                FilterAndRebuild();
                DisplayDetails(null);
            }
        }

        private void OnSelectButtonClicked()
        {
            if (_selectedSkill != null && _skillsProperty != null)
            {
                _skillsProperty.serializedObject.Update();

                bool alreadyAdded = false;
                for (int i = 0; i < _skillsProperty.arraySize; i++)
                {
                    if (_skillsProperty.GetArrayElementAtIndex(i).objectReferenceValue == _selectedSkill)
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    _skillsProperty.arraySize++;
                    _skillsProperty.GetArrayElementAtIndex(_skillsProperty.arraySize - 1).objectReferenceValue = _selectedSkill;
                }
                _skillsProperty.serializedObject.ApplyModifiedProperties();
            }
            Close();
        }
    }
}
