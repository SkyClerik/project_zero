using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class ItemContainer : MonoBehaviour
    {
        [Header("Хранилище данных")]
        [SerializeField]
        private ItemContainerDefinition _itemDataStorageSO;
        public ItemContainerDefinition ItemDataStorageSO => _itemDataStorageSO;

        [Header("Конфигурация сетки")]
        [Tooltip("Ссылка на UI Document, в котором находится сетка для этого контейнера.")]
        [SerializeField] private UIDocument _uiDocument;
        [Tooltip("Имя корневой панели в UI документе, внутри которой находится элемент 'grid'.")]
        [SerializeField] private string _rootPanelName;

        [Tooltip("Рассчитанный размер сетки инвентаря (ширина, высота). Не редактировать вручную.")]
        [SerializeField]
        [ReadOnly]
        [Space]
        private Vector2Int _gridDimensions;
        [Tooltip("Рассчитанный размер ячейки в пикселях. Не редактировать вручную.")]
        [SerializeField]
        [ReadOnly]
        [Space]
        private Vector2 _cellSize;
        [Tooltip("Рассчитанные мировые координаты сетки. Не редактировать вручную.")]
        private Rect _gridWorldRect;

        public Vector2Int GridDimensions => _gridDimensions;
        public Vector2 CellSize => _cellSize;
        public Rect GridWorldRect => _gridWorldRect;

        // --- События для UI ---
        public event Action<ItemBaseDefinition> OnItemAdded;
        public event Action<ItemBaseDefinition> OnItemRemoved;
        public event Action OnCleared;
        public event Action OnGridOccupancyChanged;

        // --- Логика сетки ---
        private bool[,] _gridOccupancy;

        public bool[,] GetGridOccupancy => _gridOccupancy;

#if UNITY_EDITOR
        [ContextMenu("Рассчитать размер сетки из UI (Нажать в Play Mode или при видимом UI)")]
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
                Debug.LogError("rootVisualElement не найден. Убедитесь, что UIDocument активен и его панель видима.", this);
                return;
            }

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
                var calculatedCellSize = new Vector2(firstCell.resolvedStyle.width, firstCell.resolvedStyle.height);

                if (calculatedCellSize.x > 0 && calculatedCellSize.y > 0)
                {
                    var gridStyle = inventoryGrid.resolvedStyle;
                    int widthCount = Mathf.RoundToInt(gridStyle.width / calculatedCellSize.x);
                    int heightCount = Mathf.RoundToInt(gridStyle.height / calculatedCellSize.y);

                    bool changed = false;
                    if (_gridDimensions.x != widthCount || _gridDimensions.y != heightCount)
                    {
                        _gridDimensions = new Vector2Int(widthCount, heightCount);
                        changed = true;
                    }
                    if (_cellSize != calculatedCellSize)
                    {
                        _cellSize = calculatedCellSize;
                        changed = true;
                    }
                    if (_gridWorldRect != inventoryGrid.worldBound)
                    {
                        _gridWorldRect = inventoryGrid.worldBound;
                        changed = true;
                    }

                    if (changed)
                    {
                        UnityEditor.EditorUtility.SetDirty(this);
                        Debug.Log($"[ItemContainer:{name}] Параметры сетки для '{_rootPanelName}' успешно рассчитаны:\n" +
                                  $"- Размеры в ячейках: {widthCount}x{heightCount}\n" +
                                  $"- Размер ячейки (px): {_cellSize.x}x{_cellSize.y}\n" +
                                  $"- Позиция и размер сетки (world): {_gridWorldRect}", this);
                    }
                    else
                    {
                        Debug.Log($"[ItemContainer:{name}] Параметры сетки для '{_rootPanelName}' уже актуальны.", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"Не удалось рассчитать параметры сетки для '{name}'. Размер ячейки равен нулю. Убедитесь, что UI отрисован и виден.", this);
                }

            }).ExecuteLater(1);
        }
