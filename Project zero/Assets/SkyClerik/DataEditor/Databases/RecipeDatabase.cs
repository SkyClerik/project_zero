using UnityEngine;
using UnityEngine.DataEditor.Databases; // Добавлено для DefinitionDatabase
using UnityEngine.CraftingSystem; // Добавлено для CraftingRecipe

namespace UnityEngine.DataEditor.Databases
{
    [CreateAssetMenu(fileName = "New Recipe Database", menuName = "SkyClerik/Databases/Recipe Database")]
    public class RecipeDatabase : DefinitionDatabase<CraftingRecipe>
    {
        // Этот класс пустой, вся логика наследуется от DefinitionDatabase<T>
    }
}