using UnityEngine;

namespace SkyClerik.Inventory
{
    public struct PlacementResults
    {
        public ReasonConflict Conflict;
        public Vector2 Position;
        public ItemVisual OverlapItem;
        public IDropTarget TargetInventory;

        public PlacementResults Init(ReasonConflict conflict, Vector2 position, ItemVisual overlapItem, IDropTarget targetInventory)
        {
            Conflict = conflict;
            Position = position;
            OverlapItem = overlapItem;
            TargetInventory = targetInventory;

            return this;
        }

        public PlacementResults Init(ReasonConflict conflict, ItemVisual overlapItem, IDropTarget targetInventory)
        {
            Conflict = conflict;
            Position = Vector2.zero;
            OverlapItem = overlapItem;
            TargetInventory = targetInventory;

            return this;
        }
    }
}