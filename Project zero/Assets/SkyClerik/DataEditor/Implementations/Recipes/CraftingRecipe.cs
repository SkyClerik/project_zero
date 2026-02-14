using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.CraftingSystem
{
    /// <summary>
    /// ScriptableObject для определения рецепта крафта.
    /// Создайте ассет этого типа для каждого нового рецепта.
    /// </summary>
    [CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "SkyClerik/Crafting/Crafting Recipe")]
    public class CraftingRecipe : BaseDefinition
    {
        [SerializeField]
        [Tooltip("Список ингредиентов, необходимых для крафта.")]
        private List<Ingredient> _ingredients = new List<Ingredient>();
        public List<Ingredient> Ingredients => _ingredients;

        [SerializeField]
        [Tooltip("Предмет, который будет получен в результате крафта.")]
        private Ingredient _result;
        public Ingredient Result => _result;
    }
}
