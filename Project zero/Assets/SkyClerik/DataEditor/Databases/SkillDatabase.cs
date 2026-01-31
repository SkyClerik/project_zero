namespace UnityEngine.DataEditor
{
    /// <summary>
    /// База данных для хранения всех определений навыков.
    /// Этот класс пуст, так как вся логика реализована в базовом классе DefinitionDatabase.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "SkyClerik/Definition/Database/Skill Database")]
    public class SkillDatabase : DefinitionDatabase<SkillBaseDefinition> { }
}
