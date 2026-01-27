using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Toolbox;
using UnityEngine.DataEditor;

namespace UnityEditor.DataEditor
{
    public delegate void DefinitionSelectedHandler(BaseDefinition selectedDefinition);
    public delegate BaseDefinition CreateNewDefinitionAssetHandler();

    public class ListViewPanel<TDefinition, TDatabase> : VisualElement
        where TDefinition : BaseDefinition
        where TDatabase : ScriptableObject, IDefinitionDatabase<TDefinition>
    {
        public event DefinitionSelectedHandler OnDefinitionSelected;

        private VisualElement _listViewContainer;
        private SerializedProperty _itemsProperty;
        private SerializedObject _serializedDatabase;

        private CreateNewDefinitionAssetHandler _createNewAssetHandler;

        public ListViewPanel(VisualElement container, CreateNewDefinitionAssetHandler createNewAssetHandler, bool showButtons = true)
        {
            _createNewAssetHandler = createNewAssetHandler;

            style.flexGrow = 1;

            var database = DataEditorWindow.LoadOrCreateDatabase<TDatabase>();
            _serializedDatabase = new SerializedObject(database);
            _itemsProperty = _serializedDatabase.FindProperty("_items");

            _listViewContainer = ToolkitExt.CreateCustomListView(
                _itemsProperty,
                AddNewDefinition,
                (selectedProp) => OnDefinitionSelected?.Invoke(selectedProp?.objectReferenceValue as BaseDefinition),
                showButtons: showButtons,
                makeItem: MakeDefinitionListItem,
                bindItem: BindDefinitionListItem
            );
            _listViewContainer.style.flexGrow = 1;

            container.Add(_listViewContainer);
            _listViewContainer.Q<ListView>()?.Rebuild();
        }

        private VisualElement MakeDefinitionListItem()
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.alignItems = Align.Center;
            root.style.height = 40;

            var dragHandle = new Label("☰") { tooltip = "Перетащите для изменения порядка" };
            dragHandle.style.unityTextAlign = TextAnchor.MiddleLeft;
            dragHandle.style.paddingRight = 5;
            root.Add(dragHandle);

            var icon = new Image { name = "item-icon", scaleMode = ScaleMode.ScaleToFit };
            icon.style.width = 32;
            icon.style.height = 32;
            root.Add(icon);

            var label = new Label { name = "item-label" };
            label.style.flexGrow = 1;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.Add(label);

            var settingsButton = new Button { text = "S", name = "settings-button", tooltip = "Дополнительные настройки" };
            settingsButton.style.display = DisplayStyle.None;
            root.Add(settingsButton);

            return root;
        }

        private void BindDefinitionListItem(VisualElement element, int index)
        {
            var property = _itemsProperty.GetArrayElementAtIndex(index);
            var definition = property?.objectReferenceValue as BaseDefinition;

            var icon = element.Q<Image>("item-icon");
            var label = element.Q<Label>("item-label");
            // var settingsButton = element.Q<Button>("settings-button"); // Not used for now

            if (definition != null)
            {
                label.text = string.IsNullOrEmpty(definition.DefinitionName) ? definition.name : definition.DefinitionName;
                icon.sprite = definition.Icon;
            }
            else
            {
                label.text = "Элемент не найден (NULL)";
                icon.sprite = null;
            }
        }

        private BaseDefinition AddNewDefinition()
        {
            if (_createNewAssetHandler != null)
            {
                var newAsset = _createNewAssetHandler();
                if (newAsset != null)
                {
                    _serializedDatabase.ApplyModifiedProperties();
                    _listViewContainer.Q<ListView>()?.Rebuild();
                    _listViewContainer.Q<ListView>()?.SetSelection(_itemsProperty.arraySize - 1);
                    return newAsset;
                }
            }
            return null;
        }

        public void RemoveSelectedDefinition()
        {
            int selectedIndex = _listViewContainer.Q<ListView>().selectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _itemsProperty.arraySize)
            {
                var selectedDef = _itemsProperty.GetArrayElementAtIndex(selectedIndex).objectReferenceValue as TDefinition;
                if (EditorUtility.DisplayDialog("Delete Definition?", $"Delete '{selectedDef.DefinitionName}'?", "Delete", "Cancel"))
                {
                    string path = AssetDatabase.GetAssetPath(selectedDef);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();

                    _itemsProperty.DeleteArrayElementAtIndex(selectedIndex);
                    _serializedDatabase.ApplyModifiedProperties();
                    _listViewContainer.Q<ListView>()?.Rebuild();
                    OnDefinitionSelected?.Invoke(null);
                }
            }
        }

        public void RebuildList()
        {
            _serializedDatabase.Update();
            _listViewContainer.Q<ListView>()?.Rebuild();
        }

        public void SetSelection(TDefinition definition)
        {
            if (definition == null)
            {
                _listViewContainer.Q<ListView>()?.ClearSelection();
                return;
            }

            for (int i = 0; i < _itemsProperty.arraySize; i++)
            {
                if (_itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue == definition)
                {
                    _listViewContainer.Q<ListView>()?.SetSelection(i);
                    return;
                }
            }
            _listViewContainer.Q<ListView>()?.ClearSelection();
        }
    }
}
