using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor.Toolbox;
using UnityEngine.DataEditor;

namespace UnityEditor.DataEditor
{
    public class ItemsTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;
        private ScrollView _detailView;
        private BaseDefinition _currentSelectedItem;
        private SerializedObject _currentSerializedObject;
        private EventCallback<SerializedPropertyChangeEvent> _definitionNameChangeHandler;
        private Type _activeItemType;
        private readonly Dictionary<Type, VisualElement> _tabContents = new Dictionary<Type, VisualElement>();
        private VisualElement _currentActiveTabContent;
        private TabButtonGroup _innerTabButtonGroup; // Added for internal tabs

        // Specific Item Databases and their SerializedObject counterparts
        private ConsumableItemDatabase _consumableItemDatabase;
        private SerializedObject _serializedConsumableItemDatabase;
        private KeyItemDatabase _keyItemDatabase;
        private SerializedObject _serializedKeyItemDatabase;
        private ArmorDatabase _armorDatabase;
        private SerializedObject _serializedArmorDatabase;
        private WeaponDatabase _weaponDatabase;
        private SerializedObject _serializedWeaponDatabase;
        private AccessoryDatabase _accessoryDatabase;
        private SerializedObject _serializedAccessoryDatabase;
        private Dictionary<Type, SerializedObject> _itemTypeToSerializedDatabase;


        public ItemsTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            _tabContents.Clear(); // Clear dictionary on reload

