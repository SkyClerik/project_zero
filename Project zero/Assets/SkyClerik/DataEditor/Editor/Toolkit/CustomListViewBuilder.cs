#if UNITY_EDITOR
using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEditor.Toolbox
{
    // Базовый класс для кастомного ListView
    public abstract class CustomListView<TItem> : VisualElement
    {
        protected ListView _listView;
        protected List<TItem> _items;
        protected int _selectedIndex = -1;
        protected VisualElement _lastSelectedElementContainer;

        protected Action _refreshListAction;

        public event Action<TItem> OnSelectionChanged;

        public CustomListView(float? fixedItemHeight = null)
        {
            style.flexDirection = FlexDirection.Column;

            _items = new List<TItem>();
            _listView = new ListView();
            _listView.reorderable = true;
            _listView.selectionType = SelectionType.Single;
            _listView.itemsSource = _items;

            // Default makeItem and bindItem, can be overridden by derived classes or set in constructor
            _listView.makeItem = CreateDefaultItem;
            _listView.bindItem = BindDefaultItem;
            if (fixedItemHeight.HasValue)
            {
                _listView.fixedItemHeight = fixedItemHeight.Value;
            }

            _listView.selectionChanged += OnListViewSelectionChanged;

            Add(_listView);

            _refreshListAction = RefreshList;
        }

        protected virtual VisualElement CreateDefaultItem()
        {
            var itemContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, flexGrow = 1 } };
            var dragHandle = new VisualElement { style = { width = 20, height = 20, backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f)), marginRight = 5 } };
            itemContainer.Add(dragHandle);
            var label = new Label();
            itemContainer.Add(label);
            itemContainer.SetBorderWidth(1);
            itemContainer.SetBorderColor(Color.clear);
            return itemContainer;
        }

        protected virtual void BindDefaultItem(VisualElement element, int i)
        {
            element.userData = i;
            var label = element.Q<Label>();
            if (label != null)
            {
                label.text = _items[i]?.ToString() ?? "None";
            }
        }

        protected void RefreshList()
        {
            PopulateItems();
            _listView.itemsSource = _items;
            _listView.Rebuild();
        }

        protected abstract void PopulateItems();

        private void OnListViewSelectionChanged(IEnumerable<object> selectedEnumerable)
        {
            if (_lastSelectedElementContainer != null)
            {
                _lastSelectedElementContainer.SetBorderColor(Color.clear);
            }

            var selectedItem = selectedEnumerable.FirstOrDefault();

            _selectedIndex = _listView.selectedIndex;
            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
            {
                _lastSelectedElementContainer = _listView.GetRootElementForIndex(_selectedIndex);
                _lastSelectedElementContainer.SetBorderColor(Color.green);
            }
            else
            {
                _lastSelectedElementContainer = null;
            }
            OnSelectionChanged?.Invoke((TItem)selectedItem);
        }

        // Methods to be implemented by derived classes for specific actions
        protected abstract void AddItem();
        protected abstract void RemoveItem();
        protected abstract void CreateNewAssetAndAddItem();
        protected abstract void DeleteAssetAndRemoveItem();
    }

    // Специализированный класс для работы с SerializedProperty
    public class SerializedPropertyListView : CustomListView<SerializedProperty>
    {
        private SerializedProperty _property;
        private Func<UnityEngine.Object> _createNewAssetCallback;
        private bool _requireConfirmationOnDelete;

        public SerializedPropertyListView(
            SerializedProperty property,
            Func<UnityEngine.Object> createNewAssetCallback,
            bool requireConfirmationOnDelete = true,
            Func<VisualElement> makeItem = null,
            Action<VisualElement, int> bindItem = null,
            float? fixedItemHeight = null,
            bool showButtons = true
            ) : base(fixedItemHeight)
        {
            if (property == null || !property.isArray)
            {
                Add(new Label($"{property?.name ?? "null"} is not a valid list property."));
                return;
            }

            _property = property;
            _createNewAssetCallback = createNewAssetCallback;
            _requireConfirmationOnDelete = requireConfirmationOnDelete;

            if (makeItem != null)
            {
                _listView.makeItem = makeItem;
            }
            if (bindItem != null)
            {
                _listView.bindItem = bindItem;
            }

            PopulateItems();
            _listView.itemsSource = _items;

            if(showButtons)
            {
                AddButtonPanel();
            }
        }

        protected override void PopulateItems()
        {
            _items.Clear();
            for (int i = 0; i < _property.arraySize; i++)
            {
                _items.Add(_property.GetArrayElementAtIndex(i));
            }
        }

        protected override void BindDefaultItem(VisualElement element, int i)
        {
            element.userData = i;
            var prop = _items[i] as SerializedProperty;
            var obj = prop?.objectReferenceValue;
            var label = element.Q<Label>();
            if (label != null)
            {
                label.text = obj != null ? obj.ToString() : "None (Object)";
            }
        }


        protected override void AddItem()
        {
            _property.serializedObject.Update();
            _property.InsertArrayElementAtIndex(_property.arraySize);
            var newElement = _property.GetArrayElementAtIndex(_property.arraySize - 1);
            if (newElement.propertyType == SerializedPropertyType.ObjectReference)
                newElement.objectReferenceValue = null;
            _property.serializedObject.ApplyModifiedProperties();
            _refreshListAction();
        }

        protected override void RemoveItem()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _property.arraySize)
            {
                _property.serializedObject.Update();
                var element = _property.GetArrayElementAtIndex(_selectedIndex);
                if (element.propertyType == SerializedPropertyType.ObjectReference)
                    element.objectReferenceValue = null;
                _property.DeleteArrayElementAtIndex(_selectedIndex);
                _property.serializedObject.ApplyModifiedProperties();
                _refreshListAction();
                _selectedIndex = -1;
            }
        }

        protected override void CreateNewAssetAndAddItem()
        {
            if (_createNewAssetCallback != null)
            {
                var newAsset = _createNewAssetCallback();
                if (newAsset != null)
                {
                    _property.serializedObject.Update();
                    _property.InsertArrayElementAtIndex(_property.arraySize);
                    var newElement = _property.GetArrayElementAtIndex(_property.arraySize - 1);
                    newElement.objectReferenceValue = newAsset;
                    _property.serializedObject.ApplyModifiedProperties();
                    _refreshListAction();
                }
            }
        }

        protected override void DeleteAssetAndRemoveItem()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _property.arraySize)
            {
                var element = _property.GetArrayElementAtIndex(_selectedIndex);
                var asset = element.objectReferenceValue;
                if (asset == null) return;

                bool delete = !_requireConfirmationOnDelete || EditorUtility.DisplayDialog(
                        "Delete Asset?",
                        $"Are you sure you want to delete the asset '{asset.name}' and remove it from the list? This action cannot be undone.",
                        "Delete", "Cancel");
                if (delete)
                {
                    string path = AssetDatabase.GetAssetPath(asset);
                    _property.serializedObject.Update();
                    element.objectReferenceValue = null;
                    _property.DeleteArrayElementAtIndex(_selectedIndex);
                    _property.serializedObject.ApplyModifiedProperties();
                    AssetDatabase.DeleteAsset(path);
                    _refreshListAction();
                    _selectedIndex = -1;
                }
            }
        }

        private void AddButtonPanel()
        {
            var buttonPanel = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 5 } };

            if (_createNewAssetCallback != null)
            {
                var btnNew = new Button(CreateNewAssetAndAddItem) { text = "N" };
                buttonPanel.Add(btnNew);

                var btnDeleteAsset = new Button(DeleteAssetAndRemoveItem) { text = "D" };
                buttonPanel.Add(btnDeleteAsset);
            }

            var btnPlus = new Button(AddItem) { text = "+", name = "add-button" };
            buttonPanel.Add(btnPlus);

            var btnMinus = new Button(RemoveItem) { text = "-" };
            buttonPanel.Add(btnMinus);

            Add(buttonPanel);
        }
    }


    public static partial class ToolkitExt
    {
        public static VisualElement CreateCustomListView(
            SerializedProperty property,
            Func<UnityEngine.Object> createNewAssetCallback,
            Action<SerializedProperty> onSelectionChanged,
            bool requireConfirmationOnDelete = true,
            Func<VisualElement> makeItem = null,
            Action<VisualElement, int> bindItem = null,
            float? fixedItemHeight = null,
            bool showButtons = true
            )
        {
            var listView = new SerializedPropertyListView(
                property,
                createNewAssetCallback,
                requireConfirmationOnDelete,
                makeItem,
                bindItem,
                fixedItemHeight,
                showButtons
            );
            listView.OnSelectionChanged += onSelectionChanged;
            return listView;
        }

        public static FieldInfo GetFieldInfoForProperty(SerializedProperty property)
        {
            if (property == null) return null;

            var path = property.propertyPath.Replace(".Array.data[", "[");
            var pathParts = path.Split('.');

            var currentType = property.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = null;

            foreach (var part in pathParts)
            {
                if (currentType == null) return null;

                if (part.EndsWith("]"))
                {
                    var collectionName = part.Substring(0, part.IndexOf("["));
                    fieldInfo = GetFieldRecursive(currentType, collectionName);
                    if (fieldInfo == null) return null;

                    currentType = fieldInfo.FieldType.IsArray
                        ? fieldInfo.FieldType.GetElementType()
                        : fieldInfo.FieldType.GetGenericArguments()[0];
                }
                else
                {
                    fieldInfo = GetFieldRecursive(currentType, part);
                    if (fieldInfo == null) return null;
                    currentType = fieldInfo.FieldType;
                }
            }
            return fieldInfo;
        }

        private static FieldInfo GetFieldRecursive(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }
    }
}
#endif