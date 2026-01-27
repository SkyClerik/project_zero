using System;
using UnityEngine; // PropertyAttribute is in UnityEngine

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Помечает поле типа Sprite для отрисовки с помощью кастомного SquareIconField.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class DrawWithIconFieldAttribute : PropertyAttribute
	{
	}
}
