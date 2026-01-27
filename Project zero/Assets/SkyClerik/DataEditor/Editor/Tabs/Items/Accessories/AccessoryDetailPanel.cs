using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Toolbox;
using UnityEngine.DataEditor;

namespace UnityEditor.DataEditor
{
    public class AccessoryDetailPanel : VisualElement
    {
        private SerializedObject _currentSerializedObject;
        private AccessoryDefinition _currentDefinition; // Specific type
        private EventCallback<SerializedPropertyChangeEvent> _definitionNameChangeHandler;
        private Action _onDefinitionNameChangedCallback;

        public AccessoryDetailPanel(Action onDefinitionNameChangedCallback)
        {
            _onDefinitionNameChangedCallback = onDefinitionNameChangedCallback;
            style.flexGrow = 1;
            style.paddingLeft = 5;
        }

        public void DisplayDetails(BaseDefinition selected) // Accepts BaseDefinition for generic ListViewPanel
        {
            Clear();
            _currentDefinition = selected as AccessoryDefinition; // Cast to specific type

            // Unregister previous handler if any
            if (_definitionNameChangeHandler != null && _currentSerializedObject != null)
            {
                this.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
            }

            if (_currentDefinition == null)
            {
                Add(new Label("Выберите аксессуар из списка для редактирования.")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = new Color(0.46f, 0.46f, 0.46f), flexGrow = 1, unityFontStyleAndWeight = FontStyle.Italic }
                });
                _currentSerializedObject = null;
                return;
            }

            _currentSerializedObject = new SerializedObject(_currentDefinition);

            // Create a container for properties and bind it
            var propertiesContainer = new VisualElement();
            propertiesContainer.Bind(_currentSerializedObject);

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            scrollView.Add(propertiesContainer);
            Add(scrollView);

            var propertyIterator = _currentSerializedObject.GetIterator();
            bool hasVisibleProperties = propertyIterator.NextVisible(true);

            if (hasVisibleProperties)
            {
                do
                {
                    if (propertyIterator.name == "m_Script") continue;

                    VisualElement currentFieldElement = null;

                    var fieldInfo = UnityEditor.Toolbox.ToolkitExt.GetFieldInfoForProperty(propertyIterator);
                    var iconAttribute = fieldInfo?.GetCustomAttribute<DrawWithIconFieldAttribute>();
                    var selectorAttribute = fieldInfo?.GetCustomAttribute<DrawWithDefinitionSelectorAttribute>();
                    Type fieldType = fieldInfo?.FieldType;

                    var listAttribute = fieldInfo?.GetCustomAttribute<DrawAsDefinitionListAttribute>();

                    if (iconAttribute != null)
                    {
                        currentFieldElement = new SquareIconField();
                        (currentFieldElement as SquareIconField).BindProperty(propertyIterator.Copy());
                    }
                    else if (selectorAttribute != null && !propertyIterator.isArray)
                    {
                        // Check if the fieldType is a BaseDefinition or derived from it
                        if (fieldType != null && typeof(BaseDefinition).IsAssignableFrom(fieldType))
                        {
                            var selector = new DefinitionSelectorField(propertyIterator.displayName, fieldType, 32);
                            selector.BindProperty(propertyIterator.Copy());
                            currentFieldElement = selector;
                        }
                        else
                        {
                            // Fallback to default PropertyField if type is not a BaseDefinition
                            currentFieldElement = new PropertyField(propertyIterator.Copy());
                        }
                    }
                    //else if (listAttribute != null && propertyIterator.isArray)
                    //{
                    //    SerializedProperty arrayProperty = propertyIterator.Copy();

                    //    // --- Header ---
                    //    var displayNameOverride = fieldInfo?.GetCustomAttribute<DisplayNameOverrideAttribute>();
                    //    string displayName = displayNameOverride != null ? displayNameOverride.DisplayName : propertyIterator.displayName;
                    //    var listLabel = new Label(displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } };
                    //    propertiesContainer.Add(listLabel); // Add header directly to propertiesContainer

                    //    // --- ListView ---
                    //    Func<VisualElement> makeItem = () => new TraitDataField(); // Assuming TraitDataField exists for TraitData
                    //    Action<VisualElement, int> bindItem = (e, i) =>
                    //    {
                    //        (e as TraitDataField).BindProperty(arrayProperty.GetArrayElementAtIndex(i));
                    //    };

                    //    var listView = UnityEditor.Toolbox.ToolkitExt.CreateCustomListView(arrayProperty, null, null, true, makeItem, bindItem, 120f);
                    //    currentFieldElement = listView;

                    //    // Register a callback to rebuild the list on any data change (like deletion)
                    //    this.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
                    //    {
                    //        if (evt.changedProperty.propertyPath.StartsWith(arrayProperty.propertyPath))
                    //        {
                    //            listView.Q<ListView>()?.Rebuild();
                    //        }
                    //    });

                    //    // Initial refresh
                    //    currentFieldElement.Q<ListView>()?.RefreshItems();
                    //}
                    else if (propertyIterator.isArray && fieldType != null && fieldType.IsGenericType && typeof(BaseDefinition).IsAssignableFrom(fieldType.GetGenericArguments()[0]))
                    {
                        var listLabel = new Label(propertyIterator.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
                        propertiesContainer.Add(listLabel); // Add header directly to propertiesContainer

                        SerializedProperty arrayProperty = propertyIterator.Copy();
                        Type elementType = fieldType.GetGenericArguments()[0];

                        Func<VisualElement> makeItem = () => new DefinitionSelectorField("", elementType, 32);
                        Action<VisualElement, int> bindItem = (e, i) =>
                        {
                            (e as DefinitionSelectorField).BindProperty(arrayProperty.GetArrayElementAtIndex(i));
                        };

                        currentFieldElement = UnityEditor.Toolbox.ToolkitExt.CreateCustomListView(arrayProperty, null, null, true, makeItem, bindItem, 96f); // Pass fixedItemHeight to ListView
                        currentFieldElement.Q<ListView>()?.RefreshItems(); // Added to force refresh
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
                        // Специальная логика для AccessoryDetailPanel
                        // if (_currentDefinition is AccessoryDefinition && propertyIterator.name == "SomeSpecificField")
                        // {
                        //     currentFieldElement.SetEnabled(false);
                        // }

                        propertiesContainer.Add(currentFieldElement);
                    }

                } while (propertyIterator.NextVisible(false));
            }

            _definitionNameChangeHandler = (evt) =>
            {
                if (evt.changedProperty.name == "_definitionName")
                {
                    _currentSerializedObject.ApplyModifiedProperties();
                    _onDefinitionNameChangedCallback?.Invoke(); // Notify parent about name change
                }
            };
            this.RegisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
        }

        public void Unload()
        {
            if (_definitionNameChangeHandler != null && _currentSerializedObject != null)
            {
                this.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
                _definitionNameChangeHandler = null;
            }
        }
    }
}
