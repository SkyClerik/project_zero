using System;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Toolbox;
using UnityEngine.DataEditor;
using UnityEngine;

namespace UnityEditor.DataEditor
{
    public class UnitDetailPanel : VisualElement
    {
        private SerializedObject _currentSerializedObject;
        private UnitBaseDefinition _currentDefinition;
        private EventCallback<SerializedPropertyChangeEvent> _definitionNameChangeHandler;
        private Action _onDefinitionNameChangedCallback;

        public UnitDetailPanel(Action onDefinitionNameChangedCallback)
        {
            _onDefinitionNameChangedCallback = onDefinitionNameChangedCallback;
            style.flexGrow = 1;
            style.paddingLeft = 5;
        }

        public void DisplayDetails(BaseDefinition selected)
        {
            Clear();
            _currentDefinition = selected as UnitBaseDefinition;

            if (_definitionNameChangeHandler != null && _currentSerializedObject != null)
            {
                this.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
            }

            if (_currentDefinition == null)
            {
                Add(new Label("Выберите юнита из списка для редактирования.")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = new Color(0.46f, 0.46f, 0.46f), flexGrow = 1, unityFontStyleAndWeight = FontStyle.Italic }
                });
                _currentSerializedObject = null;
                return;
            }

            _currentSerializedObject = new SerializedObject(_currentDefinition);

            var propertiesContainer = new VisualElement();
            propertiesContainer.Bind(_currentSerializedObject);
            propertiesContainer.style.flexGrow = 1;

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            scrollView.Add(propertiesContainer);
            Add(scrollView);
            this.style.flexGrow = 1;

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
                        if (fieldType != null && typeof(BaseDefinition).IsAssignableFrom(fieldType))
                        {
                            var selector = new DefinitionSelectorField(propertyIterator.displayName, fieldType, 64);
                            selector.BindProperty(propertyIterator.Copy());
                            currentFieldElement = selector;
                        }
                        else
                        {
                            currentFieldElement = new PropertyField(propertyIterator.Copy());
                        }
                    }
                    else if (propertyIterator.name == "_modifier")
                    {
                        currentFieldElement = new PropertyField(propertyIterator.Copy());
                    }
                    else if (propertyIterator.name == "_skills")
                    {
                        currentFieldElement = CreateAvailableSkillsEditor(propertyIterator.Copy());
                        currentFieldElement.style.flexGrow = 1;
                    }
                    else if (propertyIterator.isArray && fieldType != null && fieldType.IsGenericType && typeof(BaseDefinition).IsAssignableFrom(fieldType.GetGenericArguments()[0]))
                    {
                        var listLabel = new Label(propertyIterator.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
                        propertiesContainer.Add(listLabel);

                        SerializedProperty arrayProperty = propertyIterator.Copy();
                        Type elementType = fieldType.GetGenericArguments()[0];

                        var definitionSelectorField = new DefinitionSelectorField("", elementType, 64);
                        definitionSelectorField.IconField.SetEnabled(false);
                        Func<VisualElement> makeItem = () => definitionSelectorField;
                        Action<VisualElement, int> bindItem = (e, i) =>
                        {
                            (e as DefinitionSelectorField).BindProperty(arrayProperty.GetArrayElementAtIndex(i));
                        };

                        var customListView = UnityEditor.Toolbox.ToolkitExt.CreateCustomListView(arrayProperty, null, null, true, makeItem, bindItem, 96f);
                        customListView.Q<ListView>()?.RefreshItems();
                        customListView.style.flexGrow = 1;
                        currentFieldElement = customListView;
                    }
                    else if (propertyIterator.propertyType == SerializedPropertyType.ObjectReference && fieldType != null && typeof(BaseDefinition).IsAssignableFrom(fieldType))
                    {
                        var selector = new DefinitionSelectorField(propertyIterator.displayName, fieldType, 64);
                        selector.BindProperty(propertyIterator.Copy());
                        currentFieldElement = selector;
                    }
                    else
                    {
                        var propertyField = new PropertyField(propertyIterator.Copy());
                        if (propertyIterator.name == "_id") 
                            propertyField.SetEnabled(false);

                        currentFieldElement = propertyField;
                    }

                    if (currentFieldElement != null)
                    {
                        if (_currentDefinition is UnitBaseDefinition && propertyIterator.name == "_prefab")
                            currentFieldElement.SetEnabled(false);

                        propertiesContainer.Add(currentFieldElement);
                    }

                } while (propertyIterator.NextVisible(false));
            }

            _definitionNameChangeHandler = (evt) =>
            {
                if (evt.changedProperty.name == "_definitionName")
                {
                    _currentSerializedObject.ApplyModifiedProperties();
                    _onDefinitionNameChangedCallback?.Invoke();
                }
            };
            this.RegisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
        }

        private VisualElement CreateAvailableSkillsEditor(SerializedProperty skillsProperty)
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;

            var header = new Label(skillsProperty.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold } };

            float recommendedItemHeight = 96f; 

            Func<VisualElement> makeItem = () =>
            {
                var itemRoot = new VisualElement();
                itemRoot.style.flexDirection = FlexDirection.Row;
                itemRoot.style.flexGrow = 1;
                itemRoot.style.alignItems = Align.Center;

                var selector = new DefinitionSelectorField("", typeof(SkillBaseDefinition), 64);
                selector.IconField.SetEnabled(false);
                selector.style.flexGrow = 1;

                itemRoot.Add(selector);
                return itemRoot;
            };

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var selector = e.Q<DefinitionSelectorField>();
                if (selector != null)
                {
                    selector.BindProperty(skillsProperty.GetArrayElementAtIndex(i));
                }
            };

            VisualElement skillsListView = UnityEditor.Toolbox.ToolkitExt.CreateCustomListView(
                property: skillsProperty,
                createNewAssetCallback: null,
                onSelectionChanged: null,
                requireConfirmationOnDelete: true,
                makeItem: makeItem,
                bindItem: bindItem,
                fixedItemHeight: recommendedItemHeight,
                showButtons: false
                );
            
            var addSkillButton = new Button(() =>
            {
                TypeSelectionWindow.ShowWindow(typeof(SkillBaseDefinition), (selectedType) =>
                {
                    skillsProperty.serializedObject.Update();
                    var index = skillsProperty.arraySize;
                    skillsProperty.InsertArrayElementAtIndex(index);
                    var newElement = skillsProperty.GetArrayElementAtIndex(index);
                    newElement.objectReferenceValue = null;
                    skillsProperty.serializedObject.ApplyModifiedProperties();
                    skillsListView.Q<ListView>()?.Rebuild();
                });
            })
            {
                text = "Add Skill from Selector"
            };
            
            container.Add(header);
            container.Add(addSkillButton);
            container.Add(skillsListView);

            container.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                if (evt.changedProperty.propertyPath.StartsWith(skillsProperty.propertyPath))
                {
                    skillsListView.Q<ListView>().Rebuild();
                }
            });

            return container;
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
