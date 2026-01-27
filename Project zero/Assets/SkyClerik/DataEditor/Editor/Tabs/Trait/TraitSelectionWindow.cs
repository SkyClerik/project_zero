//using System;
//using System.Linq;
//using System.Reflection;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UIElements;
//using UnityEditor.UIElements;
//using UnityEditor.Toolbox;
//using UnityEngine.DataEditor;

//namespace UnityEditor.DataEditor
//{
//    public class TraitSelectionWindow : EditorWindow
//    {
//        private Action<TraitTypeDefinition> _onTraitSelected;

//        // UI Elements
//        private VisualElement _listPanel;
//        private ScrollView _detailPanel;
//        private Button _selectButton;
//        private ListView _traitListView;

//        // Data
//        private TraitTypeDefinition _selectedTrait;
//        private SerializedObject _currentSerializedObject;
        
//        private List<TraitTypeDefinition> _allTraitDefinitions;
//        private List<TraitTypeDefinition> _filteredTraitDefinitions;

//        private TraitCategoryDefinition _currentSelectedCategory;
//        private List<TraitCategoryDefinition> _allCategories = new List<TraitCategoryDefinition>(); // Populated in OnEnable
//        // private List<Elements.TraitCategoryEntry> _categoryEntries = new List<Elements.TraitCategoryEntry>(); // REMOVED - TabButtonGroup handles this
//        private TabButtonGroup _tabButtonGroup; // ADDED

//        public static void ShowWindow(Action<TraitTypeDefinition> onTraitSelected)
//        {
//            var window = GetWindow<TraitSelectionWindow>("Select Trait Blueprint");
//            window._onTraitSelected = onTraitSelected;
//            window.minSize = new Vector2(800, 600);
//        }

//        private void OnEnable()
//        {
//            // --- DATA PREPARATION ---
//            // This ensures data is 100% ready before CreateGUI is called.
//            LoadAllCategories();
//            LoadAllTraitDefinitions();
//        }

//        public void CreateGUI()
//        {
//            var root = rootVisualElement;
//            root.Clear();

//            // Main container for the whole window layout (Column direction for top, middle, bottom)
//            var mainVerticalContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };
//            root.Add(mainVerticalContainer);

//            // --- CATEGORY PANEL (TOP) ---
//            var categoryPanelContainer = new VisualElement
//            {
//                name = "category-panel-container",
//                style = { flexShrink = 0, flexDirection = FlexDirection.Row, borderBottomWidth = 1, borderBottomColor = Color.gray, height = 30 }
//            };
//            mainVerticalContainer.Add(categoryPanelContainer);

//            // --- MIDDLE CONTENT AREA (LIST + DETAILS) ---
//            // This is the single "element" you mentioned, which itself is a horizontal split view
//            var listAndDetailSplitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal)
//            {
//                style = { flexGrow = 1 } // Take up all available vertical space
//            };
//            mainVerticalContainer.Add(listAndDetailSplitView);

//            _listPanel = new VisualElement { name = "list-panel" };
//            listAndDetailSplitView.Add(_listPanel); // First pane

//            _detailPanel = new ScrollView(ScrollViewMode.Vertical);
//            _detailPanel.style.paddingTop = 5;
//            _detailPanel.style.paddingBottom = 5;
//            _detailPanel.style.paddingLeft = 5;
//            _detailPanel.style.paddingRight = 5;
//            listAndDetailSplitView.Add(_detailPanel); // Second pane
            
//            // --- FOOTER (BOTTOM) ---
//            var footer = new VisualElement { style = { flexShrink = 0, height = 30, flexDirection = FlexDirection.RowReverse, borderTopWidth = 1, borderTopColor = Color.gray } };
//            _selectButton = new Button(OnSelectButtonClicked) { text = "Select", style = { width = 100 } };
//            _selectButton.style.marginTop = 5;
//            _selectButton.style.marginBottom = 5;
//            _selectButton.style.marginLeft = 5;
//            _selectButton.style.marginRight = 5;
//            _selectButton.SetEnabled(false);
//            footer.Add(_selectButton);
//            mainVerticalContainer.Add(footer);

//            // --- 3. POPULATE UI with prepared data ---
//            // Create standard buttons for each category
//            var categoryButtons = new List<Button>();
//            foreach (var categoryDef in _allCategories)
//            {
//                var button = new Button { text = categoryDef.DefinitionName, userData = categoryDef, style = { flexGrow = 1 } };
//                categoryButtons.Add(button);
//            }
            
//            // Initialize TabButtonGroup
//            _tabButtonGroup = new TabButtonGroup(categoryButtons, Color.green, 2, TabButtonGroup.BorderSides.Horizontal);
//            _tabButtonGroup.OnSelectedButtonChanged += OnCategoryTabSelected; // Subscribe to its event
//            categoryPanelContainer.Add(_tabButtonGroup); // Add the TabButtonGroup (which contains the buttons) to the categoryPanelContainer

