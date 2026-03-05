using UnityEditor;
using UnityEngine;

namespace SkyClerik
{
    [CustomPropertyDrawer(typeof(TextKeyContainer))]
    public class TextKeyContainerDrawer : PropertyDrawer
    {
        private const int MaxChars = 260;
        private const int LinesMax = 5;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var key = property.FindPropertyRelative("_key");
            var debug = property.FindPropertyRelative("_debugText");

            float h = EditorGUIUtility.singleLineHeight;

            var rect = EditorGUI.PrefixLabel(position, label);

            // Key (однострочное)
            var keyRect = EditorGUI.IndentedRect(rect);
            keyRect.height = h;
            keyRect.y += h;
            EditorGUI.PropertyField(keyRect, key, new GUIContent("Key"));

            // Debug Text РУЧНОЙ TextArea (работает!)
            var textAreaHeight = h * LinesMax;
            var debugRect = EditorGUI.IndentedRect(rect);
            debugRect.height = textAreaHeight;
            debugRect.y += h * 2;

            EditorGUI.LabelField(new Rect(debugRect.x, debugRect.y, debugRect.width, h), "Debug Text");

            var textAreaRect = new Rect(debugRect.x, debugRect.y + h, debugRect.width, textAreaHeight - h);

            EditorGUI.BeginChangeCheck();
            string debugText = EditorGUI.TextArea(textAreaRect, debug.stringValue, EditorStyles.textArea);
            if (EditorGUI.EndChangeCheck())
            {
                debug.stringValue = debugText;
            }

            // предупреждение
            string src = string.IsNullOrEmpty(key?.stringValue) ? debug?.stringValue : key.stringValue;
            if (!string.IsNullOrEmpty(src) && src.Length > MaxChars)
            {
                var helpRect = EditorGUI.IndentedRect(rect);
                helpRect.height = h * 2;
                helpRect.y += h * 2 + textAreaHeight;
                EditorGUI.HelpBox(helpRect, $"Длина: {src.Length}/{MaxChars}", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (LinesMax + 4);
        }
    }
}
