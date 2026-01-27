using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Toolbox;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEngine.DataEditor
{
    public class DefinitionSelectionWindow : EditorWindow
    {
        private Action<BaseDefinition> _onSelectionMade;
        private Type _definitionType;
        private List<BaseDefinition> _allDefinitions;
        private List<BaseDefinition> _filteredDefinitions;

        private TextField _searchField;
        private ListView _definitionListView;
        private ScrollView _detailView;
        private BaseDefinition _currentSelectedItem;

        public static void ShowWindow(Type definitionType, Action<BaseDefinition> onSelectionMade, string title = "Select Definition")
        {
            var window = GetWindow<DefinitionSelectionWindow>(true, title, true);
            window._definitionType = definitionType;
            window._onSelectionMade = onSelectionMade;
            window.minSize = new Vector2(600, 400);
            window.PopulateDefinitions();
            window.CreateGUI();
        }

        private void PopulateDefinitions()
        {
            _allDefinitions = new List<BaseDefinition>();
            string[] guids = AssetDatabase.FindAssets("t:" + _definitionType.Name);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath(path, _definitionType) as BaseDefinition;
                if (def != null)
                {
                    _allDefinitions.Add(def);
                }
            }
            _filteredDefinitions = new List<BaseDefinition>(_allDefinitions);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var twoPanelsContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
            root.Add(twoPanelsContainer);

            var leftPanel = new VisualElement { name = "left-panel", style = { width = new Length(40, LengthUnit.Percent), borderRightWidth = 1, borderRightColor = Color.gray } };
            twoPanelsContainer.Add(leftPanel);

            _searchField = new TextField("Search...");
            _searchField.RegisterValueChangedCallback(OnSearchTextChanged);
            leftPanel.Add(_searchField);

            _definitionListView = new ListView();
            _definitionListView.itemsSource = _filteredDefinitions;
            _definitionListView.makeItem = () =>
            {
                var label = new Label();
                label.style.textOverflow = TextOverflow.Ellipsis;
                label.style.whiteSpace = WhiteSpace.NoWrap;
                label.style.flexShrink = 1;
                return label;
            };
            _definitionListView.bindItem = (element, i) =>
            {
                if (element is Label label && i < _filteredDefinitions.Count)
                {
                    label.text = _filteredDefinitions[i].ToString();
                }
            };
            _definitionListView.selectionType = SelectionType.Single;
            _definitionListView.selectionChanged += OnDefinitionSelected;
            _definitionListView.horizontalScrollingEnabled = false;
            leftPanel.Add(_definitionListView);

            _detailView = new ScrollView(ScrollViewMode.Vertical) { name = "detail-view", style = { flexGrow = 1, paddingLeft = 5, paddingRight = 5, paddingTop = 5, paddingBottom = 5 } };
            _detailView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            twoPanelsContainer.Add(_detailView);

            _definitionListView.Rebuild();
            DisplayDetails(null);
        }

        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            _filteredDefinitions.Clear();
            if (string.IsNullOrEmpty(evt.newValue))
            {
                _filteredDefinitions.AddRange(_allDefinitions);
            }
            else
            {
                string lowerSearch = evt.newValue.ToLower();
                _filteredDefinitions.AddRange(_allDefinitions.Where(def => def.ToString().ToLower().Contains(lowerSearch)));
            }
            _definitionListView.Rebuild();
        }

        private void OnDefinitionSelected(IEnumerable<object> selectedItems)
        {
            _currentSelectedItem = selectedItems.FirstOrDefault() as BaseDefinition;
            DisplayDetails(_currentSelectedItem);
        }

        private void DisplayDetails(BaseDefinition item)
        {
            _detailView.Clear();

            if (item == null)
            {
                _detailView.Add(new Label("Select an item to see details")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = Color.gray, flexGrow = 1 }
                });
                return;
            }

            var buttonPanel = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 5 } };

            var selectButton = new Button(() =>
            {
                _onSelectionMade?.Invoke(_currentSelectedItem);
                Close();
            })
            { text = "Выбрать", style = { flexGrow = 1 } };

            var gotoButton = new Button(() =>
            {
                SwitchToDefinitionTab();
            })
            { text = "Перейти", style = { flexGrow = 1 } };

            buttonPanel.Add(selectButton);
            buttonPanel.Add(gotoButton);
            _detailView.Add(buttonPanel);

            var serializedObject = new SerializedObject(item);
            var property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    if (property.name == "m_Script") continue;

                    VisualElement field;
                    if (property.name == "_icon")
                    {
                        field = new SquareIconField();
                        (field as SquareIconField).BindProperty(property.Copy());
                    }
                    else
                    {
                        field = new PropertyField(property.Copy());
                    }

                    field.SetEnabled(false);
                    _detailView.Add(field);

                } while (property.NextVisible(false));
            }
        }

        private void SwitchToDefinitionTab()
        {
            if (_currentSelectedItem == null) return;

            var dataEditor = GetWindow<DataEditorWindow>();
            if (dataEditor != null)
            {
                dataEditor.SwitchToTab(_currentSelectedItem.GetType());
            }
            Close();
        }
    }
}