#endif

        protected virtual void Awake()
        {
            if (_itemDataStorageSO == null)
            {
                //Debug.LogWarning("ItemDataStorageSO не назначен в ItemContainer. Создаем новый пустой ItemDataStorageSO.", this);
                _itemDataStorageSO = ScriptableObject.CreateInstance<ItemContainerDefinition>();
            }
            _itemDataStorageSO = ScriptableObject.Instantiate(_itemDataStorageSO);
            _itemDataStorageSO.ValidateGuid();

            _gridOccupancy = new bool[_gridDimensions.x, _gridDimensions.y];
            //Debug.Log($"[ItemContainer] Awake: Инициализирована _gridOccupancy с размерами: {_gridDimensions.x}x{_gridDimensions.y}", this);
        }


        /// <summary>
        /// Перестраивает логическую сетку инвентаря и помечает занятые ячейки на основе загруженных предметов.
        /// Вызывается после загрузки данных из сохранения.
        /// </summary>
        public void SetupLoadedItemsGrid()
        {
            if (_gridDimensions.x <= 0 || _gridDimensions.y <= 0)
            {
                Debug.LogError($"[ItemContainer:{name}] Grid dimensions are not set. Cannot setup loaded items grid.", this);
                return;
            }

            // Очищаем текущую логическую матрицу
            _gridOccupancy = new bool[_gridDimensions.x, _gridDimensions.y];
            Debug.Log($"[ItemContainer:{name}] SetupLoadedItemsGrid: Очищена логическая сетка. Размеры: {_gridDimensions.x}x{_gridDimensions.y}.", this);

            // Проходим по всем предметам и помечаем занятые ячейки
            foreach (var item in _itemDataStorageSO.Items)
            {
                if (item != null)
                {
                    OccupyGridCells(item, true);
                    Debug.Log($"[ItemContainer:{name}] SetupLoadedItemsGrid: Предмет '{item.DefinitionName}' установлен на позицию {item.GridPosition}.");
                }
            }
            OnGridOccupancyChanged?.Invoke();
            Debug.Log($"[ItemContainer:{name}] SetupLoadedItemsGrid: Логическая сетка инвентаря перестроена для {ItemDataStorageSO.Items.Count} предметов.", this);
        }

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
                item.Dimensions.DefaultWidth * item.Dimensions.DefaultHeight).ToList();

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

            foreach (var item in itemsCopy) Destroy(item);
        }

        public IReadOnlyList<ItemBaseDefinition> GetItems()
        {
            return _itemDataStorageSO.Items.AsReadOnly();
        }

        public bool TryAddItemAtPosition(ItemBaseDefinition item, Vector2Int gridPosition)
        {
            //Debug.Log($"[ItemContainer:{name}] TryAddItemAtPosition: Попытка добавить '{item.name}' ({item.Dimensions.CurrentWidth}x{item.Dimensions.CurrentHeight}) на позицию {gridPosition}.", this);
            if (item == null)
            {
                //Debug.LogWarning($"[ItemContainer:{name}] TryAddItemAtPosition: Предмет равен null.", this);
                return false;
            }
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.CurrentWidth, item.Dimensions.CurrentHeight);

            //Debug.Log($"[ItemContainer:{name}] TryAddItemAtPosition: Проверяем доступность области {gridPosition} с размером {itemGridSize} с помощью IsGridAreaFree.", this);
            if (IsGridAreaFree(gridPosition, itemGridSize))
            {
                //Debug.Log($"[ItemContainer:{name}] TryAddItemAtPosition: Область свободна.", this);
                item.GridPosition = gridPosition;
                OccupyGridCells(item, true);
                _itemDataStorageSO.Items.Add(item);
                OnItemAdded?.Invoke(item);
                return true;
            }
            //Debug.LogWarning($"[ItemContainer:{name}] TryAddItemAtPosition: Область занята или выходит за границы.", this);
            return false;
        }

        public void MoveItem(ItemBaseDefinition item, Vector2Int newPosition)
        {
            OccupyGridCells(item, false);
            item.GridPosition = newPosition;
            OccupyGridCells(item, true);
            OnItemRemoved?.Invoke(item);
            OnItemAdded?.Invoke(item);
        }


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
            var size = new Vector2Int(item.Dimensions.DefaultWidth, item.Dimensions.DefaultHeight);
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
            OnGridOccupancyChanged?.Invoke();
        }

        public bool IsGridAreaFree(Vector2Int start, Vector2Int size)
        {
            if (start.x < 0 || start.y < 0 || start.x + size.x > _gridDimensions.x || start.y + size.y > _gridDimensions.y)
            {
                //Debug.LogWarning($"[ItemContainer:{name}] IsGridAreaFree: Область ({start.x},{start.y}) с размером ({size.x}x{size.y}) выходит за границы сетки ({_gridDimensions.x}x{_gridDimensions.y}).", this);
                return false;
            }
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    if (_gridOccupancy[start.x + x, start.y + y])
                    {
                        //Debug.LogWarning($"[ItemContainer:{name}] IsGridAreaFree: Ячейка ({start.x + x},{start.y + y}) уже занята.", this);
                        return false;
                    }
                }
            }
            return true;
        }

        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.DefaultWidth, item.Dimensions.DefaultHeight);
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
    }
}