using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
	/// <summary>
	/// База данных, хранящая списки всех типов предметов в игре.
	/// Используется для централизованного доступа к определениям расходуемых предметов,
	/// ключевых предметов, брони, оружия и аксессуаров.
	/// </summary>
	[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Data Editor/Databases/Item Database")]
	public class ItemDatabase : ScriptableObject
	{
		[SerializeField]
		[Tooltip("Список всех расходуемых предметов.")]
		private List<ConsumableItemDefinition> _consumableItems = new List<ConsumableItemDefinition>();
		/// <summary>
		/// Список всех расходуемых предметов (только для чтения).
		/// </summary>
		public IReadOnlyList<ConsumableItemDefinition> ConsumableItems => _consumableItems;

		[SerializeField]
		[Tooltip("Список всех ключевых предметов.")]
		private List<KeyItemDefinition> _keyItems = new List<KeyItemDefinition>();
		/// <summary>
		/// Список всех ключевых предметов (только для чтения).
		/// </summary>
		public IReadOnlyList<KeyItemDefinition> KeyItems => _keyItems;

		[SerializeField]
		[Tooltip("Список всех определений брони.")]
		private List<ArmorDefinition> _armorItems = new List<ArmorDefinition>();
		/// <summary>
		/// Список всех определений брони (только для чтения).
		/// </summary>
		public IReadOnlyList<ArmorDefinition> ArmorItems => _armorItems;

		[SerializeField]
		[Tooltip("Список всех определений оружия.")]
		private List<WeaponDefinition> _weaponItems = new List<WeaponDefinition>();
		/// <summary>
		/// Список всех определений оружия (только для чтения).
		/// </summary>
		public IReadOnlyList<WeaponDefinition> WeaponItems => _weaponItems;

		[SerializeField]
		[Tooltip("Список всех определений аксессуаров.")]
		private List<AccessoryDefinition> _accessoryItems = new List<AccessoryDefinition>();
		/// <summary>
		/// Список всех определений аксессуаров (только для чтения).
		/// </summary>
		public IReadOnlyList<AccessoryDefinition> AccessoryItems => _accessoryItems;
	}
}
