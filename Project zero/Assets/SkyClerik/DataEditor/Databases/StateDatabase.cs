using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// База данных для хранения всех определений состояний.
    /// </summary>
    [CreateAssetMenu(fileName = "StateDatabase", menuName = "Definition/Database/State Database")]
    public class StateDatabase : ScriptableObject, IDefinitionDatabase<StateBaseDefinition>
    {
        [SerializeField]
        [Tooltip("Список всех определений состояний.")]
        private List<StateBaseDefinition> _items = new List<StateBaseDefinition>();
        public IReadOnlyList<StateBaseDefinition> Items => _items;
    }
}
