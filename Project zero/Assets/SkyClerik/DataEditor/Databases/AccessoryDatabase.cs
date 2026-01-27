using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "AccessoryDatabase", menuName = "Definition/Database/Accessory Database")]
    public class AccessoryDatabase : ScriptableObject, IDefinitionDatabase<AccessoryDefinition>
    {
        [SerializeField]
        private List<AccessoryDefinition> _items = new List<AccessoryDefinition>();
        public IReadOnlyList<AccessoryDefinition> Items => _items;
    }
}
