using System;

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Пользовательский атрибут, который определяет, как подкласс TraitDefinition
	/// должен отображаться в меню создания.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class CreateTraitMenuItemAttribute : Attribute
	{
		/// <summary>
		/// Путь в меню, по которому будет создаваться трейт (например, "Параметр/Бонус к Силе").
		/// </summary>
		public readonly string MenuPath;
		/// <summary>
		/// Подсказка, которая может отображаться в UI при выборе этого пункта меню.
		/// </summary>
		public readonly string Tooltip;

		/// <summary>
		/// Конструктор атрибута.
		/// </summary>
		/// <param name="menuPath">Путь в меню (например, "Параметр/Бонус к Силе").</param>
		/// <param name="tooltip">Подсказка для UI.</param>
		public CreateTraitMenuItemAttribute(string menuPath, string tooltip = "")
		{
			MenuPath = menuPath;
			Tooltip = tooltip;
		}
	}
}
