using System;

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Переопределяет отображаемое имя поля в кастомном редакторе UI Toolkit.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class DisplayNameOverrideAttribute : PropertyAttribute
	{
		/// <summary>
		/// Отображаемое имя, которое будет использоваться в редакторе.
		/// </summary>
		public string DisplayName { get; private set; }

		/// <summary>
		/// Конструктор атрибута.
		/// </summary>
		/// <param name="displayName">Отображаемое имя.</param>
		public DisplayNameOverrideAttribute(string displayName)
		{
			DisplayName = displayName;
		}
	}
}
