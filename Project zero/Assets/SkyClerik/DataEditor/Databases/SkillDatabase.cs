using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// База данных для хранения всех определений навыков.
    /// Этот класс пуст, так как вся логика реализована в базовом классе DefinitionDatabase.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "Definition/Database/Skill Database")]
    public class SkillDatabase : ScriptableObject, IDefinitionDatabase<SkillBaseDefinition>
    {
        [SerializeField]
        [Tooltip("Список всех определений навыков.")]
        private List<SkillBaseDefinition> _items = new List<SkillBaseDefinition>();
        public IReadOnlyList<SkillBaseDefinition> Items => _items;
    }
}
