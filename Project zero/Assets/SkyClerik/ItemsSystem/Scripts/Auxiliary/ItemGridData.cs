using UnityEngine;
using UnityEngine.DataEditor; // Для ItemBaseDefinition

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public class ItemGridData
    {
        public ItemBaseDefinition ItemDefinition;
        public Vector2Int GridPosition; // Позиция верхнего левого угла предмета в ячейках сетки
        public Vector2Int GridSize => new Vector2Int(ItemDefinition.Dimensions.DefaultWidth, ItemDefinition.Dimensions.DefaultHeight);
        // Возможно, потребуется хранить информацию о повороте (CurrentWidth, CurrentHeight)
        // или пересчитывать GridSize при повороте. Пока используем Default.

        public ItemGridData(ItemBaseDefinition item, Vector2Int position)
        {
            ItemDefinition = item;
            GridPosition = position;
        }
    }
}