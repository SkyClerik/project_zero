namespace UnityEngine.DataEditor
{
	/// <summary>
	/// Определение для аксессуаров.
	/// </summary>
	[CreateAssetMenu(fileName = "AccessoryDefinition", menuName = "Definition/Item/AccessoryDefinition")]
	public class AccessoryDefinition : ItemBaseDefinition
	{
		// Аксессуары часто являются носителями трейтов/особенностей,
		// поэтому у них может не быть собственных уникальных полей.
	}
}
