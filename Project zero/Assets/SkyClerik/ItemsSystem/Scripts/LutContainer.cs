using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Компонент, представляющий собой контейнер для лута.
    /// Хранит список предметов, которые могут быть переданы в другой контейнер.
    /// </summary>
    public class LutContainer : MonoBehaviour
    {
        [SerializeField]
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        /// <summary>
        /// Список предметов, находящихся в этом контейнере лута.
        /// </summary>
        public List<ItemBaseDefinition> Items => _items;
    }
}