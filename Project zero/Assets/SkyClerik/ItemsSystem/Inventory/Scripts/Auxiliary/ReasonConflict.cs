namespace Gameplay.Inventory
{
    public enum ReasonConflict : byte
    {
        None = 0,
        beyondTheGridBoundary = 1,
        intersectsObjects = 2,
        invalidSlotType = 3,
    }
}