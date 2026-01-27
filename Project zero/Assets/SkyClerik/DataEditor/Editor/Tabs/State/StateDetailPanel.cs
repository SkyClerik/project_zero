using System;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Toolbox;
using UnityEngine.DataEditor;
using UnityEngine;

namespace UnityEditor.DataEditor
{
    public class StateDetailPanel : VisualElement
    {
        private SerializedObject _currentSerializedObject;
        private StateBaseDefinition _currentDefinition;
        private EventCallback<SerializedPropertyChangeEvent> _definitionNameChangeHandler;
        private Action _onDefinitionNameChangedCallback;

        public StateDetailPanel(Action onDefinitionNameChangedCallback)
        {
            _onDefinitionNameChangedCallback = onDefinitionNameChangedCallback;
            style.flexGrow = 1;
            style.paddingLeft = 5;
        }

        public void DisplayDetails(BaseDefinition selected)
        {
            Clear();
            _currentDefinition = selected as StateBaseDefinition;

            if (_definitionNameChangeHandler != null && _currentSerializedObject != null)
            {
                this.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
            }

            if (_currentDefinition == null)
            {
                Add(new Label("Выберите состояние для редактирования.")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = new Color(0.46f, 0.46f, 0.46f), flexGrow = 1, unityFontStyleAndWeight = FontStyle.Italic }
                });
                _currentSerializedObject = null;
                return;
            }

            _currentSerializedObject = new SerializedObject(_currentDefinition);

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

                    var fieldInfo = ToolkitExt.GetFieldInfoForProperty(propertyIterator);
                    var iconAttribute = fieldInfo?.GetCustomAttribute<DrawWithIconFieldAttribute>();
                    var selectorAttribute = fieldInfo?.GetCustomAttribute<DrawWithDefinitionSelectorAttribute>();

                    if (iconAttribute != null)
                    {
                        currentFieldElement = new SquareIconField();
                        (currentFieldElement as SquareIconField).BindProperty(propertyIterator.Copy());
                    }
                    else if (selectorAttribute != null && !propertyIterator.isArray)
                    {
                        var fieldInfoForSelector = ToolkitExt.GetFieldInfoForProperty(propertyIterator);
                        Type fieldTypeForSelector = fieldInfoForSelector?.FieldType;
                        if (fieldTypeForSelector != null && typeof(BaseDefinition).IsAssignableFrom(fieldTypeForSelector))
                        {
                            var selector = new DefinitionSelectorField(propertyIterator.displayName, fieldTypeForSelector, 32);
                            selector.BindProperty(propertyIterator.Copy());
                            currentFieldElement = selector;
                        }
                        else
                        {
                            currentFieldElement = new PropertyField(propertyIterator.Copy());
                        }
                    }
                    else
                    {
                        var propertyField = new PropertyField(propertyIterator.Copy());
                        if (propertyIterator.name == "_id") propertyField.SetEnabled(false);
                        currentFieldElement = propertyField;
                    }

                    if (currentFieldElement != null)
                    {
                        propertiesContainer.Add(currentFieldElement);
                    }

                } while (propertyIterator.NextVisible(false));
            }

            //var modifierProperty = _currentSerializedObject.FindProperty("_modifier");
            //if (modifierProperty != null)
            //{
            //    Button customAddButton = null;
            //    customAddButton = new Button(() =>
            //    {
            //        var buttonWorldBound = customAddButton.worldBound;
            //        ModifierTypeSelectionWindow.ShowWindow(buttonWorldBound, (selectedType) =>
            //        {
            //            modifierProperty.serializedObject.Update();
            //            var index = modifierProperty.arraySize;
            //            modifierProperty.InsertArrayElementAtIndex(index);
            //            var newElement = modifierProperty.GetArrayElementAtIndex(index);
            //            newElement.managedReferenceValue = Activator.CreateInstance(selectedType);
            //            modifierProperty.serializedObject.ApplyModifiedProperties();
            //        });
            //    })
            //    {
            //        text = "Add Modifier Type"
            //    };
            //    propertiesContainer.Add(customAddButton);
            //}

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
    }
}
