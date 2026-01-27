using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Definition/Database/Weapon Database")]
    public class WeaponDatabase : ScriptableObject, IDefinitionDatabase<WeaponDefinition>
    {
        [SerializeField]
        private List<WeaponDefinition> _items = new List<WeaponDefinition>();
        public IReadOnlyList<WeaponDefinition> Items => _items;
    }
}
