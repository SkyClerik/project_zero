//#if UNITY_EDITOR
//using UnityEditor.UIElements;
//using UnityEngine.UIElements;

//namespace UnityEditor.Toolbox
//{
//    public class TraitDefinitionField : VisualElement
//    {
//        public TraitDefinitionField()
//        {
//            // Create a container to hold the fields in a row
//            var container = new VisualElement();
//            container.style.flexDirection = FlexDirection.Row;
//            container.style.flexGrow = 1;
//            Add(container);
//        }

//        public void BindProperty(SerializedProperty property)
//        {
//            var container = this[0];
//            container.Clear();

//            // Find the child properties
//            var codeProp = property.FindPropertyRelative("Code");
//            var dataIdProp = property.FindPropertyRelative("DataId");
//            var valueProp = property.FindPropertyRelative("Value");

//            // Create PropertyFields for each child property
//            var codeField = new PropertyField(codeProp, "");
//            codeField.style.flexGrow = 0.4f;

//            var dataIdField = new PropertyField(dataIdProp, "");
//            dataIdField.style.flexGrow = 0.2f;

//            var valueField = new PropertyField(valueProp, "");
//            valueField.style.flexGrow = 0.4f;

//            // Add fields to the container
//            container.Add(codeField);
//            container.Add(dataIdField);
//            container.Add(valueField);
//        }
//    }
//}
//#endif
