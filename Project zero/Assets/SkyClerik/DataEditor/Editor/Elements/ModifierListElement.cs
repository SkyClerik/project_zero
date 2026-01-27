using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.UIElements;

namespace UnityEditor.DataEditor
{
    public class ModifierListElement : VisualElement
    {
        private readonly SerializedProperty _listProperty;
        private readonly VisualElement _itemsContainer;
        private bool _needsRebuild = true;

        public ModifierListElement(SerializedProperty listProperty)
        {            
            _listProperty = listProperty;
            
            var header = new Label(listProperty.displayName)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, paddingLeft = 5, paddingTop = 2 }
            };
            Add(header);

            _itemsContainer = new VisualElement();
            Add(_itemsContainer);

            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (_needsRebuild)
                {
                    Rebuild();
                    _needsRebuild = false;
                }
            });
            
            RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                if (evt.changedProperty.propertyPath.StartsWith(_listProperty.propertyPath))
                {
                    _needsRebuild = true;
                }
            });

            var footer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, paddingTop = 2, paddingRight = 7 } };
            Add(footer);

            Button customAddButton = null;
            customAddButton = new Button(() =>
            {
                var buttonWorldBound = customAddButton.worldBound;
                TypeSelectionWindow.ShowWindow(typeof(BaseModifier), (selectedType) =>
                {
                    _listProperty.serializedObject.Update();
                    var index = _listProperty.arraySize;
                    _listProperty.InsertArrayElementAtIndex(index);
                    var newElement = _listProperty.GetArrayElementAtIndex(index);
                    newElement.managedReferenceValue = Activator.CreateInstance(selectedType);
                    _listProperty.serializedObject.ApplyModifiedProperties();
                    Rebuild();
                });
            })
            {
                text = "Add Modifier Type"
            };
            footer.Add(customAddButton);
        }

        private void Rebuild()
        {
            _itemsContainer.Clear();
            for (int i = 0; i < _listProperty.arraySize; i++)
            {
                var itemProperty = _listProperty.GetArrayElementAtIndex(i);                
                var itemField = new PropertyField(itemProperty.Copy());                
                itemField.Bind(_listProperty.serializedObject);                
                _itemsContainer.Add(itemField);
            }
        }
    }
}