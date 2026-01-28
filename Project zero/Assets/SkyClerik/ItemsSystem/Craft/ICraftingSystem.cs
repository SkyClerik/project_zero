using System.Collections.Generic;
using UnityEngine.DataEditor;

namespace UnityEngine.CraftingSystem
{
    /// <summary>
    /// Интерфейс для системы крафта.
    /// </summary>
    public interface ICraftingSystem
    {
        /// <summary>
        /// Пытается найти рецепт, соответствующий предоставленным предметам.
        /// </summary>
        /// <param name="providedItems">Список предметов, из которых нужно произвести крафт.</param>
        /// <param name="foundRecipe">Найденный рецепт, если он существует.</param>
        /// <returns>True, если рецепт найден, иначе false.</returns>
        bool TryFindRecipe(List<ItemBaseDefinition> providedItems, out CraftingRecipe foundRecipe);
    }
}
