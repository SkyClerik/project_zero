using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "ArmorDatabase", menuName = "Definition/Database/Armor Database")]
    public class ArmorDatabase : ScriptableObject, IDefinitionDatabase<ArmorDefinition>
    {
        [SerializeField]
        private List<ArmorDefinition> _items = new List<ArmorDefinition>();
        public IReadOnlyList<ArmorDefinition> Items => _items;
    }
}
