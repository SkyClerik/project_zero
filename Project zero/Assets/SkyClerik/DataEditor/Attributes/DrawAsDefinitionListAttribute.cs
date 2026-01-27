using System;
using UnityEngine; // PropertyAttribute is in UnityEngine

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Помечает список сериализуемых объектов для отрисовки с помощью кастомного ListView,
	/// где каждый элемент является редактируемым с помощью сложного поля,
	/// такого как DefinitionSelectorField.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class DrawAsDefinitionListAttribute : PropertyAttribute
	{
		/// <summary>
		/// Тип определения, который будет использоваться для выбора элементов в списке.
		/// </summary>
		public Type DefinitionType { get; private set; }

		/// <summary>
		/// Конструктор атрибута.
		/// </summary>
		/// <param name="definitionType">Тип определения для выбора.</param>
		public DrawAsDefinitionListAttribute(Type definitionType)
		{
			DefinitionType = definitionType;
		}
	}
}
