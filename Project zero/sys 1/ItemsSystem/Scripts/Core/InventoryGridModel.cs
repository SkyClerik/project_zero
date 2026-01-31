using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    public class InventoryGridModelOLD
    {
        private readonly bool[,] _occupancyMatrix;
        private readonly Dictionary<ItemBaseDefinition, Vector2Int> _itemPositions;

        public readonly int Width;
        public readonly int Height;

        public InventoryGridModelOLD(int width, int height)
        {
            Width = width;
            Height = height;
            _occupancyMatrix = new bool[width, height];
            _itemPositions = new Dictionary<ItemBaseDefinition, Vector2Int>();
        }

        public bool IsAreaFree(Vector2Int position, Vector2Int size)
        {
            if (position.x < 0 || position.y < 0 || position.x + size.x > Width || position.y + size.y > Height)
                return false;

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    if (_occupancyMatrix[position.x + x, position.y + y])
                        return false;
                }
            }
            return true;
        }

        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int position)
        {
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.DefaultWidth, item.Dimensions.DefaultHeight);
            for (int y = 0; y <= Height - itemGridSize.y; y++)
            {
                for (int x = 0; x <= Width - itemGridSize.x; x++)
                {
                    Vector2Int currentPosition = new Vector2Int(x, y);
                    if (IsAreaFree(currentPosition, itemGridSize))
                    {
                        position = currentPosition;
                        return true;
                    }
                }
            }
            position = Vector2Int.zero;
            return false;
        }

        public void PlaceItem(ItemBaseDefinition item, Vector2Int position)
        {
            if (_itemPositions.ContainsKey(item))
            {
                RemoveItem(item);
            }

            _itemPositions[item] = position;
            Vector2Int size = new Vector2Int(item.Dimensions.CurrentWidth, item.Dimensions.CurrentHeight);
            SetOccupancy(position, size, true);
        }

        public void RemoveItem(ItemBaseDefinition item)
        {
            if (_itemPositions.TryGetValue(item, out Vector2Int position))
            {
                _itemPositions.Remove(item);
                Vector2Int size = new Vector2Int(item.Dimensions.CurrentWidth, item.Dimensions.CurrentHeight);
                SetOccupancy(position, size, false);
            }
        }

        public Vector2Int GetItemPosition(ItemBaseDefinition item)
        {
            _itemPositions.TryGetValue(item, out var pos);
            return pos;
        }

        public IReadOnlyDictionary<ItemBaseDefinition, Vector2Int> GetAllItemPositions()
        {
            return _itemPositions;
        }

        private void SetOccupancy(Vector2Int position, Vector2Int size, bool isOccupied)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    _occupancyMatrix[position.x + x, position.y + y] = isOccupied;
                }
            }
        }
    }
}
