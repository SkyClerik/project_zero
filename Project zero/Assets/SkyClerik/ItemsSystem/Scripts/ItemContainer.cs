using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkyClerik.Inventory
{
    public class ItemContainer : MonoBehaviour
    {
        [Header("Хранилище данных")]
        [SerializeField]
        private ItemDataStorageSO _itemDataStorageSO;
        public ItemDataStorageSO ItemDataStorageSO => _itemDataStorageSO;

        [Header("Конфигурация сетки")]
        [Tooltip("Ссылка на UI Document, в котором находится сетка для этого контейнера.")]
        [SerializeField] private UIDocument _uiDocument;
        [Tooltip("Имя корневой панели в UI документе, внутри которой находится элемент 'grid'.")]
        [SerializeField] private string _rootPanelName;
        
        [Tooltip("Рассчитанный размер сетки инвентаря (ширина, высота). Не редактировать вручную.")]
        [SerializeField] [ReadOnly]
        private Vector2Int _gridDimensions;
        public Vector2Int GridDimensions => _gridDimensions;

        // --- События для UI ---
        public event Action<ItemBaseDefinition> OnItemAdded;
        public event Action<ItemBaseDefinition> OnItemRemoved;
        public event Action OnCleared;

        // --- Логика сетки ---
        private bool[,] _gridOccupancy;
        
#if UNITY_EDITOR
        [ContextMenu("Рассчитать размер сетки из UI")]
        public void CalculateGridDimensionsFromUI()
        {
            if (_uiDocument == null || string.IsNullOrEmpty(_rootPanelName))
            {
                Debug.LogError("UIDocument или Root Panel Name не назначены. Расчет невозможен.", this);
                return;
            }
            
            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("rootVisualElement не найден. Расчет невозможен.", this);
                return;
            }

            // Откладываем выполнение на 1 кадр редактора, чтобы получить актуальные размеры
            root.schedule.Execute(() =>
            {
                var rootPanel = root.Q<VisualElement>(_rootPanelName);
                if (rootPanel == null)
                {
                    Debug.LogError($"Панель с именем '{_rootPanelName}' не найдена в UIDocument.", this);
                    return;
                }

                var inventoryGrid = rootPanel.Q<VisualElement>("grid");
                if (inventoryGrid == null)
                {
                    Debug.LogError($"Элемент с именем 'grid' не найден внутри '{_rootPanelName}'.", this);
                    return;
                }
                
                if (inventoryGrid.childCount == 0)
                {
                    Debug.LogWarning($"Сетка '{inventoryGrid.name}' не содержит дочерних элементов (ячеек). Невозможно определить размер ячейки.", this);
                    return;
                }

                var firstCell = inventoryGrid.ElementAt(0);
                var cellSize = firstCell.resolvedStyle; // Теперь resolvedStyle будет доступен

                if (cellSize.width > 0 && cellSize.height > 0)
                {
                    var gridStyle = inventoryGrid.resolvedStyle;
                    int widthCount = Mathf.RoundToInt(gridStyle.width / cellSize.width);
                    int heightCount = Mathf.RoundToInt(gridStyle.height / cellSize.height);

                    if (_gridDimensions.x != widthCount || _gridDimensions.y != heightCount)
                    {
                        _gridDimensions = new Vector2Int(widthCount, heightCount);
                        EditorUtility.SetDirty(this);
                        Debug.Log($"Размер сетки для '{name}' успешно рассчитан: {widthCount}x{heightCount}", this);
                    }
                    else
                    {
                        Debug.Log($"Размер сетки для '{name}' уже актуален: {widthCount}x{heightCount}.", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"Не удалось рассчитать размер сетки для '{name}'. Размер ячейки равен нулю.", this);
                }

            }).ExecuteLater(1);
        }
#endif

        protected virtual void Awake()
        {
            if (_itemDataStorageSO == null)
            {
                Debug.LogWarning("ItemDataStorageSO не назначен в ItemContainer. Создаем новый пустой ItemDataStorageSO.", this);
                _itemDataStorageSO = ScriptableObject.CreateInstance<ItemDataStorageSO>();
            }
            _itemDataStorageSO = ScriptableObject.Instantiate(_itemDataStorageSO);
            _itemDataStorageSO.ValidateGuid();

            _gridOccupancy = new bool[_gridDimensions.x, _gridDimensions.y];
            // Контейнер теперь инициализируется пустым. Предметы добавляются через AddItems/AddClonedItems
        }
                
        #region Public API
        
        public List<ItemBaseDefinition> AddClonedItems(List<ItemBaseDefinition> itemTemplates)
        {
            if (itemTemplates == null || !itemTemplates.Any())
                return new List<ItemBaseDefinition>();

            var clones = itemTemplates.Select(template => Instantiate(template)).ToList();
            return AddItems(clones);
        }

        public List<ItemBaseDefinition> AddItems(List<ItemBaseDefinition> itemsToAdd)
        {
            if (itemsToAdd == null) return new List<ItemBaseDefinition>();
            HandleStacking(itemsToAdd);

            var remainingItems = itemsToAdd.Where(i => i != null && i.Stack > 0).ToList();
            if (!remainingItems.Any()) return new List<ItemBaseDefinition>();

            var sortedItems = remainingItems.OrderByDescending(item =>
                item.Dimensions.CurrentWidth * item.Dimensions.CurrentHeight).ToList();

            List<ItemBaseDefinition> unplacedItems = new List<ItemBaseDefinition>();

            foreach (var item in sortedItems)
            {
                if (TryFindPlacement(item, out var foundPosition))
                {
                    item.GridPosition = foundPosition;
                    OccupyGridCells(item, true);
                    _itemDataStorageSO.Items.Add(item);
                    OnItemAdded?.Invoke(item);
                }
                else
                {
                    unplacedItems.Add(item);
                }
            }
            return unplacedItems;
        }

        public bool RemoveItem(ItemBaseDefinition item, bool destroy = true)
        {
            if (item == null) return false;

            bool removed = _itemDataStorageSO.Items.Remove(item);
            if (removed)
            {
                OccupyGridCells(item, false);
                OnItemRemoved?.Invoke(item);
                if (destroy) Destroy(item);
            }
            return removed;
        }

        public void Clear()
        {
            var itemsCopy = _itemDataStorageSO.Items.ToList();
            _itemDataStorageSO.Items.Clear();
            if (_gridOccupancy != null)
                Array.Clear(_gridOccupancy, 0, _gridOccupancy.Length);
            
            OnCleared?.Invoke();

            foreach(var item in itemsCopy) Destroy(item);
        }
        
        public IReadOnlyList<ItemBaseDefinition> GetItems()
        {
            return _itemDataStorageSO.Items.AsReadOnly();
        }

        public bool TryAddItemAtPosition(ItemBaseDefinition item, Vector2Int gridPosition)
        {
            if (item == null) return false;
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.CurrentWidth, item.Dimensions.CurrentHeight);

            if (IsGridAreaFree(gridPosition, itemGridSize))
            {
                item.GridPosition = gridPosition;
                OccupyGridCells(item, true);
                _itemDataStorageSO.Items.Add(item);
                OnItemAdded?.Invoke(item);
                return true;
            }
            return false;
        }

        public void MoveItem(ItemBaseDefinition item, Vector2Int newPosition)
        {
            // Освобождаем старое место
            OccupyGridCells(item, false);
            // Устанавливаем новую позицию
            item.GridPosition = newPosition;
            // Занимаем новое место
            OccupyGridCells(item, true);

            // Уведомляем UI, что предмет был удален (со старого места) и добавлен (на новое)
            // Это заставит UI перерисовать предмет в правильном месте
            OnItemRemoved?.Invoke(item);
            OnItemAdded?.Invoke(item);
        }

        #endregion

        #region Grid Logic
        
        private void HandleStacking(List<ItemBaseDefinition> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                if (item == null || !item.Stackable || item.Stack <= 0) continue;
                foreach (var existingItem in _itemDataStorageSO.Items)
                {
                    if (item.Stack <= 0) break;
                    if (existingItem.DefinitionName == item.DefinitionName && existingItem.Stack < existingItem.MaxStack)
                    {
                        int spaceAvailable = existingItem.MaxStack - existingItem.Stack;
                        int amountToTransfer = Mathf.Min(spaceAvailable, item.Stack);
                        if (amountToTransfer > 0)
                        {
                            existingItem.AddStack(amountToTransfer, out _);
                            item.RemoveStack(amountToTransfer);
                            OnItemAdded?.Invoke(existingItem);
                        }
                    }
                }
            }
        }

        public void OccupyGridCells(ItemBaseDefinition item, bool occupy)
        {
            var size = new Vector2Int(item.Dimensions.CurrentWidth, item.Dimensions.CurrentHeight);
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int gridX = item.GridPosition.x + x;
                    int gridY = item.GridPosition.y + y;
                    if (gridX < _gridDimensions.x && gridY < _gridDimensions.y)
                        _gridOccupancy[gridX, gridY] = occupy;
                }
            }
        }

        public bool IsGridAreaFree(Vector2Int start, Vector2Int size)
        {
            if (start.x < 0 || start.y < 0 || start.x + size.x > _gridDimensions.x || start.y + size.y > _gridDimensions.y)
                return false;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    if (_gridOccupancy[start.x + x, start.y + y])
                        return false;
                }
            }
            return true;
        }

        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.CurrentWidth, item.Dimensions.CurrentHeight);
            for (int y = 0; y <= _gridDimensions.y - itemGridSize.y; y++)
            {
                for (int x = 0; x <= _gridDimensions.x - itemGridSize.x; x++)
                {
                    var currentPos = new Vector2Int(x, y);
                    if (IsGridAreaFree(currentPos, itemGridSize))
                    {
                        suggestedGridPosition = currentPos;
                        return true;
                    }
                }
            }
            suggestedGridPosition = Vector2Int.zero;
            return false;
        }
        
        #endregion
        
        #region Obsolete Methods
        
        [Obsolete("Используйте AddItems или AddClonedItems для правильного размещения в сетке.")]
        public ItemBaseDefinition AddItemAsClone(ItemBaseDefinition item)
        {
            var unplaced = AddClonedItems(new List<ItemBaseDefinition> { item });
            return unplaced.Any() ? null : item;
        }

        [Obsolete("Используйте AddItems или AddClonedItems для правильного размещения в сетке.")]
        public void AddItem(ItemBaseDefinition item)
        {
            AddItems(new List<ItemBaseDefinition> { item });
        }
        
        [Obsolete("Этот метод не учитывает логику сетки. Не использовать.")]
        public void AddItemReference(ItemBaseDefinition item)
        {
            if (item != null)
            {
                _itemDataStorageSO.Items.Add(item);
                OnItemAdded?.Invoke(item);
            }
        }
        
        #endregion
    }
}