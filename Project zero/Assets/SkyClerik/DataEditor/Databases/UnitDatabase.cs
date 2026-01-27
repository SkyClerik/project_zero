namespace UnityEngine.DataEditor
{
	/// <summary>
	/// База данных для хранения всех определений юнитов.
	/// Этот класс пуст, так как вся логика реализована в базовом классе DefinitionDatabase.
	/// </summary>
	[CreateAssetMenu(fileName = "UnitDatabase", menuName = "Game Data/Database/Unit Database")]
	public class UnitDatabase : DefinitionDatabase<UnitBaseDefinition> { }
}
