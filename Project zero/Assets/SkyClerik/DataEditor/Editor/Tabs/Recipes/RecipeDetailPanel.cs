using System;
using UnityEditor.UIElements;
using UnityEngine.DataEditor;
using SkyClerik.CraftingSystem;
using UnityEngine.UIElements;
using UnityEngine;

namespace UnityEditor.DataEditor
{
    public class RecipeDetailPanel : VisualElement
    {
        private SerializedObject _currentSerializedObject;
        private CraftingRecipe _currentDefinition;
        private EventCallback<SerializedPropertyChangeEvent> _definitionNameChangeHandler;
        private Action _onRebuildCallback;

        public RecipeDetailPanel(Action onRebuildCallback)
        {
            _onRebuildCallback = onRebuildCallback;
            style.flexGrow = 1;
            style.paddingLeft = 5;
        }

        public void DisplayDetails(BaseDefinition selected)
        {
            Clear();
            _currentDefinition = selected as CraftingRecipe;

            if (_definitionNameChangeHandler != null && _currentSerializedObject != null)
            {
                this.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
            }

            if (_currentDefinition == null)
            {
                Add(new Label("Выберите рецепт из списка для редактирования.")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = new Color(0.46f, 0.46f, 0.46f), flexGrow = 1, unityFontStyleAndWeight = FontStyle.Italic }
                });
                _currentSerializedObject = null;
                return;
            }

            _currentSerializedObject = new SerializedObject(_currentDefinition);

            var propertiesContainer = new VisualElement();
            propertiesContainer.Bind(_currentSerializedObject);
            propertiesContainer.style.flexGrow = 1;

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            scrollView.Add(propertiesContainer);
            Add(scrollView);
            this.style.flexGrow = 1;

            var propertyIterator = _currentSerializedObject.GetIterator();
            bool enterChildren = true;

            while (propertyIterator.NextVisible(enterChildren))
            {
                enterChildren = false; // Для первого элемента NextVisible(true) заходит внутрь, дальше - нет

                // Пропускаем ссылку на скрипт
                if (propertyIterator.name == "m_Script") 
                    continue; 

                // Дополнительная обработка для _id
                if (propertyIterator.name == "_id")
                {
                    var idField = new PropertyField(propertyIterator.Copy());
                    idField.SetEnabled(false); // ID должен быть нередактируемым
                    propertiesContainer.Add(idField);
                    continue;
                }
                
                // Специальная обработка для _ingredients
                if (propertyIterator.name == "_ingredients")
                {
                    var listLabel = new Label(propertyIterator.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } };
                    propertiesContainer.Add(listLabel);

                    SerializedProperty arrayProperty = propertyIterator.Copy();
                    
                    Func<VisualElement> makeItem = () =>
                    {
                        var root = new VisualElement();
                        root.style.flexDirection = FlexDirection.Row;
                        root.style.flexGrow = 1;
                        root.style.alignItems = Align.Center;
                        root.style.paddingLeft = 5;

                        // PropertyField для всего Ingredient
                        var ingredientPropertyField = new PropertyField();
                        ingredientPropertyField.style.flexGrow = 1;
                        root.Add(ingredientPropertyField);
                        return root;
                    };

                    Action<VisualElement, int> bindItem = (e, i) =>
                    {
                        var ingredientProp = arrayProperty.GetArrayElementAtIndex(i); // SerializedProperty для Ingredient
                        // Привязываем PropertyField к текущему SerializedProperty ингредиента
                        (e.Q<PropertyField>()).BindProperty(ingredientProp);
                    };

                    var customListView = UnityEditor.Toolbox.ToolkitExt.CreateCustomListView(
                        property: arrayProperty,
                        createNewAssetCallback: null, // Добавляем через кнопку '+' в ListView
                        onSelectionChanged: null,
                        requireConfirmationOnDelete: true,
                        makeItem: makeItem,
                        bindItem: bindItem,
                        fixedItemHeight: 60f
                    );
                    customListView.style.flexGrow = 1;
                    propertiesContainer.Add(customListView);
                    continue;
                }

                // Специальная обработка для _result
                if (propertyIterator.name == "_result")
                {
                    var resultLabel = new Label(propertyIterator.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } };
                    propertiesContainer.Add(resultLabel);

                    var resultProp = propertyIterator.Copy(); // SerializedProperty для Ingredient
                    var itemProp = resultProp.FindPropertyRelative("_item");
                    var quantityProp = resultProp.FindPropertyRelative("_quantity");

                    var itemField = new ObjectField("Результат") { objectType = typeof(ItemBaseDefinition) };
                    itemField.BindProperty(itemProp);
                    propertiesContainer.Add(itemField);

                    var quantityField = new IntegerField("Количество") { label = "Кол-во" };
                    quantityField.BindProperty(quantityProp);
                    propertiesContainer.Add(quantityField);
                    
                    continue;
                }

                // Отображаем остальные PropertyField по умолчанию
                propertiesContainer.Add(new PropertyField(propertyIterator.Copy()));
            }

            _definitionNameChangeHandler = (evt) =>
            {
                if (evt.changedProperty.name == "_definitionName")
                {
                    _currentSerializedObject.ApplyModifiedProperties();
                    _onRebuildCallback?.Invoke();
                }
            };
            this.RegisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
        }

        public void Unload()
        {
            if (_definitionNameChangeHandler != null && _currentSerializedObject != null)
            {
                this.UnregisterCallback<SerializedPropertyChangeEvent>(_definitionNameChangeHandler);
                _definitionNameChangeHandler = null;
            }
        }
    }
}
