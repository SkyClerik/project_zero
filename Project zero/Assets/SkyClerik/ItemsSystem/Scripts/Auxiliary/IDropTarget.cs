using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    public interface IDropTarget
    {
        UIDocument GetDocument { get; }
        PlacementResults ShowPlacementTarget(ItemVisual itemVisual);
        void FinalizeDrag();

        void AddStoredItem(ItemVisual storedItem);
        void RemoveStoredItem(ItemVisual storedItem);

        void PickUp(ItemVisual storedItem);
        void Drop(ItemVisual storedItem, Vector2 position);

        void AddItemToInventoryGrid(VisualElement item);
    }
}