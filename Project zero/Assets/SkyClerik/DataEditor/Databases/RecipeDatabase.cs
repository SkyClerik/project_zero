using SkyClerik.CraftingSystem;

namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "New Recipe Database", menuName = "SkyClerik/Definition/Databases/Recipe Database")]
    public class RecipeDatabase : DefinitionDatabase<CraftingRecipe> { }
}