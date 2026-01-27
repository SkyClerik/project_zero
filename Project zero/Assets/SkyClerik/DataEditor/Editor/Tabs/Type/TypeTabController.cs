using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Toolbox;
using System.Reflection;

namespace UnityEngine.DataEditor
{
    public class TypeTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;
        private ScrollView _detailView;
        private BaseDefinition _currentSelectedItem;
        private SerializedObject _currentSerializedObject;
        private EventCallback<SerializedPropertyChangeEvent> _itemPropertyChangedHandler;
        private VisualElement _listViewContainer;

        public TypeTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "types-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _detailView = new ScrollView(ScrollViewMode.Vertical) { name = "type-detail-view", style = { flexGrow = 1, paddingLeft = 5 } };
            splitView.Add(_detailView);

            var database = DataEditorWindow.LoadOrCreateDatabase<TypeDatabase>();
            var serializedDatabase = new SerializedObject(database);
            var itemsProperty = serializedDatabase.FindProperty("_items");

            _listViewContainer = UnityEditor.Toolbox.ToolkitExt.CreateCustomListView(
                itemsProperty,
                () => CreateNewTypeAsset(),
                (selectedProp) => DisplayTypeDetails(selectedProp?.objectReferenceValue as BaseDefinition),
                true,
                MakeListItem,
                BindListItem
            );

            leftPanel.Add(_listViewContainer);
            _listViewContainer.Q<ListView>()?.Rebuild();

            DisplayTypeDetails(null);
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

            // Re-subscribe using a lambda that captures the correct definition
            settingsButton.clicked -= () => OnSettingsButtonClicked(definition);
            settingsButton.clicked += () => OnSettingsButtonClicked(definition);
        }

        private void OnSettingsButtonClicked(BaseDefinition definition)
        {
            if (definition == null) return;
            AdvancedAssetEditorWindow.ShowWindow(definition);
        }

        private void DisplayTypeDetails(BaseDefinition selectedItem)
        {
            if (_itemPropertyChangedHandler != null) _detailView.UnregisterCallback<SerializedPropertyChangeEvent>(_itemPropertyChangedHandler);

            _detailView.Clear();
            _currentSelectedItem = selectedItem;

            if (_currentSelectedItem == null)
            {
                _detailView.Add(new Label("Выберите тип для редактирования.")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = new Color(0.46f, 0.46f, 0.46f), flexGrow = 1, unityFontStyleAndWeight = FontStyle.Italic }
                });
                _currentSerializedObject = null;
                return;
            }

            _currentSerializedObject = new SerializedObject(_currentSelectedItem);

            var propertyIterator = _currentSerializedObject.GetIterator();
            if (propertyIterator.NextVisible(true))
            {
                do
                {
                    if (propertyIterator.name == "m_Script") continue;

                    VisualElement currentFieldElement = null;

                    var fieldInfo = UnityEditor.Toolbox.ToolkitExt.GetFieldInfoForProperty(propertyIterator);
                    var iconAttribute = fieldInfo?.GetCustomAttribute<DrawWithIconFieldAttribute>();
                    var selectorAttribute = fieldInfo?.GetCustomAttribute<DrawWithDefinitionSelectorAttribute>();
                    Type fieldType = fieldInfo?.FieldType;

                    if (iconAttribute != null)
                    {
                        currentFieldElement = new SquareIconField();
                        (currentFieldElement as SquareIconField).BindProperty(propertyIterator.Copy());
                    }
                    else if (selectorAttribute != null && !propertyIterator.isArray)
                    {
                        var selector = new DefinitionSelectorField(propertyIterator.displayName, selectorAttribute.DefinitionType, 32);
                        selector.BindProperty(propertyIterator.Copy());
                        currentFieldElement = selector;
                    }
                    else if (propertyIterator.isArray && fieldType != null && fieldType.IsGenericType && typeof(BaseDefinition).IsAssignableFrom(fieldType.GetGenericArguments()[0]))
                    {
                        var listLabel = new Label(propertyIterator.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
                        _detailView.Add(listLabel);

                        SerializedProperty arrayProperty = propertyIterator.Copy();
                        Type elementType = fieldType.GetGenericTypeDefinition() == typeof(List<>) ? fieldType.GetGenericArguments()[0] : fieldType.GetElementType();

                        Func<VisualElement> makeItem = () => new DefinitionSelectorField("", elementType, 32);
                        Action<VisualElement, int> bindItem = (e, i) =>
                        {
                            (e as DefinitionSelectorField).BindProperty(arrayProperty.GetArrayElementAtIndex(i));
                        };

                        currentFieldElement = UnityEditor.Toolbox.ToolkitExt.CreateCustomListView(arrayProperty, null, null, true, makeItem, bindItem, 96f); // Pass fixedItemHeight to ListView
                        currentFieldElement.Q<ListView>()?.RefreshItems();
                    }
                    else if (propertyIterator.propertyType == SerializedPropertyType.ObjectReference && fieldType != null && typeof(BaseDefinition).IsAssignableFrom(fieldType))
                    {
                        var selector = new DefinitionSelectorField(propertyIterator.displayName, fieldType, 32);
                        selector.BindProperty(propertyIterator.Copy());
                        currentFieldElement = selector;
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
                        currentFieldElement.Q<ListView>()?.Rebuild();
                    }

                } while (propertyIterator.NextVisible(false));
            }

            _detailView.Bind(_currentSerializedObject);

            _itemPropertyChangedHandler = (evt) =>
            {
                if (evt.changedProperty.name == "_definitionName" || evt.changedProperty.name == "_category")
                {
                    _currentSerializedObject.ApplyModifiedProperties();
                    _listViewContainer?.Q<ListView>()?.Rebuild();
                }
            };
            _detailView.RegisterCallback<SerializedPropertyChangeEvent>(_itemPropertyChangedHandler);
        }

        private TypeDefinition CreateNewTypeAsset()
        {
            var newAsset = DataEditorWindow.CreateNewAsset<TypeDefinition>("Types");
            if (newAsset == null) return null;

            var newAssetSO = new SerializedObject(newAsset);

            newAssetSO.FindProperty("_definitionName").stringValue = newAsset.name;
            newAssetSO.ApplyModifiedProperties();

            return newAsset;
        }

        public void Unload()
        {
            if (_itemPropertyChangedHandler != null && _detailView != null)
            {
                _detailView.UnregisterCallback<SerializedPropertyChangeEvent>(_itemPropertyChangedHandler);
                _itemPropertyChangedHandler = null;
            }
        }
    }
}