//            SetupTraitListView();

//            // Set initial selection
//            if (_allCategories.Any())
//            {
//                var firstCategoryButton = categoryButtons.FirstOrDefault(btn => (btn.userData as TraitCategoryDefinition) == _allCategories.First());
//                if (firstCategoryButton != null)
//                {
//                    _tabButtonGroup.SetSelected(firstCategoryButton);
//                }
//            }
//            else
//            {
//                FilterAndRebuild();
//                DisplayDetails(null);
//            }
//        }
        
//        private void SetupTraitListView()
//        {
//            _listPanel.Clear();
//            _traitListView = new ListView
//            {
//                makeItem = () => new Label(),
//                bindItem = (element, i) =>
//                {
//                    (element as Label).text = _filteredTraitDefinitions[i].DefinitionName;
//                    element.tooltip = _filteredTraitDefinitions[i].Description;
//                },
//                selectionType = SelectionType.Single,
//                fixedItemHeight = 22
//            };
//            _traitListView.selectionChanged += OnTraitSelectionChanged;
//            _listPanel.Add(_traitListView);
//            AddListViewButtons();
//        }
        
//        private void FilterAndRebuild()
//        {
//             _filteredTraitDefinitions = (_currentSelectedCategory == null)
//                ? new List<TraitTypeDefinition>()
//                : _allTraitDefinitions.Where(t => t.Category == _currentSelectedCategory).ToList();

//            if (_traitListView != null)
//            {
//                _traitListView.itemsSource = _filteredTraitDefinitions;
//                _traitListView.Rebuild();
//            }
//        }

//        private void OnCategoryTabSelected(Button selectedButton) // NEW METHOD
//        {
//            _currentSelectedCategory = selectedButton?.userData as TraitCategoryDefinition;
//            FilterAndRebuild();
//            _traitListView.ClearSelection();
//            DisplayDetails(null);
//        }

//        private void OnTraitSelectionChanged(IEnumerable<object> selectedItems)
//        {
//            _selectedTrait = selectedItems.FirstOrDefault() as TraitTypeDefinition;
//            DisplayDetails(_selectedTrait);
//        }

//        private void DisplayDetails(TraitTypeDefinition selected)
//        {
//            _detailPanel.Clear();
//            _selectButton.SetEnabled(selected != null);

//            if (selected == null)
//            {
//                _detailPanel.Add(new Label("Select a Trait Blueprint to edit.") { style = { unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
//                _currentSerializedObject = null;
//                // Debug.Log("[TraitSelectionWindow] DisplayDetails: Selected is null, panel cleared."); // Removed debug logs for final version
//                return;
//            }
//            // Debug.Log($"[TraitSelectionWindow] DisplayDetails: Attempting to display details for: {selected.DefinitionName}"); // Removed debug logs

//            _currentSerializedObject = new SerializedObject(selected);
//            // Debug.Log($"[TraitSelectionWindow] DisplayDetails: SerializedObject created. Target object null? {_currentSerializedObject.targetObject == null}"); // Removed debug logs
            
//            // Create a container for properties and bind it
//            var propertiesContainer = new VisualElement();
//            propertiesContainer.Bind(_currentSerializedObject);
//            _detailPanel.Add(propertiesContainer);

//            var propertyIterator = _currentSerializedObject.GetIterator();
//            bool hasVisibleProperties = propertyIterator.NextVisible(true);
//            // Debug.Log($"[TraitSelectionWindow] DisplayDetails: Has visible properties? {hasVisibleProperties}"); // Removed debug logs

//            if (hasVisibleProperties)
//            {
//                do
//                {
//                    if (propertyIterator.name == "m_Script") continue;

//                    VisualElement currentFieldElement = null;

//                    var fieldInfo = UnityEditor.Toolbox.ToolkitExt.GetFieldInfoForProperty(propertyIterator);
//                    var iconAttribute = fieldInfo?.GetCustomAttribute<DrawWithIconFieldAttribute>();
//                    var selectorAttribute = fieldInfo?.GetCustomAttribute<DrawWithDefinitionSelectorAttribute>();
//                    Type fieldType = fieldInfo?.FieldType;

