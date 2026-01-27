using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "KeyItemDatabase", menuName = "Definition/Database/Key Item Database")]
    public class KeyItemDatabase : ScriptableObject, IDefinitionDatabase<KeyItemDefinition>
    {
        [SerializeField]
        private List<KeyItemDefinition> _items = new List<KeyItemDefinition>();
        public IReadOnlyList<KeyItemDefinition> Items => _items;
    }
}
