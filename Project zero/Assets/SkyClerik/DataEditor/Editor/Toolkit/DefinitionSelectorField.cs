using System;
using UnityEditor;
using UnityEditor.Toolbox;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEngine.DataEditor
{
    public class DefinitionSelectorField : VisualElement
    {
        private readonly ObjectField _objectField;
        private readonly VisualElement _bottomPanel;
        private readonly Label _descriptionLabel;
        private readonly Label _definitionNameLabel;
        private readonly SquareIconField _iconField;
        private readonly Button _selectButton;

        private SerializedProperty _boundProperty;
        private readonly Type _definitionType;

        public SquareIconField IconField => _iconField;

        public DefinitionSelectorField(string label, Type definitionType, float iconSize,  bool enable = true)
        {
            _definitionType = definitionType;

            style.flexDirection = FlexDirection.Column;
            style.minHeight = Length.Auto();
            style.height = Length.Auto(); // Use auto if not specified

            var topPanel = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            _objectField = new ObjectField(label)
            {
                objectType = _definitionType,
                style = { flexGrow = 1 }
            };
            _objectField.RegisterValueChangedCallback(evt => this.UpdateDisplay(evt.newValue));
            _objectField.SetEnabled(enable); // Set enabled state for ObjectField
            topPanel.Add(_objectField);

            _selectButton = new Button(() =>
            {
                DefinitionSelectionWindow.ShowWindow(_definitionType, (selectedDef) =>
                {
                    if (_boundProperty != null)
                    {
                        _boundProperty.objectReferenceValue = selectedDef;
                        _boundProperty.serializedObject.ApplyModifiedProperties();
                    }
                }, "Select " + label);
            })
            { text = "..." };
            _selectButton.style.width = 30;
            // Set display style for the button so it's completely hidden when disabled
            _selectButton.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
            topPanel.Add(_selectButton);

            Add(topPanel);

            _bottomPanel = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 2, borderTopWidth = 1, borderTopColor = Color.gray, paddingTop = 2 } };
            Add(_bottomPanel);

            var leftPanel = new VisualElement { style = { flexGrow = 1 } };
            _bottomPanel.Add(leftPanel);

            _definitionNameLabel = new Label();
            _definitionNameLabel.style.whiteSpace = WhiteSpace.Normal; // Ensure text wraps
            leftPanel.Add(_definitionNameLabel);
            _descriptionLabel = new Label();
            _descriptionLabel.style.whiteSpace = WhiteSpace.Normal; // Ensure text wraps
            leftPanel.Add(_descriptionLabel);

            _iconField = new SquareIconField(iconSize);
            _iconField.SetEnabled(enable); // Set enabled state for SquareIconField
            _bottomPanel.Add(_iconField);

            UpdateDisplay(null);
        }

        public void BindProperty(SerializedProperty property)
        {
            _boundProperty = property;
            _objectField.BindProperty(property);
            UpdateDisplay(property.objectReferenceValue);
        }

        private void UpdateDisplay(UnityEngine.Object targetObject)
        {
            if (targetObject == null || !(targetObject is BaseDefinition def))
            {
                _bottomPanel.style.display = DisplayStyle.None;
                return;
            }

            _bottomPanel.style.display = DisplayStyle.Flex;

            _descriptionLabel.text = "Description: " + def.Description;
            _definitionNameLabel.text = "DefinitionName: " + def.DefinitionName;

            var so = new SerializedObject(def);
            var iconProp = so.FindProperty("_icon");
            if (iconProp != null)
            {
                _iconField.BindProperty(iconProp);
                _iconField.style.display = DisplayStyle.Flex;
            }
            else
            {
                _iconField.style.display = DisplayStyle.None;
            }
        }
    }
}