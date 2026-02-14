using System;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Toolbox;
using UnityEngine.DataEditor;
using UnityEngine;

namespace UnityEditor.DataEditor
{
    public class SkillDetailPanel : VisualElement
    {
        private SerializedObject _currentSerializedObject;
        private SkillBaseDefinition _currentDefinition;
        private EventCallback<SerializedPropertyChangeEvent> _definitionNameChangeHandler;
        private Action _onDefinitionNameChangedCallback;

        public SkillDetailPanel(Action onDefinitionNameChangedCallback)
        {
            _onDefinitionNameChangedCallback = onDefinitionNameChangedCallback;
            style.flexGrow = 1;
            style.paddingLeft = 5;
        }

        public void DisplayDetails(BaseDefinition selected)
        {
            Clear();
            _currentDefinition = selected as SkillBaseDefinition;

            if (_definitionNameChangeHandler != null && _currentSerializedObject != null)
            {
                this.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
            }

            if (_currentDefinition == null)
            {
                Add(new Label("Выберите навык для редактирования.")
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
                    VisualElement currentFieldElement = null;

                    var fieldInfo = ToolkitExt.GetFieldInfoForProperty(propertyIterator);
                    var iconAttribute = fieldInfo?.GetCustomAttribute<DrawWithIconFieldAttribute>();

                    if (propertyIterator.name == "m_Script")
                        continue;

                    if (propertyIterator.name == "_icon")
                    {
                        currentFieldElement = new SquareIconField();
                        (currentFieldElement as SquareIconField).BindProperty(propertyIterator.Copy());
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
