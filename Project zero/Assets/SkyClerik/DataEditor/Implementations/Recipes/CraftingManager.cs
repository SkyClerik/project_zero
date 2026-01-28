using System.Collections.Generic;
using System.Linq;
using UnityEngine; // Добавлено, так как UnityEditor.Object убран
using UnityEngine.DataEditor;
using UnityEngine.DataEditor.Databases; // Добавлено для RecipeDatabase
using UnityEngine.Toolbox;

namespace UnityEngine.CraftingSystem
{
    /// <summary>
    /// Управляет рецептами и логикой крафта в игре.
    /// Регистрирует себя как сервис 'ICraftingSystem' в ServiceProvider.
    /// </summary>
    public class CraftingManager : MonoBehaviour, ICraftingSystem
    {
        [SerializeField]
        private RecipeDatabase _recipeDatabase; // Наша база данных рецептов

        // Словарь для молниеносного поиска рецептов по их "отпечатку".
        private readonly Dictionary<string, CraftingRecipe> _recipes = new Dictionary<string, CraftingRecipe>();

        private void Awake()
        {
            // Регистрируем себя как реализацию интерфейса ICraftingSystem.
            ServiceProvider.Register<ICraftingSystem>(this);
            Initialize();
        }

        private void OnDestroy()
        {
            // Отменяем регистрацию, когда объект уничтожается.
            ServiceProvider.Unregister<ICraftingSystem>();
        }

        /// <summary>
        /// Инициализирует менеджер, загружая и обрабатывая все рецепты из RecipeDatabase.
        /// </summary>
        private void Initialize()
        {
            if (_recipeDatabase == null)
            {
                Debug.LogError("CraftingManager: RecipeDatabase не назначен! Крафт не будет работать.");
                return;
            }

            foreach (var recipe in _recipeDatabase.Items) // Берем рецепты из нашей базы
            {
                if (recipe != null && recipe.Ingredients.Count > 0)
                {
                    // Создаем "отпечаток" для рецепта и добавляем в словарь.
                    string key = GenerateRecipeKey(recipe.Ingredients);
                    if (!_recipes.ContainsKey(key))
                    {
                        _recipes.Add(key, recipe);
                    }
                    else
                    {
                        // Предупреждение, если найдены два рецепта с одинаковым набором ингредиентов.
                        Debug.LogWarning($"Найден дубликат рецепта для набора ингредиентов. Рецепт '{recipe.DefinitionName}' будет проигнорирован.");
                    }
                }
            }
        }

        /// <summary>
        /// Пытается найти рецепт, соответствующий предоставленным предметам.
        /// </summary>
        public bool TryFindRecipe(List<ItemBaseDefinition> providedItems, out CraftingRecipe foundRecipe)
        {
            // Генерируем ключ из предоставленных предметов.
            string key = GenerateItemsKey(providedItems);
            return _recipes.TryGetValue(key, out foundRecipe);
        }

        /// <summary>
        /// Генерирует уникальный ключ-отпечаток из списка ингредиентов рецепта.
        /// </summary>
        private string GenerateRecipeKey(List<Ingredient> ingredients)
        {
            // Собираем ID и количество для каждого ингредиента.
            var itemCounts = ingredients
                .Select(i => $"{i.Item.ID}:{i.Quantity}")
                .ToList();

            // Сортируем, чтобы порядок не имел значения.
            itemCounts.Sort();

            return string.Join(";", itemCounts);
        }

        /// <summary>
        /// Генерирует ключ-отпечаток из списка предметов, находящихся в сетке крафта.
        /// </summary>
        private string GenerateItemsKey(List<ItemBaseDefinition> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;

            // Группируем одинаковые предметы и подсчитываем их количество.
            var itemCounts = items
                .GroupBy(i => i.ID)
                .Select(g => $"{g.Key}:{g.Count()}")
                .ToList();

            // Сортируем для консистентности.
            itemCounts.Sort();

            return string.Join(";", itemCounts);
        }
    }
}
