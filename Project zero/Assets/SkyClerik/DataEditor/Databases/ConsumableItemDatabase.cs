using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "ConsumableItemDatabase", menuName = "Definition/Database/Consumable Item Database")]
    public class ConsumableItemDatabase : ScriptableObject, IDefinitionDatabase<ConsumableItemDefinition>
    {
        [SerializeField]
        private List<ConsumableItemDefinition> _items = new List<ConsumableItemDefinition>();
        public IReadOnlyList<ConsumableItemDefinition> Items => _items;
    }
}
