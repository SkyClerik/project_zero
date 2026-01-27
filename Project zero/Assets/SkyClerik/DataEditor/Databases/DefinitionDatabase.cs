using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Обобщенный ScriptableObject, служащий контейнером-базой данных
	/// для определенного типа определений (T), наследующих от BaseDefinition.
	/// </summary>
	/// <typeparam name="T">Тип определения (например, SkillDefinition, UnitBaseDefinition)</typeparam>
	public class DefinitionDatabase<T> : ScriptableObject, IDefinitionDatabase<T> where T : BaseDefinition
	{
		[SerializeField]
		[Tooltip("Список всех определений, хранящихся в этой базе данных.")]
		private List<T> _items = new List<T>();

		/// <summary>
		/// Список всех определений, хранящихся в этой базе данных (только для чтения).
		/// </summary>
		public IReadOnlyList<T> Items => _items;
	}
}
