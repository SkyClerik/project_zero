namespace UnityEngine.DataEditor
{
	/// <summary>
	/// База данных для хранения всех определений классов персонажей.
	/// Этот класс пуст, так как вся логика реализована в базовом классе DefinitionDatabase.
	/// </summary>
	[CreateAssetMenu(fileName = "ClassDatabase", menuName = "Game Data/Database/Class Database")]
	public class ClassDatabase : DefinitionDatabase<ClassDefinition> 
	{

	}
}
