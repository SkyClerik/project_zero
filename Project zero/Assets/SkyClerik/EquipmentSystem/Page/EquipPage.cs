using SkyClerik.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        private VisualElement _inventoryGrid;
        private const string _inventoryGridID = "grid";
        private ItemsPage _itemsPage;
        private VisualElement _equipRoot;

        private List<VisualElement> _visualSlots;
        private static bool _isShow;

        [Header("Хранилище данных")]
        [SerializeField]
        [SerializeReference]
        private List<EquipmentSlot> _equipSlots = new List<EquipmentSlot>();

        [Header("Конфигурация")]
        [SerializeField]
        [ReadOnly]
        [Tooltip("Ссылка на UI Document, в котором находятся слоты для экипировки.")]
        private UIDocument _uiDocument;

        [Header("UI-элементы")]
        [SerializeField]
        [Tooltip("Имя корневой панели в UI документе, внутри которой находится элемент 'grid'.")]
        private string _rootPanelName;

        public static bool IsShow { get => _isShow; set => _isShow = value; }



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
            _equipRoot = _uiDocument.rootVisualElement.Q<VisualElement>(_rootPanelName);
            _inventoryGrid = _equipRoot.Q<VisualElement>(_inventoryGridID);

            StartCoroutine(Initialize());
            SystemClosePage();
        }

        private void Update()
        {
            if (!_isShow)
                return;

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
                equipSlot.InitializeDocumentAndTelegraph(_uiDocument);
            }

            foreach (EquipmentSlot slot in _equipSlots)
            {
                if (slot.EquippedItem != null)
                {
                    var newItemVisual = slot.CreateItemVisualFromSlot(slot.EquippedItem, _itemsPage);
                    slot.Drop(newItemVisual, Vector2Int.zero);
                }
            }
        }

        public void OpenEquip()
        {
            _itemsPage.CloseAll();
            _itemsPage.OpenInventoryNormal();
            EquipPage.IsShow = true;
            _equipRoot.SetDisplay(true);
        }

        public void CloseEquip()
        {
            _itemsPage.CloseAll();
            SystemClosePage();
        }

        public void SystemClosePage()
        {
            EquipPage.IsShow = false;
            _equipRoot.SetDisplay(EquipPage.IsShow);
        }

        /// <summary>
        /// Обрабатывает обратную связь при перетаскивании для слотов экипировки.
        /// </summary>
        /// <param name="draggedItem">Перетаскиваемый предмет.</param>
        /// <param name="mousePosition">Текущая позиция мыши в мировых координатах UI.</param>
        /// <returns>Результаты размещения, или null, если мышь не над слотом экипировки.</returns>
        public PlacementResults ProcessDragFeedback(ItemVisual draggedItem, Vector2 mousePosition)
        {
            if (_equipRoot == null || !_equipRoot.enabledSelf || _equipRoot.resolvedStyle.display == DisplayStyle.None || _equipRoot.resolvedStyle.visibility == Visibility.Hidden)
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
                PlacementResults results = hoveredSlot.ShowPlacementTarget(draggedItem);
                foreach (var equipSlot in _equipSlots)
                {
                    if (equipSlot != hoveredSlot)
                    {
                        equipSlot.FinalizeDrag();
                    }
                }
                return results;
            }
            else
            {
                ClearAllTelegraphs();
                return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
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