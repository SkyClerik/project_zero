using UnityEngine;

namespace SkyClerik.Inventory
{
    public struct PlacementResults
    {
        public ReasonConflict Conflict;
        public Vector2 Position;
        public Vector2Int SuggestedGridPosition;
        public ItemVisual OverlapItem;
        public IDropTarget TargetInventory;

        public PlacementResults Init(ReasonConflict conflict, Vector2 position, Vector2Int suggestedGridPosition, ItemVisual overlapItem, IDropTarget targetInventory)
        {
            Conflict = conflict;
            Position = position;
            SuggestedGridPosition = suggestedGridPosition;
            OverlapItem = overlapItem;
            TargetInventory = targetInventory;

            return this;
        }

        public PlacementResults Init(ReasonConflict conflict, Vector2Int suggestedGridPosition, ItemVisual overlapItem, IDropTarget targetInventory)
        {
            Conflict = conflict;
            Position = Vector2.zero;
            SuggestedGridPosition = suggestedGridPosition;
            OverlapItem = overlapItem;
            TargetInventory = targetInventory;

            return this;
        }
    }
}