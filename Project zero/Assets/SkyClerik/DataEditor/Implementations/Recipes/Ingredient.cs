using System;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.CraftingSystem
{
    /// <summary>
    /// Описывает один ингредиент для рецепта, включая необходимое количество.
    /// </summary>
    [Serializable]
    public class Ingredient
    {
        [SerializeField]
        private ItemBaseDefinition _item;
        public ItemBaseDefinition Item => _item;

        [SerializeField]
        [Min(1)]
        private int _quantity = 1;
        public int Quantity => _quantity;
    }
}
