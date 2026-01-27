//#if UNITY_EDITOR
//using System;
//using UnityEngine;
//using UnityEngine.Toolbox;
//using UnityEditor.UIElements;
//using UnityEngine.UIElements;

//namespace UnityEditor.Toolbox
//{
//    public class AssetOperationPopup : VisualElement
//    {
//        private ListView _listView;
//        private SerializedProperty _listProperty;
//        private Action _refreshList;
//        private UnityEngine.Object _asset; // Add this field to store the asset

//        // Constructor to initialize the popup with necessary data
//        public AssetOperationPopup(
//            VisualElement root,
//            SerializedProperty elementProperty,
//            ListView listView,
//            Vector2 position,
//            SerializedProperty listProperty,
//            Action refreshList)
//        {
//            _listView = listView;
//            _listProperty = listProperty;
//            _refreshList = refreshList;
//            _asset = elementProperty.objectReferenceValue;

//            // Setup the backdrop
//            name = "rename-popup-backdrop";
//            style.position = Position.Absolute;
//            style.top = 0;
//            style.left = 0;
//            style.right = 0;
//            style.bottom = 0;
//            style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.2f));

//            RegisterCallback<PointerDownEvent>(evt =>
//            {
//                if (evt.target == this) // Check if click is on backdrop itself
//                {
//                    root.Remove(this);
//                }
//            });

//            // Setup the popup content
//            var popup = new VisualElement
//            {
//                name = "rename-popup",
//                style =
//                {
//                    position = Position.Absolute,
//                    left = position.x,
//                    top = position.y,
//                    minWidth = 200,
//                    backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f)),
//                }
//            };
//            popup.SetBorderColor(Color.black);
//            popup.SetBorderWidth(1);
//            popup.SetPadding(5);
//            Add(popup); // Add popup to the backdrop

//            var mainContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
//            popup.Add(mainContainer);

//            var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Column, marginRight = 10 } };
//            mainContainer.Add(buttonContainer);

//            var contentContainer = new VisualElement { style = { flexGrow = 1 } };
//            mainContainer.Add(contentContainer);

//            var objectField = new ObjectField("Asset:") { value = _asset }; // Use _asset
//            objectField.SetEnabled(false);
//            contentContainer.Add(objectField);

//            var textField = new TextField("New Name:") { value = _asset.name }; // Use _asset
//            contentContainer.Add(textField);

//            var confirmButton = new Button(() =>
//            {
//                string newName = textField.value;
//                if (newName != _asset.name && !string.IsNullOrEmpty(newName)) // Use _asset
//                {
//                    string path = AssetDatabase.GetAssetPath(_asset); // Use _asset
//                    var so = new SerializedObject(_asset); // Use _asset
//                    var defNameProp = so.FindProperty("_definitionName");
//                    if (defNameProp != null)
//                    {
//                        defNameProp.stringValue = newName;
//                        so.ApplyModifiedProperties();
//                    }
//                    AssetDatabase.RenameAsset(path, newName);
//                    AssetDatabase.SaveAssets();
//                    _listView.Rebuild();
//                }
//                root.Remove(this);
//            })
//            { text = "Confirm" };

//            var cloneButton = new Button(() =>
//            {
//                string originalPath = AssetDatabase.GetAssetPath(_asset); // Use _asset
//                string newPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

//                if (AssetDatabase.CopyAsset(originalPath, newPath))
//                {
//                    var clonedAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newPath);
//                    if (clonedAsset != null)
//                    {
//                        _listProperty.serializedObject.Update();
//                        _listProperty.InsertArrayElementAtIndex(_listProperty.arraySize);
//                        var newElement = _listProperty.GetArrayElementAtIndex(_listProperty.arraySize - 1);
//                        newElement.objectReferenceValue = clonedAsset;
//                        _listProperty.serializedObject.ApplyModifiedProperties();
//                        _refreshList?.Invoke();
//                    }
//                }
//                root.Remove(this);
//            })
//            { text = "Clone" };

//            var cancelButton = new Button(() => root.Remove(this)) { text = "Cancel" };

//            buttonContainer.Add(confirmButton);
//            buttonContainer.Add(cloneButton);
//            buttonContainer.Add(cancelButton);

//            textField.Focus();
//        }

//        // Static method to show the popup
//        public static void Show(
//            VisualElement root,
//            SerializedProperty elementProperty,
//            ListView listView,
//            Vector2 position,
//            SerializedProperty listProperty,
//            Action refreshList)
//        {
//            // Remove any existing backdrop to ensure only one popup is active
//            var existingBackdrop = root.Q<AssetOperationPopup>();
//            if (existingBackdrop != null)
//            {
//                root.Remove(existingBackdrop);
//            }

//            var popup = new AssetOperationPopup(root, elementProperty, listView, position, listProperty, refreshList);
//            root.Add(popup);
//        }
//    }
//}
//#endif
