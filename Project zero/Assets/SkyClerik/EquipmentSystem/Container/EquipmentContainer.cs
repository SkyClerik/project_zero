using SkyClerik.EquipmentSystem;
using System.Linq;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik
{
    public class EquipmentContainer : MonoBehaviour
    {
        [Header("Хранилище данных")]
        [SerializeField]
        private EquipmentContainerDefinition _playerEquipmentContainerDefinition;
        public EquipmentContainerDefinition PlayerEquipmentContainerDefinition { get => _playerEquipmentContainerDefinition; set => _playerEquipmentContainerDefinition = value; }

        [Header("Конфигурация")]
        [Tooltip("Ссылка на UI Document, в котором находятся слоты для экипировки.")]
        [SerializeField] private UIDocument _uiDocument;
        [Tooltip("Имя корневой панели в UI документе, внутри которой находится элемент 'grid'.")]
        [SerializeField] private string _rootPanelName;
        public string RootPanelName => _rootPanelName;

        [Tooltip("Рассчитанные мировые координаты сетки. Не редактировать вручную.")]
        [SerializeField]
        [ReadOnly]
        private Rect _gridWorldRect; // (1387,226,896,1024)

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

                if (_gridWorldRect != inventoryGrid.worldBound)
                {
                    _gridWorldRect = inventoryGrid.worldBound;
                }

                if (inventoryGrid.childCount == 0)
                {
                    Debug.LogWarning($"Сетка '{inventoryGrid.name}' не содержит дочерних элементов (ячеек). Невозможно определить размер ячейки.", this);
                    return;
                }

                var allCell = inventoryGrid.Children().ToList();

                _playerEquipmentContainerDefinition.EquipmentSlots.Clear();
                foreach (var visualElement in allCell)
                {
                    var calculatedCellSize = new Vector2(visualElement.resolvedStyle.width, visualElement.resolvedStyle.height);

                    if (calculatedCellSize.x > 0 && calculatedCellSize.y > 0)
                    {
                        var rect = new Rect(visualElement.resolvedStyle.left, visualElement.resolvedStyle.top, visualElement.resolvedStyle.width, visualElement.resolvedStyle.height);
                        _playerEquipmentContainerDefinition.EquipmentSlots.Add(new EquipmentSlot(rect: rect));
                    }
                }

            }).ExecuteLater(1);
        }
#endif

        private void Awake()
        {
            _playerEquipmentContainerDefinition.ValidateGuid();
        }
    }
}
