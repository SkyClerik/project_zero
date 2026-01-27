using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Интерфейс, представляющий игровую единицу (юнита), которая обладает характеристиками и навыками.
	/// Возможно, назвать его IGameEntity, если он будет шире, чем просто "юнит".
	/// </summary>
	public interface IUnit 
	{
		/// <summary>
		/// Контейнер со всеми характеристиками юнита.
		/// </summary>
		StatsContainer Stats { get; }

		/// <summary>
		/// Список навыков, доступных юниту.
		/// </summary>
		List<SkillBaseDefinition> Skills { get; } 
		// Здесь могут быть и другие свойства юнита: Inventory, AppliedStates и т.д.
	}
}
