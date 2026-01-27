using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;
using System.Linq;

namespace UnityEditor.DataEditor
{
    public class ModifierListItem : VisualElement
    {
        public ModifierListItem()
        {
            this.SetPadding(5);
            style.borderBottomWidth = 1;
            style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
        }

        public void BindProperty(SerializedProperty property)
        {
            Clear();

            var typeName = "Unknown";
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var fullTypeName = property.managedReferenceFullTypename;
                if (!string.IsNullOrEmpty(fullTypeName))
                {
                    var parts = fullTypeName.Split(' ');
                    var className = parts.Length > 1 ? parts.Last() : fullTypeName;
                    typeName = className.Split('.').Last();
                }
            }
            Add(new Label(typeName) { style = { unityFontStyleAndWeight = FontStyle.Bold, paddingBottom = 5 } });

            if (property.managedReferenceValue != null)
            {
                SerializedProperty managedReferenceProperty = property.Copy();

                if (managedReferenceProperty.NextVisible(true))
                {
                    do
                    {
                        if (managedReferenceProperty.depth < property.depth || SerializedProperty.EqualContents(managedReferenceProperty, property))
                            break;

                        Add(new PropertyField(managedReferenceProperty.Copy()));
                    }
                    while (managedReferenceProperty.NextVisible(false));
                }
            }
            else
            {
                Add(new Label("Нет модификатора") { style = { unityFontStyleAndWeight = FontStyle.Italic } });
            }

            this.Bind(property.serializedObject);
        }
    }
}