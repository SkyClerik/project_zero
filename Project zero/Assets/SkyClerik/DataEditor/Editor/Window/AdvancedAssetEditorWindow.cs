using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UnityEngine.DataEditor
{
    public class AdvancedAssetEditorWindow : EditorWindow
    {
        private SerializedObject _serializedObject;
        private Object _targetObject;

        public static void ShowWindow(Object target)
        {
            AdvancedAssetEditorWindow window = GetWindow<AdvancedAssetEditorWindow>("Advanced Asset Settings");
            window._targetObject = target;
            window._serializedObject = new SerializedObject(target);
            window.minSize = new Vector2(350, 150);
            window.maxSize = new Vector2(350, 150);
        }

        private void CreateGUI()
        {
            if (_serializedObject == null)
            {
                rootVisualElement.Add(new Label("No object selected."));
                return;
            }

            rootVisualElement.style.paddingTop = 10;
            rootVisualElement.style.paddingLeft = 10;
            rootVisualElement.style.paddingRight = 10;

            var nameField = new PropertyField(_serializedObject.FindProperty("m_Name"), "Asset Name");
            rootVisualElement.Add(nameField);

            var buttonContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginTop = 20
                }
            };
            rootVisualElement.Add(buttonContainer);

            var cloneButton = new Button(CloneAsset) { text = "Clone" };
            buttonContainer.Add(cloneButton);

            var applyButton = new Button(ApplyAndClose) { text = "Apply and Close" };
            buttonContainer.Add(applyButton);
        }

        private void ApplyAndClose()
        {
            _serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Close();
        }

        private void CloneAsset()
        {
            if (_targetObject == null) return;

            string originalPath = AssetDatabase.GetAssetPath(_targetObject);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

            if (AssetDatabase.CopyAsset(originalPath, newPath))
            {
                Debug.Log($"Asset cloned to: {newPath}");
            }
            else
            {
                Debug.LogError("Failed to clone asset.");
            }
        }
    }
}
