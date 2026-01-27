using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Inventory
{
    public interface IDropTarget
    {
        UIDocument GetDocument { get; }
        PlacementResults ShowPlacementTarget(ItemVisual itemVisual);

        void AddStoredItem(StoredItem storedItem);
        void RemoveStoredItem(StoredItem storedItem);

        void PickUp(StoredItem storedItem);
        void Drop(StoredItem storedItem, Vector2 position);

        void AddItemToInventoryGrid(VisualElement item);
    }
}