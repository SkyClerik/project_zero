namespace UnityEngine.DataEditor
{
	/// <summary>
	/// База данных для хранения всех универсальных определений типов.
	/// Этот класс пуст, так как вся логика реализована в базовом классе DefinitionDatabase.
	/// </summary>
	[CreateAssetMenu(fileName = "TypeDatabase", menuName = "SkyClerik/Game Data/Database/Type Database")]
	public class TypeDatabase : DefinitionDatabase<TypeDefinition> { }
}
