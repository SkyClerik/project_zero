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
        internal VisualElement _root;
        private PlacementResults _placementResults;
        private List<VisualElement> _visualSlots;

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
            _root = _uiDocument.rootVisualElement.Q<VisualElement>(_rootPanelName);
            _inventoryGrid = _root.Q<VisualElement>(_inventoryGridID);
            _header = _root.Q(_headerID);
            _title = _header.Q<Label>(_titleID);
            _title.text = _titleText;

            StartCoroutine(Initialize());
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
            for (int i = 0; i < _equipSlots.Count; i++)
            {
                if (i < _visualSlots.Count)
                {
                    var cell = _visualSlots[i];
                    Vector2 cellPosition = new Vector2(cell.worldBound.x + 1, cell.worldBound.y + 1);
                    var equipSlot = _equipSlots[i];

                    if (TryGetTargetSlot(cellPosition, out equipSlot))
                    {
                        if (cell == null)
                        {
                            Debug.LogWarning($"Ячейка UI по индексу {i} равна null. Пропускаем ее.", this);
                            continue;
                        }
                        if (equipSlot == null)
                        {
                            Debug.LogWarning($"Слот экипировки по индексу {i} равен null в EquipmentContainerDefinition. Пропускаем его.", this);
                            continue;
                        }
                        equipSlot.Cell = cell;
                        //equipSlot.GetDocument = _uiDocument; // Устанавливаем UIDocument в существующий слот
                        equipSlot.InitializeTelegraph(_uiDocument); // Инициализируем телеграф для этого слота
                    }
                }
            }

            // В каждый созданный EquipmentSlot пробуем положить его содержимое
            foreach (EquipmentSlot slot in _equipSlots)
            {
                if (slot.EquippedItem != null)
                {
                    // Просим EquipmentSlot создать ItemVisual для экипированного предмета
                    var newItemVisual = slot.CreateItemVisualForSlot(slot.EquippedItem, _itemsPage);
                    // Экипируем полученный ItemVisual
                    slot.Equip(newItemVisual);
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

        public void RemoveStoredItem(EquipmentSlot equipmentSlot)
        {
            if (equipmentSlot != null)
                equipmentSlot.Unequip(); // Изменено с equipmentSlot.Unequip(_uiDocument);
        }

        public void OpenEquip()
        {
            _itemsPage.OpenInventoryNormal();
            _root.SetDisplay(true);
        }

        /// <summary>
        /// Обрабатывает перемещение предмета из ItemContainer в слот экипировки.
        /// </summary>
        internal void HandleItemTransfer(ItemVisual draggedItem, ItemContainer sourceContainer, EquipmentSlot targetEquipSlot)
        {
            var itemToEquip = draggedItem.ItemDefinition;

            Debug.Log($"[ЭКИПИРОВКА][EquipPage] Попытка экипировать предмет '{itemToEquip.name}' из '{sourceContainer.name}' в слот экипировки '{targetEquipSlot.Cell.name}'.");

            // Проверяем, может ли draggedItem быть помещен в целевой слот
            if (!targetEquipSlot.CanEquip(itemToEquip))
            {
                Debug.LogWarning($"[ЭКИПИРОВКА][EquipPage] Предмет '{itemToEquip.name}' не может быть экипирован в слот '{targetEquipSlot.Cell.name}'. Операция отменена.");
                // draggedItem все еще "в руке", и система ItemsPage должна вернуть его обратно в исходный инвентарь.
                return;
            }

            // Сохраняем ItemVisual, который находится в целевом слоте, во временное хранилище
            ItemVisual itemInTargetSlotVisual = targetEquipSlot.ItemVisual;
            ItemBaseDefinition itemDefinitionFromTargetSlot = null; // Для хранения снятого ItemBaseDefinition

            // Удаляем предмет из исходного контейнера (инвентаря)
            sourceContainer.RemoveItem(itemToEquip, destroy: false);
            Debug.Log($"[ИНВЕНТАРЬ][EquipPage] Предмет '{itemToEquip.name}' был забран из контейнера '{sourceContainer.name}'.");

            // Если слот экипировки занят, "поднимаем" оттуда текущий предмет
            if (itemInTargetSlotVisual != null)
            {
                Debug.Log($"[ЭКИПИРОВКА][EquipPage] Слот экипировки '{targetEquipSlot.Cell.name}' занят предметом '{itemInTargetSlotVisual.ItemDefinition.name}'. Поднимаем его.");
                itemDefinitionFromTargetSlot = targetEquipSlot.Unequip(); // Сохраняем снятый ItemBaseDefinition
            }

            // Экипируем новый предмет в слот
            targetEquipSlot.Equip(draggedItem);
            Debug.Log($"[ЭКИПИРОВКА][EquipPage] Предмет '{itemToEquip.name}' успешно экипирован в слот '{targetEquipSlot.Cell.name}'.");


        }

        /// <summary>
        /// Обрабатывает перемещение предмета из слота экипировки в ItemContainer.
        /// </summary>
        internal void HandleItemTransferFromEquip(ItemVisual draggedItem, EquipmentSlot sourceEquipSlot, ItemContainer targetContainer, Vector2Int gridPosition)
        {
            ItemBaseDefinition itemToUnequip = draggedItem.ItemDefinition; // Явно указываем пространство имен

            Debug.Log($"[ЭКИПИРОВКА][EquipPage] Попытка снять предмет '{itemToUnequip.name}' из слота '{sourceEquipSlot.Cell.name}' и поместить в '{targetContainer.name}'.");

            // Снимаем предмет из слота экипировки
            sourceEquipSlot.Unequip();
            Debug.Log($"[ЭКИПИРОВКА][EquipPage] Предмет '{itemToUnequip.name}' был снят из слота '{sourceEquipSlot.Cell.name}'.");

            // Пытаемся добавить предмет в целевой контейнер (инвентарь)
            bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToUnequip, gridPosition);

            if (addedToTarget)
            {
                Debug.Log($"[ИНВЕНТАРЬ][EquipPage] Предмет '{itemToUnequip.name}' был положен в контейнер '{targetContainer.name}' в позицию: {gridPosition}.");
            }
            else
            {
                // Если не удалось добавить в инвентарь, возвращаем его обратно в слот экипировки
                sourceEquipSlot.Equip(draggedItem);
                Debug.LogWarning($"[ЭКИПИРОВКА][EquipPage] Не удалось поместить предмет '{itemToUnequip.name}' в инвентарь '{targetContainer.name}'. Возвращен в слот экипировки.");
            }
        }

        /// <summary>
        /// Обрабатывает свап или перемещение предмета между слотами экипировки.
        /// </summary>
        internal void HandleEquipSlotTransfer(ItemVisual initialDraggedItem, EquipmentSlot sourceEquipSlot, EquipmentSlot targetEquipSlot)
        {
            // initialDraggedItem - это предмет, который ты *изначально* начал тащить (из sourceEquipSlot).
            // ItemsPage.CurrentDraggedItem - это тот ItemVisual, который был в targetEquipSlot и теперь "в руке".
            // Мы получили его через itemToSwap.PickUp(true) в HandleSwap().

            ItemVisual itemFromTargetSlotVisual = ItemsPage.CurrentDraggedItem; 

            // 1. Проверяем, может ли initialDraggedItem быть помещен в целевой слот
            if (!targetEquipSlot.CanEquip(initialDraggedItem.ItemDefinition))
            {
                // Если нет, возвращаем initialDraggedItem обратно в исходный слот
                sourceEquipSlot.Equip(initialDraggedItem);
                // И возвращаем itemFromTargetSlotVisual обратно в targetEquipSlot
                targetEquipSlot.Equip(itemFromTargetSlotVisual); // Он сейчас в CurrentDraggedItem
                Debug.LogWarning($"[ЭКИПИРОВКА][EquipPage] Невозможно поместить '{initialDraggedItem.ItemDefinition.name}' в слот '{targetEquipSlot.Cell.name}'. Предметы возвращены.");
                return;
            }

            // 2. Проверяем, может ли itemFromTargetSlotVisual быть помещен в исходный слот
            if (itemFromTargetSlotVisual != null && !sourceEquipSlot.CanEquip(itemFromTargetSlotVisual.ItemDefinition))
            {
                // Если нет, то обмен невозможен. Возвращаем initialDraggedItem.
                sourceEquipSlot.Equip(initialDraggedItem);
                // И возвращаем itemFromTargetSlotVisual обратно в targetEquipSlot
                targetEquipSlot.Equip(itemFromTargetSlotVisual);
                Debug.LogWarning($"[ЭКИПИРОВКА][EquipPage] Невозможен обмен: предмет '{itemFromTargetSlotVisual.ItemDefinition.name}' из целевого слота не может быть помещен в исходный. Предметы возвращены.");
                return;
            }

            // --- ОБМЕН ---
            Debug.Log($"[ЭКИПИРОВКА][EquipPage] Выполняется обмен. '{initialDraggedItem.ItemDefinition.name}' -> '{targetEquipSlot.Cell.name}' и '{itemFromTargetSlotVisual?.ItemDefinition.name}' -> '{sourceEquipSlot.Cell.name}'.");

            // Шаг 1: Помещаем initialDraggedItem в targetEquipSlot.
            targetEquipSlot.Equip(initialDraggedItem);

            // Шаг 2: Помещаем itemFromTargetSlotVisual в sourceEquipSlot.
            if (itemFromTargetSlotVisual != null)
            {
                sourceEquipSlot.Equip(itemFromTargetSlotVisual);
            }
            
            Debug.Log("[ЭКИПИРОВКА][EquipPage] Обмен завершен.");
        }

        public PlacementResults ShowPlacementTarget(ItemVisual draggedItem)
        {
            if (_root == null || !_root.enabledSelf || _root.resolvedStyle.display == DisplayStyle.None || _root.resolvedStyle.visibility == Visibility.Hidden)
            {
                return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
            }

            Vector2 mouseLocalPosition = _inventoryGrid.WorldToLocal(_itemsPage.MouseUILocalPosition);
            if (TryGetTargetSlot(mouseLocalPosition, out EquipmentSlot targetSlot))
            {
                // Курсор находится над слотом
                // Вместо того, чтобы самому EquipPage обрабатывать логику размещения,
                // мы делегируем это найденному EquipmentSlot, который теперь является IDropTarget.
                return targetSlot.ShowPlacementTarget(draggedItem);
            }
            return _placementResults.Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
        }

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
                        var slotData = new EquipmentSlot(rect: rect, document: _uiDocument); // Передаем _uiDocument
                        _equipSlots.Add(slotData);
                    }
                }
                UnityEditor.EditorUtility.SetDirty(this);
            }).ExecuteLater(1);
        }
#endif
    }
}