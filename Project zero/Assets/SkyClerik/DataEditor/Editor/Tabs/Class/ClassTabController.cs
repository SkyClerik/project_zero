using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Toolbox;

namespace UnityEngine.DataEditor
{
    public class ClassTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;
        private ScrollView _detailView;
        private BaseDefinition _currentSelectedItem;
        private SerializedObject _currentSerializedObject;
        private EventCallback<SerializedPropertyChangeEvent> _definitionNameChangeHandler;
        private VisualElement _listViewContainer;

        public ClassTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "classes-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _detailView = new ScrollView(ScrollViewMode.Vertical) { name = "class-detail-view", style = { flexGrow = 1, paddingLeft = 5 } };
            splitView.Add(_detailView);

            var database = DataEditorWindow.LoadOrCreateDatabase<ClassDatabase>();
            var serializedDatabase = new SerializedObject(database);
            var itemsProperty = serializedDatabase.FindProperty("_items");

            _listViewContainer = ToolkitExt.CreateCustomListView(
                itemsProperty,
                () => CreateNewClassAsset(),
                (selectedProp) => DisplayClassDetails(selectedProp?.objectReferenceValue as BaseDefinition),
                makeItem: MakeListItem,
                bindItem: BindListItem
            );

            leftPanel.Add(_listViewContainer);
            _listViewContainer.Q<ListView>()?.Rebuild();

            DisplayClassDetails(null);
        }

        private VisualElement MakeListItem()
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.alignItems = Align.Center;
            root.style.paddingTop = 2;
            root.style.paddingBottom = 2;

            var dragHandle = new Label("☰") { tooltip = "Перетащите для изменения порядка" };
            dragHandle.style.unityTextAlign = TextAnchor.MiddleLeft;
            dragHandle.style.paddingRight = 5;
            root.Add(dragHandle);

            var icon = new Image { name = "item-icon", scaleMode = ScaleMode.ScaleToFit };
            icon.style.width = 32;
            icon.style.height = 32;
            icon.style.marginRight = 5;
            root.Add(icon);

            var label = new Label { name = "item-label" };
            label.style.flexGrow = 1;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.Add(label);

            var settingsButton = new Button { text = "S", name = "settings-button", tooltip = "Дополнительные настройки" };
            root.Add(settingsButton);

            return root;
        }

        private void BindListItem(VisualElement element, int index)
        {
            var listView = _listViewContainer.Q<ListView>();
            var property = listView.itemsSource[index] as SerializedProperty;
            var definition = property?.objectReferenceValue as BaseDefinition;

            var icon = element.Q<Image>("item-icon");
            var label = element.Q<Label>("item-label");
            var settingsButton = element.Q<Button>("settings-button");

            if (definition != null)
            {
                label.text = string.IsNullOrEmpty(definition.DefinitionName) ? definition.name : definition.DefinitionName;
                icon.sprite = definition.Icon;
            }
            else
            {
                label.text = "Элемент не найден (NULL)";
                icon.sprite = null;
            }

            settingsButton.clicked -= () => OnSettingsButtonClicked(definition);
            settingsButton.clicked += () => OnSettingsButtonClicked(definition);
        }

        private void OnSettingsButtonClicked(BaseDefinition definition)
        {
            if (definition == null) return;
            AdvancedAssetEditorWindow.ShowWindow(definition);
        }

        private void DisplayClassDetails(BaseDefinition selectedItem)
        {
            if (_definitionNameChangeHandler != null) _detailView.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);

            _detailView.Clear();
            _currentSelectedItem = selectedItem;

            if (_currentSelectedItem == null)
            {
                _detailView.Add(new Label("Выберите класс для редактирования.")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = new Color(0.46f, 0.46f, 0.46f), flexGrow = 1, unityFontStyleAndWeight = FontStyle.Italic }
                });
                _currentSerializedObject = null;
                return;
            }

            _currentSerializedObject = new SerializedObject(_currentSelectedItem);

            SerializedProperty propertyIterator = _currentSerializedObject.GetIterator();
            if (propertyIterator.NextVisible(true))
            {
                do
                {
                    if (propertyIterator.name == "m_Script") continue;

                    VisualElement currentFieldElement = null;

                    var fieldInfo = ToolkitExt.GetFieldInfoForProperty(propertyIterator);
                    var drawAttribute = fieldInfo?.GetCustomAttribute<DrawWithDefinitionSelectorAttribute>();

                    if (drawAttribute != null)
                    {
                        if (propertyIterator.isArray)
                        {
                            var listLabel = new Label(propertyIterator.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
                            _detailView.Add(listLabel);

                            string elementTypeName = drawAttribute.DefinitionType.Name;
                            string drawerTypeName = $"{elementTypeName}Field";

                            var drawerType = typeof(ToolkitExt).Assembly.GetTypes().FirstOrDefault(t => t.Name == drawerTypeName);

                            if (drawerType != null && typeof(VisualElement).IsAssignableFrom(drawerType))
                            {
                                SerializedProperty arrayProperty = propertyIterator.Copy();

                                Func<VisualElement> makeItem = () => (VisualElement)Activator.CreateInstance(drawerType);
                                Action<VisualElement, int> bindItem = (e, i) =>
                                {
                                    var itemProp = arrayProperty.GetArrayElementAtIndex(i);
                                    (e as dynamic).BindProperty(itemProp);
                                };

                                currentFieldElement = ToolkitExt.CreateCustomListView(arrayProperty, null, null, makeItem: makeItem, bindItem: bindItem);
                            }
                            else
                            {
                                currentFieldElement = new PropertyField(propertyIterator.Copy());
                            }
                        }
                        else
                        {
                            var selector = new DefinitionSelectorField(propertyIterator.displayName, drawAttribute.DefinitionType, 32);
                            selector.BindProperty(propertyIterator.Copy());
                            currentFieldElement = selector;
                        }
                    }
                    else if (propertyIterator.name == "_icon")
                    {
                        var iconField = new SquareIconField();
                        iconField.BindProperty(propertyIterator.Copy());
                        currentFieldElement = iconField;
                    }
                    else
                    {
                        var propertyField = new PropertyField(propertyIterator.Copy());
                        if (propertyIterator.name == "_id") propertyField.SetEnabled(false);
                        currentFieldElement = propertyField;
                    }

                    if (currentFieldElement != null)
                    {
                        _detailView.Add(currentFieldElement);
                    }

                } while (propertyIterator.NextVisible(false));
            }

            _detailView.Bind(_currentSerializedObject);

            _definitionNameChangeHandler = (evt) =>
            {
                if (evt.changedProperty.name == "_definitionName")
                {
                    _currentSerializedObject.ApplyModifiedProperties();
                    _listViewContainer?.Q<ListView>()?.Rebuild();
                }
            };
            _detailView.RegisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
        }

        private ClassDefinition CreateNewClassAsset()
        {
            var newAsset = DataEditorWindow.CreateNewAsset<ClassDefinition>("Classes");
            if (newAsset == null) return null;

            var newAssetSO = new SerializedObject(newAsset);

            newAssetSO.FindProperty("_definitionName").stringValue = newAsset.name;
            newAssetSO.ApplyModifiedProperties();

            return newAsset;
        }

        public void Unload()
        {
            if (_definitionNameChangeHandler != null && _detailView != null)
            {
                _detailView.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
                _definitionNameChangeHandler = null;
            }
        }
    }
}



