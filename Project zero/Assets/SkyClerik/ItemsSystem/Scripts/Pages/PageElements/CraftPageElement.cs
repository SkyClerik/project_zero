using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using SkyClerik.CraftingSystem;
using UnityEngine.Toolbox;
using System.Collections.Generic;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет элемент UI страницы крафта, управляющий взаимодействием с пользователем
    /// и логикой крафта предметов. Наследует функциональность базовой страницы сетки.
    /// </summary>
    public class CraftPageElement : GridPageElementBase
    {
        private const string _titleText = "Окно крафта предметов";
        private VisualElement _body;
        private const string _bodyID = "body";
        private Button _craftButton;
        private const string _craftButtonID = "b_craft";

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CraftPageElement"/>.
        /// </summary>
        /// <param name="inventoryStorage">Ссылка на главную страницу предметов.</param>
        /// <param name="document">UIDocument, содержащий корневой визуальный элемент.</param>
        /// <param name="itemContainer">Контейнер предметов, связанный с этой страницей крафта.</param>
        public CraftPageElement(InventoryStorage inventoryStorage, UIDocument document, ItemContainer itemContainer)
            : base(inventoryStorage, document, itemContainer, itemContainer.RootPanelName)
        {
            _body = _root.Q(_bodyID);
            _craftButton = _body.Q<Button>(_craftButtonID);

            _craftButton.clicked += _craftButton_clicked;
        }

        // Специфичная логика для крафта
        private void _craftButton_clicked()
        {
            var craftSystem = ServiceProvider.Get<CraftingManager>();
            if (craftSystem == null)
            {
                Debug.LogError("Система крафта не найдена!");
                return;
            }

            // 1. Собираем все предметы, которые сейчас лежат в контейнере крафта
            var ingredients = _itemContainer.GetItems().ToList();

            if (craftSystem.TryFindRecipe(ingredients, out var foundRecipe))
            {
                Debug.Log($"Найден рецепт! Результат: {foundRecipe.Result.Item.DefinitionName}");

                // 2. Очищаем контейнер крафта. Это удалит все ингредиенты из данных и вызовет событие для UI.
                _itemContainer.Clear();

                // 3. Создаем результирующий предмет как экземпляр
                var resultItemInstance = UnityEngine.Object.Instantiate(foundRecipe.Result.Item);
                resultItemInstance.Stack = foundRecipe.Result.Quantity;

                // 4. Добавляем результат в контейнер. Он сам найдет место и вызовет событие для отрисовки.
                var unplaced = _itemContainer.AddItems(new List<ItemBaseDefinition> { resultItemInstance });

                if (unplaced.Any())
                {
                    Debug.LogError("В сетке крафта нет места для результата! Результат уничтожен.");
                    // Уничтожаем экземпляр, если он не поместился
                    UnityEngine.Object.Destroy(unplaced.First());
                }
            }
            else
            {
                Debug.Log("Такого рецепта не существует.");
            }
        }

    }
}