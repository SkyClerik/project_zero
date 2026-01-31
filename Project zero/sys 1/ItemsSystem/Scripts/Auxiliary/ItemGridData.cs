using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    [System.Serializable]
    public class ItemGridData
    {
        [SerializeField]
        private ItemBaseDefinition _itemDefinition; 
        [SerializeField]
        private Vector2Int _gridPosition;

        public ItemBaseDefinition ItemDefinition => _itemDefinition;
        public Vector2Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        public Vector2Int GridSize => new Vector2Int(ItemDefinition.Dimensions.CurrentWidth, ItemDefinition.Dimensions.CurrentHeight);

        public ItemGridData(ItemBaseDefinition item, Vector2Int position)
        {
            _itemDefinition = item;
            _gridPosition = position;
        }
    }
}