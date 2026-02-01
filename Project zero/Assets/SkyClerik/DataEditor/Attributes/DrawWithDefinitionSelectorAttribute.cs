using System;

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Помечает поле с атрибутом [SerializeReference] типа BaseDefinition
	/// для отрисовки с помощью кастомной кнопки выбора (селектора).
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class DrawWithDefinitionSelectorAttribute : PropertyAttribute
	{
		/// <summary>
		/// Тип определения, который будет использоваться для выбора элемента.
		/// </summary>
		public Type DefinitionType { get; private set; }

		/// <summary>
		/// Конструктор атрибута.
		/// </summary>
		/// <param name="definitionType">Тип определения для выбора.</param>
		public DrawWithDefinitionSelectorAttribute(Type definitionType)
		{
			DefinitionType = definitionType;
		}
	}
}
