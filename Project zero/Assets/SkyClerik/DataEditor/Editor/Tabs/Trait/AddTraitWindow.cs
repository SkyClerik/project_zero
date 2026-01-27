//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEditor;
//using UnityEngine.UIElements;

//namespace UnityEngine.DataEditor
//{
//    public class AddTraitWindow : EditorWindow
//    {
//        private Action<TraitTypeDefinition> _onTraitSelected;

//        public static void ShowWindow(Action<TraitTypeDefinition> onTraitSelected)
//        {
//            var window = GetWindow<AddTraitWindow>("Add Trait");
//            window._onTraitSelected = onTraitSelected;
//            window.minSize = new Vector2(300, 400);
//        }

//        private void CreateGUI()
//        {
//            rootVisualElement.Clear();

//            var createButton = new Button(() =>
//            {
//                var newAsset = DataEditorWindow.CreateNewAsset<TraitTypeDefinition>("TraitTypes");
//                if (newAsset != null)
//                {
//                    var so = new SerializedObject(newAsset);
//                    so.FindProperty("_definitionName").stringValue = newAsset.name;
//                    so.ApplyModifiedProperties();
//                }

//                // Re-render the GUI to show the new asset
//                CreateGUI();
//            })
//            {
//                text = "Создать новый тип",
//                style = { marginBottom = 10 }
//            };
//            rootVisualElement.Add(createButton);

//            var allTraitTypes = FindAllTraitTypes();

//            var scrollView = new ScrollView(ScrollViewMode.Vertical);
//            rootVisualElement.Add(scrollView);

//            if (!allTraitTypes.Any())
//            {
//                scrollView.Add(new Label("Не найдено ни одного 'чертежа' особенностей (TraitTypeDefinition).\nСоздайте новый, нажав на кнопку выше."));
//                return;
//            }

//            foreach (var traitType in allTraitTypes)
//            {
//                var button = new Button(() =>
//                {
//                    _onTraitSelected?.Invoke(traitType);
//                    Close();
//                })
//                {
//                    style =
//                    {
//                        flexDirection = FlexDirection.Row,
//                        alignItems = Align.Center,
//                        paddingTop = 5,
//                        paddingBottom = 5,
//                        height = 40
//                    }
//                };

//                var icon = new Image { scaleMode = ScaleMode.ScaleToFit, style = { width = 32, height = 32, marginRight = 10 } };
//                if (traitType.Icon != null)
//                {
//                    icon.image = traitType.Icon.texture;
//                }

//                var label = new Label(string.IsNullOrEmpty(traitType.DefinitionName) ? traitType.name : traitType.DefinitionName);

//                button.Add(icon);
//                button.Add(label);

//                scrollView.Add(button);
//            }
//        }

//        private static IEnumerable<TraitTypeDefinition> FindAllTraitTypes()
//        {
//            return AssetDatabase.FindAssets($"t:{nameof(TraitTypeDefinition)}")
//                .Select(AssetDatabase.GUIDToAssetPath)
//                .Select(AssetDatabase.LoadAssetAtPath<TraitTypeDefinition>)
//                .Where(t => t != null);
//        }
//    }
//}
