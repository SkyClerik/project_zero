using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    public class LutContainer : MonoBehaviour
    {
        [SerializeField]
        private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

        public List<ItemBaseDefinition> Items => _items;
    }
}