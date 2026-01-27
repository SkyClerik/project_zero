using UnityEngine;

namespace Gameplay.Inventory
{
    public struct PlacementResults
    {
        public ReasonConflict Conflict;
        public Vector2 Position;
        public ItemVisual OverlapItem;

        public PlacementResults Init(ReasonConflict conflict, Vector2 position, ItemVisual overlapItem)
        {
            Conflict = conflict;
            Position = position;
            OverlapItem = overlapItem;

            return this;
        }

        public PlacementResults Init(ReasonConflict conflict, ItemVisual overlapItem)
        {
            Conflict = conflict;
            Position = Vector2.zero;
            OverlapItem = overlapItem;

            return this;
        }
    }
}