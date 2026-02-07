using SkyClerik.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.EquipmentSystem
{
    /// <summary>
    /// Конкретная реализация UI-страницы для отображения слотов экипировки игрока.
    /// </summary>
    public class EquipPage : MonoBehaviour
    {
        private const string _titleText = "Окно Экипировки";
        private VisualElement _header;
        private const string _headerID = "header";
        private Label _title;
        private const string _titleID = "l_title";

        internal VisualElement _inventoryGrid;
        private const string _inventoryGridID = "grid";

        [Header("Хранилище данных")]
        [SerializeField]
        [SerializeReference]
        private List<EquipmentSlot> _equipSlots = new List<EquipmentSlot>();
        public List<EquipmentSlot> EquipmentSlots => _equipSlots;


        [Header("Конфигурация")]

        [Tooltip("Ссылка на UI Document, в котором находятся слоты для экипировки.")]
        [SerializeField]
        [ReadOnly]
        private UIDocument _uiDocument;

        [Tooltip("Имя корневой панели в UI документе, внутри которой находится элемент 'grid'.")]
        [SerializeField]
        private string _rootPanelName;

        private ItemsPage _itemsPage;

        // UI-элементы
        internal VisualElement _EquipRoot;
        private PlacementResults _placementResults;
        private List<VisualElement> _visualSlots;

        private static bool _isShow;
        public static bool IsShow { get => _isShow; set => _isShow = value; }

        /// <summary>
        /// Обрабатывает обратную связь при перетаскивании для слотов экипировки.
        /// </summary>
        /// <param name="draggedItem">Перетаскиваемый предмет.</param>
        /// <param name="mousePosition">Текущая позиция мыши в мировых координатах UI.</param>
        /// <returns>Результаты размещения, или null, если мышь не над слотом экипировки.</returns>
        public PlacementResults ProcessDragFeedback(ItemVisual draggedItem, Vector2 mousePosition)
        {
            if (_EquipRoot == null || !_EquipRoot.enabledSelf || _EquipRoot.resolvedStyle.display == DisplayStyle.None || _EquipRoot.resolvedStyle.visibility == Visibility.Hidden)
            {
                ClearAllTelegraphs();
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null); // Возвращаем некорректный результат
            }

            EquipmentSlot hoveredSlot = null;
            foreach (var equipSlot in _equipSlots)
            {
                if (equipSlot.Rect.Contains(mousePosition))
                {
                    hoveredSlot = equipSlot;
                    break;
                }
            }

            if (hoveredSlot != null)
            {
                // Мышь находится над слотом, делегируем ему показ телеграфа
                PlacementResults results = hoveredSlot.ShowPlacementTarget(draggedItem);
                // Скрываем телеграфы всех других слотов
                foreach (var equipSlot in _equipSlots)
                {
                    if (equipSlot != hoveredSlot)
                    {
                        equipSlot.FinalizeDrag(); // Скрывает телеграф
                    }
                }
                return results;
            }
            else
            {
                // Мышь не над каким-либо слотом, скрываем все телеграфы
                ClearAllTelegraphs();
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null); // Возвращаем некорректный результат
            }
        }

        /// <summary>
        /// Скрывает все телеграфы для всех слотов экипировки.
        /// </summary>
        public void ClearAllTelegraphs()
        {
            foreach (var equipSlot in _equipSlots)
            {
                equipSlot.FinalizeDrag();
            }
        }

        private void OnValidate()
        {
            _uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
        }

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        private void Start()
        {
            _itemsPage = ServiceProvider.Get<ItemsPage>();
            _EquipRoot = _uiDocument.rootVisualElement.Q<VisualElement>(_rootPanelName);
            _inventoryGrid = _EquipRoot.Q<VisualElement>(_inventoryGridID);
            _header = _EquipRoot.Q(_headerID);
            _title = _header.Q<Label>(_titleID);
            _title.text = _titleText;

            StartCoroutine(Initialize());
        }

        private void Update()
        {
            if (ItemsPage.CurrentDraggedItem != null)
            {
                ProcessDragFeedback(ItemsPage.CurrentDraggedItem, _itemsPage.MouseUILocalPosition);
            }
        }

        protected IEnumerator Initialize()
        {
            while (_inventoryGrid == null || _inventoryGrid.resolvedStyle.width == 0 || _inventoryGrid.childCount == 0 || _itemsPage.ContainersAndPages.Count == 0)
                yield return null;

            _visualSlots = _inventoryGrid.Children().ToList();
            FirstLoadInitialVisuals();
        }

        protected void FirstLoadInitialVisuals()
        {
            // Обеспечиваем прямое соответствие между визуальными слотами и слотами данных
            for (int i = 0; i < _equipSlots.Count && i < _visualSlots.Count; i++)
            {
                var cell = _visualSlots[i];
                var equipSlot = _equipSlots[i];

                if (cell == null)
                {
                    // Debug.LogWarning($"Визуальная ячейка по индексу {i} равна null. Пропускаем ее.", this);
                    continue;
                }
                if (equipSlot == null)
                {
                    // Debug.LogWarning($"Данные слота экипировки по индексу {i} равны null. Пропускаем их.", this);
                    continue;
                }

                equipSlot.Cell = cell;
                equipSlot.InitializeDocumentAndTelegraph(_uiDocument); // Инициализируем телеграф для этого слота
            }

            // Пытаемся экипировать предметы, которые уже определены в данных EquipmentSlot
            foreach (EquipmentSlot slot in _equipSlots)
            {
                if (slot.EquippedItem != null)
                {
                    // Просим EquipmentSlot создать ItemVisual для экипированного предмета
                    var newItemVisual = slot.CreateItemVisualForSlot(slot.EquippedItem, _itemsPage);
                    // Экипируем созданный ItemVisual
                    slot.Drop(newItemVisual, Vector2Int.zero);
                }
            }
        }
        private bool TryGetTargetSlot(Vector2 position, out EquipmentSlot equipmentSlot)
        {
            equipmentSlot = null;

            foreach (var slot in _equipSlots)
            {
                if (slot.Rect.Contains(position))
                {
                    equipmentSlot = slot;
                    return true;
                }
            }

            return false;
        }

        public void OpenEquip()
        {
            EquipPage.IsShow = true;
            _itemsPage.OpenInventoryNormal();
            _EquipRoot.SetDisplay(EquipPage.IsShow);
        }

        public void CloseEquip()
        {
            EquipPage.IsShow = false;
            _itemsPage.CloseAll();
            _EquipRoot.SetDisplay(EquipPage.IsShow);
        }

        ///// <summary>
        ///// Обрабатывает перемещение предмета из ItemContainer в слот экипировки.
        ///// </summary>
        //internal void HandleItemTransfer(ItemVisual draggedItem, ItemContainer sourceContainer, EquipmentSlot targetEquipSlot)
        //{
        //    var itemToEquip = draggedItem.ItemDefinition;

        //    Debug.Log($"[ЭКИПИРОВКА][HandleItemTransfer] Начало. Перетаскиваемый предмет: '{itemToEquip.name}' из '{sourceContainer.name}'. Целевой слот: '{targetEquipSlot.Cell.name}'.");

        //    // Проверяем, может ли draggedItem быть помещен в целевой слот
        //    if (!targetEquipSlot.CanEquip(itemToEquip))
        //    {
        //        Debug.LogWarning($"[ЭКИПИРОВКА][HandleItemTransfer] Предмет '{itemToEquip.name}' не может быть экипирован в слот '{targetEquipSlot.Cell.name}'. Операция отменена.");
        //        // draggedItem все еще "в руке", и система ItemsPage должна вернуть его обратно в исходный инвентарь.
        //        return;
        //    }

        //    // Сохраняем ItemVisual, который находится в целевом слоте, во временное хранилище
        //    ItemVisual itemInTargetSlotVisual = targetEquipSlot.ItemVisual;
        //    //ItemBaseDefinition itemDefinitionFromTargetSlot = null; // Для хранения снятого ItemBaseDefinition

        //    // Удаляем предмет из исходного контейнера (инвентаря)
        //    sourceContainer.RemoveItem(itemToEquip, destroy: false);
        //    // Debug.Log($"[ИНВЕНТАРЬ][EquipPage] Предмет '{itemToEquip.name}' был забран из контейнера '{sourceContainer.name}'."); // Закомментировано

        //    // Если слот экипировки занят, "поднимаем" оттуда текущий предмет
        //    if (itemInTargetSlotVisual != null)
        //    {
        //        Debug.Log($"[ЭКИПИРОВКА][HandleItemTransfer] В целевом слоте '{targetEquipSlot.Cell.name}' был предмет '{itemInTargetSlotVisual.ItemDefinition.name}'. Снимаем его.");
        //        //itemDefinitionFromTargetSlot = targetEquipSlot.Unequip(); // Сохраняем снятый ItemBaseDefinition
        //        targetEquipSlot.Unequip();
        //    } else {
        //        Debug.Log($"[ЭКИПИРОВКА][HandleItemTransfer] Целевой слот '{targetEquipSlot.Cell.name}' пуст.");
        //    }

        //    // Экипируем новый предмет в слот
        //    targetEquipSlot.Equip(draggedItem);
        //    Debug.Log($"[ЭКИПИРОВКА][HandleItemTransfer] Предмет '{itemToEquip.name}' успешно экипирован в слот '{targetEquipSlot.Cell.name}'.");
        //}
        ///// <summary>
        ///// Обрабатывает свап или перемещение предмета между слотами экипировки.
        ///// </summary>
        //internal void HandleEquipSlotTransfer(ItemVisual initialDraggedItem, EquipmentSlot sourceEquipSlot, EquipmentSlot targetEquipSlot)
        //{
        //    // initialDraggedItem - это предмет, который ты *изначально* начал тащить (из sourceEquipSlot).
        //    // ItemsPage.CurrentDraggedItem - это тот ItemVisual, который был в targetEquipSlot и теперь "в руке".
        //    // Мы получили его через itemToSwap.PickUp(true) в HandleSwap().

        //    Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Начало обмена между слотами экипировки.");
        //    Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Исходный слот: '{sourceEquipSlot.Cell.name}'. Целевой слот: '{targetEquipSlot.Cell.name}'.");
        //    Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Перетаскиваемый предмет (initialDraggedItem): '{initialDraggedItem.ItemDefinition.name}'.");
            
        //    ItemVisual itemFromTargetSlotVisual = ItemsPage.CurrentDraggedItem;
        //    Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Предмет из целевого слота (itemFromTargetSlotVisual): '{itemFromTargetSlotVisual?.ItemDefinition.name ?? "ПУСТО"}' (текущий dragged item в ItemsPage).");


        //    // 1. Проверяем, может ли initialDraggedItem быть помещен в целевой слот
        //    Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Проверка: может ли '{initialDraggedItem.ItemDefinition.name}' быть экипирован в '{targetEquipSlot.Cell.name}'?");
        //    if (!targetEquipSlot.CanEquip(initialDraggedItem.ItemDefinition))
        //    {
        //        Debug.LogWarning($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Невозможно поместить '{initialDraggedItem.ItemDefinition.name}' в слот '{targetEquipSlot.Cell.name}'. Отмена обмена.");
        //        // Если нет, возвращаем initialDraggedItem обратно в исходный слот
        //        sourceEquipSlot.Equip(initialDraggedItem);
        //        Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] '{initialDraggedItem.ItemDefinition.name}' возвращен в исходный слот '{sourceEquipSlot.Cell.name}'.");
        //        // И возвращаем itemFromTargetSlotVisual обратно в targetEquipSlot
        //        if (itemFromTargetSlotVisual != null)
        //        {
        //            targetEquipSlot.Equip(itemFromTargetSlotVisual); // Он сейчас в CurrentDraggedItem
        //            Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] '{itemFromTargetSlotVisual.ItemDefinition.name}' возвращен в целевой слот '{targetEquipSlot.Cell.name}'.");
        //        }
        //        // Debug.LogWarning($"[ЭКИПИРОВКА][EquipPage] Невозможно поместить '{initialDraggedItem.ItemDefinition.name}' в слот '{targetEquipSlot.Cell.name}'. Предметы возвращены."); // Закомментировано
        //        return;
        //    }

        //    // 2. Проверяем, может ли itemFromTargetSlotVisual быть помещен в исходный слот
        //    if (itemFromTargetSlotVisual != null)
        //    {
        //        Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Проверка: может ли '{itemFromTargetSlotVisual.ItemDefinition.name}' быть экипирован в '{sourceEquipSlot.Cell.name}'?");
        //        if (!sourceEquipSlot.CanEquip(itemFromTargetSlotVisual.ItemDefinition))
        //        {
        //            Debug.LogWarning($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Невозможен обмен: предмет '{itemFromTargetSlotVisual.ItemDefinition.name}' из целевого слота не может быть помещен в исходный. Отмена обмена.");
        //            // Если нет, то обмен невозможен. Возвращаем initialDraggedItem.
        //            sourceEquipSlot.Equip(initialDraggedItem);
        //            Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] '{initialDraggedItem.ItemDefinition.name}' возвращен в исходный слот '{sourceEquipSlot.Cell.name}'.");
        //            // И возвращаем itemFromTargetSlotVisual обратно в targetEquipSlot
        //            targetEquipSlot.Equip(itemFromTargetSlotVisual);
        //            Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] '{itemFromTargetSlotVisual.ItemDefinition.name}' возвращен в целевой слот '{targetEquipSlot.Cell.name}'.");
        //            // Debug.LogWarning($"[ЭКИПИРОВКА][EquipPage] Невозможен обмен: предмет '{itemFromTargetSlotVisual.ItemDefinition.name}' из целевого слота не может быть помещен в исходный. Предметы возвращены."); // Закомментировано
        //            return;
        //        }
        //    } else {
        //        Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Целевой слот '{targetEquipSlot.Cell.name}' пуст, обмен не требуется для itemFromTargetSlotVisual.");
        //    }

        //    // --- ОБМЕН ---
        //    // Debug.Log($"[ЭКИПИРОВКА][EquipPage] Выполняется обмен. '{initialDraggedItem.ItemDefinition.name}' -> '{targetEquipSlot.Cell.name}' и '{itemFromTargetSlotVisual?.ItemDefinition.name}' -> '{sourceEquipSlot.Cell.name}'."); // Закомментировано
        //    Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Все проверки пройдены. Выполняем обмен.");

        //    // Шаг 1: Помещаем initialDraggedItem в targetEquipSlot.
        //    targetEquipSlot.Equip(initialDraggedItem);
        //    Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] '{initialDraggedItem.ItemDefinition.name}' успешно экипирован в '{targetEquipSlot.Cell.name}'.");

        //    // Шаг 2: Помещаем itemFromTargetSlotVisual в sourceEquipSlot.
        //    if (itemFromTargetSlotVisual != null)
        //    {
        //        sourceEquipSlot.Equip(itemFromTargetSlotVisual);
        //        Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] '{itemFromTargetSlotVisual.ItemDefinition.name}' успешно экипирован в '{sourceEquipSlot.Cell.name}'.");
        //    } else {
        //        Debug.Log($"[ЭКИПИРОВКА][HandleEquipSlotTransfer] Исходный слот '{sourceEquipSlot.Cell.name}' был пуст, предмет не возвращается.");
        //    }

        //    Debug.Log("[ЭКИПИРОВКА][HandleEquipSlotTransfer] Обмен завершен.");
        //}
        //public PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        //{
        //    if (_root == null || !_root.enabledSelf || _root.resolvedStyle.display == DisplayStyle.None || _root.resolvedStyle.visibility == Visibility.Hidden)
        //    {
        //        return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
        //    }

        //    Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);
        //    if (TryGetTargetSlot(mouseLocalPosition, out EquipmentSlot targetSlot))
        //    {
        //        // Курсор находится над слотом
        //        // Вместо того, чтобы самому EquipPage обрабатывать логику размещения,
        //        // мы делегируем это найденному EquipmentSlot, который теперь является IDropTarget.
        //        return targetSlot.ShowPlacementTarget(draggedItem);
        //    }
        //    return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
        //}

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

                // Добавляем проверку, что _inventoryGrid имеет размеры
                if (inventoryGrid.resolvedStyle.width == 0 || inventoryGrid.resolvedStyle.height == 0)
                {
                    Debug.LogWarning("UI-элементы еще не отрисованы, откладываем расчет...", this);
                    // Если размеры еще не получены, откладываем выполнение еще раз.
                    // Можно сделать это более надежно, используя корутину, но для ContextMenu
                    // откладывание - это простой способ.
                    root.schedule.Execute(() => CalculateGridDimensionsFromUI()).ExecuteLater(1); // Рекурсивный вызов
                    return;
                }

                if (inventoryGrid.childCount == 0)
                {
                    Debug.LogWarning($"Сетка '{inventoryGrid.name}' не содержит дочерних элементов (ячеек). Невозможно определить размер ячейки.", this);
                    return;
                }

                var allCell = inventoryGrid.Children().ToList();

                _equipSlots.Clear();
                foreach (var visualElement in allCell)
                {
                    var calculatedCellSize = new Vector2(visualElement.resolvedStyle.width, visualElement.resolvedStyle.height);

                    if (calculatedCellSize.x > 0 && calculatedCellSize.y > 0)
                    {
                        var rect = new Rect(visualElement.worldBound.x, visualElement.worldBound.y, visualElement.resolvedStyle.width, visualElement.resolvedStyle.height);
                        var slotData = new EquipmentSlot(rect: rect, document: _uiDocument);
                        _equipSlots.Add(slotData);
                    }
                }
                UnityEditor.EditorUtility.SetDirty(this);
            }).ExecuteLater(1);
        }
#endif
    }
}