            var root = new VisualElement { name = "items-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement { name = "left-panel", style = { flexGrow = 1 } };
            splitView.Add(leftPanel);

            _detailView = new ScrollView(ScrollViewMode.Vertical) { name = "item-detail-view", style = { flexGrow = 1, paddingLeft = 5 } };
            splitView.Add(_detailView);

            var listContainer = new VisualElement { name = "item-lists-container", style = { flexGrow = 1 } };
            leftPanel.Add(listContainer);

            // Load all specific item databases
            _consumableItemDatabase = DataEditorWindow.LoadOrCreateDatabase<ConsumableItemDatabase>();
            _serializedConsumableItemDatabase = new SerializedObject(_consumableItemDatabase);

            _keyItemDatabase = DataEditorWindow.LoadOrCreateDatabase<KeyItemDatabase>();
            _serializedKeyItemDatabase = new SerializedObject(_keyItemDatabase);

            _armorDatabase = DataEditorWindow.LoadOrCreateDatabase<ArmorDatabase>();
            _serializedArmorDatabase = new SerializedObject(_armorDatabase);

            _weaponDatabase = DataEditorWindow.LoadOrCreateDatabase<WeaponDatabase>();
            _serializedWeaponDatabase = new SerializedObject(_weaponDatabase);

            _accessoryDatabase = DataEditorWindow.LoadOrCreateDatabase<AccessoryDatabase>();
            _serializedAccessoryDatabase = new SerializedObject(_accessoryDatabase);

            // Map item types to their serialized databases
            _itemTypeToSerializedDatabase = new Dictionary<Type, SerializedObject>
            {
                { typeof(ConsumableItemDefinition), _serializedConsumableItemDatabase },
                { typeof(KeyItemDefinition), _serializedKeyItemDatabase },
                { typeof(ArmorDefinition), _serializedArmorDatabase },
                { typeof(WeaponDefinition), _serializedWeaponDatabase },
                { typeof(AccessoryDefinition), _serializedAccessoryDatabase }
            };

            var itemTypes = new List<Type>
                {
                    typeof(ConsumableItemDefinition), typeof(KeyItemDefinition), typeof(ArmorDefinition), typeof(WeaponDefinition), typeof(AccessoryDefinition)
                };

            var tabNames = new List<string> { "Обычные", "Ключевые", "Броня", "Оружие", "Аксессуары" };

            var innerTabButtonsList = new List<Button>();

            for (int i = 0; i < itemTypes.Count; i++)
            {
                var itemType = itemTypes[i];
                var tabName = tabNames[i];

                var tabButton = new Button { text = tabName, userData = itemType };
                innerTabButtonsList.Add(tabButton);

                var propertyName = GetPropertyNameForType(itemType);
                if (string.IsNullOrEmpty(propertyName)) continue;

                // Get the correct serialized database based on itemType
                if (!_itemTypeToSerializedDatabase.TryGetValue(itemType, out SerializedObject currentSerializedDatabase))
                {
                    Debug.LogError($"No serialized database found for item type: {itemType.Name}");
                    continue;
                }

                var itemsProperty = currentSerializedDatabase.FindProperty(propertyName);
                if (itemsProperty == null)
                {
                    Debug.LogError($"Property '{propertyName}' not found in database for item type: {itemType.Name}");
                    continue;
                }

                var tabContent = ToolkitExt.CreateCustomListView(
                    itemsProperty,
                    () => CreateNewItemAsset(itemType, GetSubfolderName(itemType)),
                    (selectedProp) => DisplayItemDetails(selectedProp?.objectReferenceValue as ItemBaseDefinition),
                    makeItem: MakeListItem,
                    bindItem: BindListItem
                );

                tabContent.name = $"{itemType.Name.ToLower()}-tab-content";
                tabContent.style.display = DisplayStyle.None;

                _tabContents.Add(itemType, tabContent);
                listContainer.Add(tabContent);
            }

            _innerTabButtonGroup = new TabButtonGroup(innerTabButtonsList, Color.green, new Border4(2, 2, 0, 0), FlexDirection.Row);
            _innerTabButtonGroup.OnSelectedButtonChanged += OnInnerTabButtonChanged;
            root.Insert(0, _innerTabButtonGroup);

            if (innerTabButtonsList.Any())
            {
                _innerTabButtonGroup.SetSelected(innerTabButtonsList.First());
            }
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
            var listView = _currentActiveTabContent.Q<ListView>();
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

        private void SwitchTab(Type type)
        {
            _activeItemType = type;

            if (_currentActiveTabContent != null)
            {
                var prevListView = _currentActiveTabContent.Q<ListView>();
                prevListView?.ClearSelection();
            }

            foreach (var kvp in _tabContents)
            {
                kvp.Value.style.display = (kvp.Key == type) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_tabContents.TryGetValue(type, out var activeTabContent))
            {
                _currentActiveTabContent = activeTabContent;
                _currentActiveTabContent.Q<ListView>()?.Rebuild();
            }
            else
            {
                _currentActiveTabContent = null;
            }

            DisplayItemDetails(null);
        }

        private void OnInnerTabButtonChanged(Button selectedButton)
        {
            if (selectedButton?.userData is Type itemType)
            {
                SwitchTab(itemType);
            }
            else
            {
                Debug.LogWarning("Invalid user data in selected inner tab button.");
            }
        }

        private void DisplayItemDetails(ItemBaseDefinition selectedItem)
        {
            if (_definitionNameChangeHandler != null)
            {
                _detailView.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
                _definitionNameChangeHandler = null;
            }

            _detailView.Clear();
            _currentSelectedItem = selectedItem;

            if (_currentSelectedItem == null)
            {
                _detailView.Add(new Label("Выберите предмет из списка для редактирования.")
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

                    var fieldInfo = ToolkitExt.GetFieldInfoForProperty(propertyIterator);
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
                        Type elementType = fieldType.GetGenericArguments()[0];

                        Func<VisualElement> makeItem = () => new DefinitionSelectorField("", elementType, 32);
                        Action<VisualElement, int> bindItem = (e, i) =>
                        {
                            (e as DefinitionSelectorField).BindProperty(arrayProperty.GetArrayElementAtIndex(i));
                        };

                        currentFieldElement = ToolkitExt.CreateCustomListView(arrayProperty, null, null, true, makeItem, bindItem);
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

            _definitionNameChangeHandler = (evt) =>
            {
                if (evt.changedProperty.name == "_definitionName")
                {
                    _currentSerializedObject.ApplyModifiedProperties();
                    _currentActiveTabContent?.Q<ListView>()?.Rebuild();
                }
            };
            _detailView.RegisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
        }

        private ItemBaseDefinition CreateNewItemAsset(Type itemType, string subfolder = "Items")
        {
            MethodInfo createNewAssetMethod = typeof(DataEditorWindow).GetMethod(nameof(DataEditorWindow.CreateNewAsset));
            if (createNewAssetMethod == null)
            {
                Debug.LogError("CreateNewAsset method not found in DataEditorWindow!");
                return null;
            }

            MethodInfo genericCreateNewAssetMethod = createNewAssetMethod.MakeGenericMethod(itemType);
            var newAsset = genericCreateNewAssetMethod.Invoke(null, new object[] { subfolder }) as ItemBaseDefinition;

            if (newAsset == null) return null;

            var newAssetSO = new SerializedObject(newAsset);

            newAssetSO.FindProperty("_definitionName").stringValue = newAsset.name;
            newAssetSO.ApplyModifiedProperties();

            AssetDatabase.Refresh();
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

        private string GetPropertyNameForType(Type itemType)
        {
            // All DefinitionDatabase<T> instances store their items in a private field named "_items".
            // We need to return this name to correctly find the SerializedProperty.
            return "_items";
        }

        private string GetSubfolderName(Type itemType)
        {
            if (itemType == typeof(ConsumableItemDefinition)) return "Consumables";
            if (itemType == typeof(KeyItemDefinition)) return "KeyItems";
            if (itemType == typeof(ArmorDefinition)) return "Armors";
            if (itemType == typeof(WeaponDefinition)) return "Weapons";
            if (itemType == typeof(AccessoryDefinition)) return "Accessories";
            return "Items";
        }
    }
}