//                    if (iconAttribute != null)
//                    {
//                        currentFieldElement = new SquareIconField();
//                        (currentFieldElement as SquareIconField).BindProperty(propertyIterator.Copy());
//                    }
//                    else if (selectorAttribute != null && !propertyIterator.isArray)
//                    {
//                        // Check if the fieldType is a BaseDefinition or derived from it
//                        if (fieldType != null && typeof(BaseDefinition).IsAssignableFrom(fieldType))
//                        {
//                            var selector = new DefinitionSelectorField(propertyIterator.displayName, fieldType, enable: false);
//                            selector.BindProperty(propertyIterator.Copy());
//                            currentFieldElement = selector;
//                        }
//                        else
//                        {
//                            // Fallback to default PropertyField if type is not a BaseDefinition
//                            currentFieldElement = new PropertyField(propertyIterator.Copy());
//                        }
//                    }
//                    else
//                    {
//                        var propertyField = new PropertyField(propertyIterator.Copy());
//                        if (propertyIterator.name == "_id") propertyField.SetEnabled(false); // Make _id non-editable
//                        currentFieldElement = propertyField;
//                    }

//                    if (currentFieldElement != null)
//                    {
//                        propertiesContainer.Add(currentFieldElement);
//                    }

//                } while (propertyIterator.NextVisible(false));
//            }
//            else
//            {
//                // Debug.Log($"[TraitSelectionWindow] DisplayDetails: No visible properties found for {selected.DefinitionName}"); // Removed debug logs
//            }
//        }
        


//        private void LoadAllTraitDefinitions()
//        {
//            _allTraitDefinitions = new List<TraitTypeDefinition>();
//            var guids = AssetDatabase.FindAssets("t:TraitTypeDefinition");
//            foreach (var guid in guids)
//            {
//                var path = AssetDatabase.GUIDToAssetPath(guid);
//                var traitDef = AssetDatabase.LoadAssetAtPath<TraitTypeDefinition>(path);
//                if(traitDef != null) _allTraitDefinitions.Add(traitDef);
//            }
//        }

//        private void LoadAllCategories()
//        {
//            _allCategories = new List<TraitCategoryDefinition>();
//            var guids = AssetDatabase.FindAssets("t:TraitCategoryDefinition");
//            foreach (var guid in guids)
//            {
//                var path = AssetDatabase.GUIDToAssetPath(guid);
//                var catDef = AssetDatabase.LoadAssetAtPath<TraitCategoryDefinition>(path);
//                if(catDef != null) _allCategories.Add(catDef);
//            }
//            _allCategories = _allCategories.OrderBy(c => c.DefinitionName).ToList();
//        }

//        private void AddListViewButtons()
//        {
//            var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 5 } };
//            var btnNew = new Button(CreateNewTraitAsset) { text = "N" };
//            buttonContainer.Add(btnNew);
//            var btnDelete = new Button(DeleteSelectedTraitAsset) { text = "D" };
//            buttonContainer.Add(btnDelete);
//            _listPanel.Add(buttonContainer);
//        }

//        private void CreateNewTraitAsset()
//        {
//            string directory = "Assets/Game/TraitTypes";
//            if (!AssetDatabase.IsValidFolder(directory))
//            {
//                AssetDatabase.CreateFolder("Assets/Game", "TraitTypes");
//            }
//            var newAsset = ScriptableObject.CreateInstance<TraitTypeDefinition>();
//            string path = AssetDatabase.GenerateUniqueAssetPath($"{directory}/New TraitType.asset");
//            AssetDatabase.CreateAsset(newAsset, path);
            
//            var so = new SerializedObject(newAsset);
//            so.FindProperty("_category").objectReferenceValue = _currentSelectedCategory;
//            so.FindProperty("_definitionName").stringValue = newAsset.name;
//            so.ApplyModifiedProperties();
            
//            AssetDatabase.SaveAssets();
//            LoadAllTraitDefinitions();
//            FilterAndRebuild();

//            // Select the new asset in the list
//            int newIndex = _filteredTraitDefinitions.IndexOf(newAsset);
//            if (newIndex >= 0)
//            {
//                _traitListView.selectedIndex = newIndex;
//            }
            
//            Selection.activeObject = newAsset;
//        }

//        private void DeleteSelectedTraitAsset()
//        {
//            if (_selectedTrait == null) return;
//            if (EditorUtility.DisplayDialog("Delete Trait Blueprint?", $"Delete '{_selectedTrait.DefinitionName}'?", "Delete", "Cancel"))
//            {
            
//                string path = AssetDatabase.GetAssetPath(_selectedTrait);
//                AssetDatabase.DeleteAsset(path);
//                AssetDatabase.SaveAssets();
                
//                _selectedTrait = null;
//                LoadAllTraitDefinitions();
//                FilterAndRebuild();
//                DisplayDetails(null);
//            }
//        }

//        private void OnSelectButtonClicked()
//        {
//            if (_selectedTrait != null)
//            {
//                _onTraitSelected?.Invoke(_selectedTrait);
//            }
//            Close();
//        }
//    }
//}