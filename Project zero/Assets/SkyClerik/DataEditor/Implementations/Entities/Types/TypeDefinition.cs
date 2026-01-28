namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Универсальное определение для различных игровых "типов", 
	/// таких как типы навыков, типы оружия, элементы и т.д.
	/// </summary>
	[CreateAssetMenu(fileName = "TypeDefinition", menuName = "Definition/Game/TypeDefinition")]
	public class TypeDefinition : BaseDefinition
	{
		//TODO переписать типы. Это базовый и используется как магический. Нужно четкое разделение по типам от базового.
		[Header("Настройки типа")]
		[Tooltip("Категория, к которой относится этот тип (например, 'SkillType', 'ElementType', 'WeaponType'). Используется для группировки и фильтрации в редакторе.")]
		[SerializeField]
		private string _category;
		public string Category => _category;

		public override string ToString()
		{
			return $"[{(!string.IsNullOrEmpty(_category) ? _category : "N/A")}] {DefinitionName ?? name}";
		}

		// Этот класс намеренно простой. Он наследует ID, Name, Description и Icon от BaseDefinition.
		// Основная его задача - быть ссылкой, которую можно использовать в других определениях.
	}
}
