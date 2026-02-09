using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Компонент, управляющий логическим хранением предметов в сетке.
    /// Отвечает за операции добавления, удаления, перемещения предметов,
    /// а также за отслеживание занятости ячеек сетки.
    /// </summary>
    public class ItemContainer : MonoBehaviour
    {
        [Header("Хранилище данных")]
        [SerializeField]
        private ItemContainerDefinition _containerDefinition;
        /// <summary>
        /// ScriptableObject, хранящий данные о предметах в этом контейнере.
        /// </summary>
        public ItemContainerDefinition ContainerDefinition => _containerDefinition;

        [Header("Конфигурация сетки")]
        [Tooltip("Ссылка на UI Document, в котором находится сетка для этого контейнера.")]
        [SerializeField] private UIDocument _uiDocument;
        [Tooltip("Имя корневой панели в UI документе, внутри которой находится элемент 'grid'.")]
        [SerializeField] private string _rootPanelName;
        /// <summary>
        /// Возвращает имя корневой панели UI, связанной с этим контейнером.
        /// </summary>
        public string RootPanelName => _rootPanelName;

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
        [SerializeField]
        [ReadOnly]
        private Rect _gridWorldRect;
        /// <summary>
        /// Возвращает размер одной ячейки сетки в пикселях.
        /// </summary>
        public Vector2 CellSize => _cellSize;

        // --- События для UI ---
        /// <summary>
        /// Событие, вызываемое при добавлении предмета в контейнер.
        /// </summary>
        public event Action<ItemBaseDefinition> OnItemAdded;
        /// <summary>
        /// Событие, вызываемое при удалении предмета из контейнера.
        /// </summary>
        public event Action<ItemBaseDefinition> OnItemRemoved;
        /// <summary>
        /// Событие, вызываемое при полной очистке контейнера.
        /// </summary>
        public event Action OnCleared;
        /// <summary>
        /// Событие, вызываемое при изменении занятости ячеек сетки.
        /// </summary>
        public event Action OnGridOccupancyChanged;

        // --- Логика сетки ---
        private bool[,] _gridOccupancy;


#if UNITY_EDITOR
        /// <summary>
        /// [Контекстное меню редактора] Рассчитывает размеры сетки контейнера (ширина, высота ячеек)
        /// на основе текущего состояния UI. Должен вызываться в режиме Play Mode или при видимом UI.
        /// </summary>
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
            if (_containerDefinition == null)
            {
                //Debug.LogWarning("ItemDataStorageSO не назначен в ItemContainer. Создаем новый пустой ItemDataStorageSO.", this);
                _containerDefinition = ScriptableObject.CreateInstance<ItemContainerDefinition>();
            }
            _containerDefinition = ScriptableObject.Instantiate(_containerDefinition);
            _containerDefinition.ValidateGuid();

            _gridOccupancy = new bool[_gridDimensions.x, _gridDimensions.y];
            //Debug.Log($"[ItemContainer] Awake: Инициализирована _gridOccupancy с размерами: {_gridDimensions.x}x{_gridDimensions.y}", this);
        }


        /// <summary>
        /// Перестраивает логическую сетку контейнера и помечает занятые ячейки на основе загруженных предметов.
        /// Вызывается после загрузки данных из сохранения.
        /// </summary>
        public void SetupLoadedItemsGrid()
        {
            if (_gridDimensions.x <= 0 || _gridDimensions.y <= 0)
                return;

            _gridOccupancy = new bool[_gridDimensions.x, _gridDimensions.y];
            //Debug.Log($"[ItemContainer:{name}] SetupLoadedItemsGrid: Очищена логическая сетка. Размеры: {_gridDimensions.x}x{_gridDimensions.y}.", this);

            // Проходим по всем предметам и помечаем занятые ячейки
            foreach (var item in _containerDefinition.Items)
            {
                if (item != null)
                {
                    OccupyGridCells(item, true);
                    //Debug.Log($"[ItemContainer:{name}] SetupLoadedItemsGrid: Предмет '{item.DefinitionName}' установлен на позицию {item.GridPosition}.");
                }
            }
            OnGridOccupancyChanged?.Invoke();
            //Debug.Log($"[ItemContainer:{name}] SetupLoadedItemsGrid: Логическая сетка инвентаря перестроена для {ItemDataStorageSO.Items.Count} предметов.", this);
        }

        /// <summary>
        /// Добавляет клонированные предметы в контейнер. Клонирует каждый предмет из списка шаблонов.
        /// </summary>
        /// <param name="itemTemplates">Список шаблонов предметов для добавления.</param>
        /// <returns>Список предметов, которые не удалось разместить в контейнере.</returns>
        internal List<ItemBaseDefinition> AddClonedItems(List<ItemBaseDefinition> itemTemplates)
        {
            if (itemTemplates == null || !itemTemplates.Any())
                return new List<ItemBaseDefinition>();

            var clones = itemTemplates.Select(template => Instantiate(template)).ToList();
            return AddItems(clones);
        }

        /// <summary>
        /// Добавляет список предметов в контейнер, обрабатывая их стакинг и размещение.
        /// </summary>
        /// <param name="itemsToAdd">Список предметов для добавления.</param>
        /// <returns>Список предметов, которые не удалось разместить в контейнере.</returns>
        internal List<ItemBaseDefinition> AddItems(List<ItemBaseDefinition> itemsToAdd)
        {
            if (itemsToAdd == null)
                return new List<ItemBaseDefinition>();

            HandleStacking(itemsToAdd);

            var remainingItems = itemsToAdd.Where(i => i != null && i.Stack > 0).ToList();
            if (!remainingItems.Any())
                return new List<ItemBaseDefinition>();

            var sortedItems = remainingItems.OrderByDescending(item =>
                item.Dimensions.Width * item.Dimensions.Height).ToList();

            List<ItemBaseDefinition> unplacedItems = new List<ItemBaseDefinition>();

            foreach (var item in sortedItems)
            {
                if (TryFindPlacement(item, out var foundPosition))
                {
                    Debug.Log($"В контейнер добавится : {item} с поворотом {item.Dimensions.Angle}");
                    item.GridPosition = foundPosition;
                    OccupyGridCells(item, true);
                    _containerDefinition.Items.Add(item);
                    OnItemAdded?.Invoke(item);
                }
                else
                {
                    unplacedItems.Add(item);
                }
            }
            return unplacedItems;
        }

        /// <summary>
        /// Удаляет указанный предмет из контейнера.
        /// </summary>
        /// <param name="item">Предмет для удаления.</param>
        /// <param name="destroy">Если true, объект предмета будет уничтожен после удаления.</param>
        /// <returns>True, если предмет успешно удален; иначе false.</returns>
        internal bool RemoveItem(ItemBaseDefinition item, bool destroy = true)
        {
            if (item == null) return false;

            bool removed = _containerDefinition.Items.Remove(item);
            if (removed)
            {
                OccupyGridCells(item, false);
                OnItemRemoved?.Invoke(item);
                if (destroy) Destroy(item);
            }
            return removed;
        }

        /// <summary>
        /// Полностью очищает контейнер от всех предметов.
        /// </summary>
        internal void Clear()
        {
            var itemsCopy = _containerDefinition.Items.ToList();
            _containerDefinition.Items.Clear();
            if (_gridOccupancy != null)
                Array.Clear(_gridOccupancy, 0, _gridOccupancy.Length);

            OnCleared?.Invoke();

            foreach (var item in itemsCopy) Destroy(item);
        }

        /// <summary>
        /// Возвращает список предметов, находящихся в контейнере.
        /// </summary>
        /// <returns>Список предметов только для чтения.</returns>
        internal IReadOnlyList<ItemBaseDefinition> GetItems()
        {
            //TODO переделать выдачу листа предметов
            return _containerDefinition.Items.AsReadOnly();
        }

        /// <summary>
        /// Ищет предмет в контейнере по его WrapperIndex.
        /// </summary>
        /// <param name="itemID">WrapperIndex искомого предмета.</param>
        /// <returns>Найденный ItemBaseDefinition или null, если предмет не найден.</returns>
        /// <summary>
        /// Ищет предмет в контейнере по его WrapperIndex.
        /// </summary>
        /// <param name="itemID">WrapperIndex искомого предмета.</param>
        /// <returns>Найденный ItemBaseDefinition или null, если предмет не найден.</returns>
        internal ItemBaseDefinition GetItemByItemID(int itemID)
        {
            if (_containerDefinition == null || _containerDefinition.Items == null)
            {
                //Debug.LogWarning("[ItemContainer] Попытка получить предмет по ID из неинициализированного хранилища.");
                return null;
            }

            foreach (var item in _containerDefinition.Items)
            {
                if (item != null && item.ID == itemID)
                {
                    return item;
                }
            }
            //Debug.LogWarning($"[ItemContainer] Предмет с WrapperIndex '{itemID}' не найден в контейнере '{name}'.");
            return null;
        }

        /// <summary>
        /// Пытается добавить предмет в контейнер по указанной позиции в сетке.
        /// </summary>
        /// <param name="item">Предмет для добавления.</param>
        /// <param name="gridPosition">Позиция в сетке, куда нужно добавить предмет.</param>
        /// <returns>True, если предмет успешно добавлен; иначе false.</returns>
        internal bool TryAddItemAtPosition(ItemBaseDefinition item, Vector2Int gridPosition)
        {
            //Debug.Log($"[ItemContainer:{name}] TryAddItemAtPosition: Попытка добавить '{item.name}' ({item.Dimensions.Width}x{item.Dimensions.Width}) на позицию {gridPosition}.", this);
            if (item == null)
            {
                //Debug.LogWarning($"[ItemContainer:{name}] TryAddItemAtPosition: Предмет равен null.", this);
                return false;
            }
            Vector2Int itemGridSize = new Vector2Int(item.Dimensions.Width, item.Dimensions.Height);

            //Debug.Log($"[ItemContainer:{name}] TryAddItemAtPosition: Проверяем доступность области {gridPosition} с размером {itemGridSize} с помощью IsGridAreaFree.", this);
            if (IsGridAreaFree(gridPosition, itemGridSize))
            {
                //Debug.Log($"[ItemContainer:{name}] TryAddItemAtPosition: Область свободна.", this);
                item.GridPosition = gridPosition;
                OccupyGridCells(item, true);
                _containerDefinition.Items.Add(item);
                OnItemAdded?.Invoke(item);
                return true;
            }
            //Debug.LogWarning($"[ItemContainer:{name}] TryAddItemAtPosition: Область занята или выходит за границы.", this);
            return false;
        }

        /// <summary>
        /// Перемещает существующий предмет в новую позицию в сетке контейнера.
        /// </summary>
        /// <param name="item">Предмет для перемещения.</param>
        /// <param name="newPosition">Новая позиция в сетке.</param>
        internal void MoveItem(ItemBaseDefinition item, Vector2Int newPosition)
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
                foreach (var existingItem in _containerDefinition.Items)
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

        /// <summary>
        /// Отмечает ячейки сетки как занятые или свободные для указанного предмета.
        /// </summary>
        /// <param name="item">Предмет, чьи ячейки необходимо отметить.</param>
        /// <param name="occupy">True, чтобы отметить как занятые; false, чтобы отметить как свободные.</param>
        public void OccupyGridCells(ItemBaseDefinition item, bool occupy)
        {
            if (item.GridPosition.x < 0 || item.GridPosition.y < 0) return;

            var size = new Vector2Int(item.Dimensions.Width, item.Dimensions.Height);
            //Debug.Log($"[ItemContainer] OccupyGridCells вызван для '{item.DefinitionName}'. Действие: {(occupy ? "ЗАНЯТЬ" : "ОСВОБОДИТЬ")}. Позиция: {item.GridPosition}, Размер: {size}");
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int gridX = item.GridPosition.x + x;
                    int gridY = item.GridPosition.y + y;
                    if (gridX < _gridDimensions.x && gridY < _gridDimensions.y && gridX >= 0 && gridY >= 0)
                        _gridOccupancy[gridX, gridY] = occupy;
                }
            }

            //LogGridState();
            //void LogGridState()
            //{
            //    Debug.Log($"--- Состояние логической сетки ({name}) {_gridDimensions.x}x{_gridDimensions.y} ---");
            //    for (int y = _gridDimensions.y - 1; y >= 0; y--)
            //    {
            //        string row = "";
            //        for (int x = 0; x < _gridDimensions.x; x++)
            //        {
            //            row += _gridOccupancy[x, y] ? "1 " : "0 ";
            //        }
            //        Debug.Log(row);
            //    }
            //    Debug.Log("-----------------------------------------");
            //}

            OnGridOccupancyChanged?.Invoke();
        }

        /// <summary>
        /// Вспомогательный метод для проверки, свободна ли указанная область в сетке.
        /// </summary>
        /// <param name="start">Начальная позиция области в сетке.</param>
        /// <param name="size">Размер области.</param>
        /// <returns>True, если область свободна и находится в пределах сетки; иначе false.</returns>
        private bool CheckGridArea(Vector2Int start, Vector2Int size)
        {
            // Проверка на выход за границы сетки
            if (start.x < 0 || start.y < 0 || start.x + size.x > _gridDimensions.x || start.y + size.y > _gridDimensions.y)
            {
                Debug.LogWarning($"[ItemContainer:{name}] CheckGridArea: Область ({start.x},{start.y}) с размером ({size.x}x{size.y}) выходит за границы сетки ({_gridDimensions.x}x{_gridDimensions.y}).", this);
                return false;
            }

            // Проверка на занятость ячеек
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    if (_gridOccupancy[start.x + x, start.y + y])
                    {
                        Debug.LogWarning($"[ItemContainer:{name}] CheckGridArea: Ячейка ({start.x + x},{start.y + y}) уже занята.", this);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Проверяет, свободна ли указанная область в сетке.
        /// </summary>
        /// <param name="start">Начальная позиция области в сетке.</param>
        /// <param name="size">Размер области.</param>
        /// <param name="allowRotation">Разрешить ли проверку повернутого варианта предмета.</param>
        /// <returns>True, если область свободна и находится в пределах сетки (возможно, с поворотом); иначе false.</returns>
        public bool IsGridAreaFree(Vector2Int start, Vector2Int size, bool allowRotation = true)
        {
            // Проверяем оригинальный размер
            if (CheckGridArea(start, size))
            {
                return true;
            }

            // Если разрешено вращение и предмет не квадратный, проверяем повернутый размер
            if (allowRotation && size.x != size.y)
            {
                Vector2Int rotatedSize = new Vector2Int(size.y, size.x);
                Debug.Log($"проверка для размера : {rotatedSize}");
                if (CheckGridArea(start, rotatedSize))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Ищет первое свободное место в сетке для указанного предмета.
        /// </summary>
        /// <param name="item">Предмет, для которого ищется место.</param>
        /// <param name="suggestedGridPosition">Найденная свободная позиция в сетке.</param>
        /// <returns>True, если свободное место найдено; иначе false.</returns>
        public bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int suggestedGridPosition)
        {
            Vector2Int originalItemGridSize = new Vector2Int(item.Dimensions.Width, item.Dimensions.Height);
            Vector2Int rotatedItemGridSize = new Vector2Int(item.Dimensions.Height, item.Dimensions.Width);

            // Определяем максимальные размеры, чтобы циклы поиска не выходили за пределы сетки
            int searchGridWidth = _gridDimensions.x - Math.Min(originalItemGridSize.x, rotatedItemGridSize.x) + 1;
            int searchGridHeight = _gridDimensions.y - Math.Min(originalItemGridSize.y, rotatedItemGridSize.y) + 1;

            if (searchGridWidth <= 0 || searchGridHeight <= 0)
            {
                suggestedGridPosition = Vector2Int.zero;
                return false;
            }

            for (int y = 0; y < searchGridHeight; y++)
            {
                for (int x = 0; x < searchGridWidth; x++)
                {
                    var currentPos = new Vector2Int(x, y);

                    // Проверяем оригинальный размер
                    if (CheckGridArea(currentPos, originalItemGridSize))
                    {
                        suggestedGridPosition = currentPos;
                        return true;
                    }

                    // Если предмет не квадратный и allowRotation было true в IsGridAreaFree (что по умолчанию так),
                    // то TryFindPlacement должен принимать это во внимание.
                    // Однако, IsGridAreaFree сам решает, проверять ли rotatedSize.
                    // Здесь нам нужно явно проверить rotatedSize и, если подходит, сохранить его.
                    if (originalItemGridSize.x != originalItemGridSize.y && CheckGridArea(currentPos, rotatedItemGridSize))
                    {
                        item.Dimensions.Width = rotatedItemGridSize.x;
                        item.Dimensions.Height = rotatedItemGridSize.y;
                        item.Dimensions.Angle = 90;
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