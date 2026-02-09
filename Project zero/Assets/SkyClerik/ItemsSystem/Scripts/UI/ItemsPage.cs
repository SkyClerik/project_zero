using SkyClerik.EquipmentSystem;
using SkyClerik.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    public class ContainerAndPage
    {
        [SerializeField]
        private ItemContainer _container;
        [SerializeField]
        private GridPageElementBase _page;
        public ItemContainer Container => _container;
        public GridPageElementBase Page => _page;
        public ContainerAndPage(ItemContainer itemContainer, GridPageElementBase gridPageElementBase)
        {
            _container = itemContainer;
            _page = gridPageElementBase;
        }
    }

    public class GivenItem
    {
        private ItemBaseDefinition _desiredProduct = null;
        private ItemVisual _visual;
        private bool _tracing;
        private Color _tracingColor = Color.white;
        private int _tracingWidth = 2;
        private int _tracingZero = 0;
        private int _maxAttempt = 4;

        // Предмет, который был выбран для отдачи (например, при передаче NPC).
        public ItemBaseDefinition DesiredProduct { get => _desiredProduct; set => _desiredProduct = value; }
        // Сразу получаю и храню ссылку на визуальный элемент
        public ItemVisual Visual { get => _visual; set => _visual = value; }
        // Если true то предмет в инвентаре подсвечивается постоянно
        public bool Tracing { get => _tracing; set => _tracing = value; }
        public Color TracingColor { get => _tracingColor; set => _tracingColor = value; }
        public int TracingWidth { get => _tracingWidth; set => _tracingWidth = value; }
        public int TracingZero => _tracingZero;
        public int MaxAttempt { get => _maxAttempt; set => _maxAttempt = value; }
    }

    /// <summary>
    /// Главный контроллер всех страниц UI инвентаря (инвентарь, крафт, сундук, лут).
    /// Отвечает за координацию отображения, перетаскивания предметов и взаимодействие с глобальными игровыми состояниями.
    /// </summary>
    public class ItemsPage : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private UIDocument _uiDocument;
        private Vector2 _mousePositionOffset;
        private GlobalGameProperty _globalGameProperty;
        private ItemTooltip _itemTooltip;
        private Coroutine _tooltipShowCoroutine;
        private const float _tooltipDelay = 0.5f;
        private GivenItem _givenItem = new GivenItem();

        public GivenItem GivenItem => _givenItem;

        private Vector2 _mouseUILocalPosition;
        /// <summary>
        /// Текущая локальная позиция мыши в пространстве UI.
        /// </summary>
        internal Vector2 MouseUILocalPosition => _mouseUILocalPosition;


        private static ItemVisual _currentDraggedItem;
        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

        [SerializeField]
        private ItemContainer _inventoryItemContainer;
        private InventoryPageElement _inventoryPage;
        /// <summary>
        /// Определяет, виден ли UI инвентаря.
        /// </summary>
        internal bool IsInventoryVisible { get => _inventoryPage.Root.enabledSelf; set => _inventoryPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _craftItemContainer;
        private CraftPageElement _craftPage;
        /// <summary>
        /// Определяет, виден ли UI страницы крафта.
        /// </summary>
        internal bool IsCraftVisible { get => _craftPage.Root.enabledSelf; set => _craftPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _cheastItemContainer;
        private CheastPageElement _cheastPage;
        /// <summary>
        /// Определяет, виден ли UI страницы сундука.
        /// </summary>
        internal bool IsCheastVisible { get => _cheastPage.Root.enabledSelf; set => _cheastPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _lutItemContainer;
        private LutPageElement _lutPage;
        /// <summary>
        /// Определяет, виден ли UI страницы лута.
        /// </summary>
        internal bool IsLutVisible { get => _lutPage.Root.enabledSelf; set => _lutPage.Root.SetEnabled(value); }

        private List<ContainerAndPage> _containersAndPages = new List<ContainerAndPage>();
        /// <summary>
        /// Список всех зарегистрированных связок контейнеров и их UI-страниц.
        /// </summary>
        internal List<ContainerAndPage> ContainersAndPages => _containersAndPages;

        private void OnValidate()
        {
            _uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
        }

        private void Awake()
        {
            ServiceProvider.Register(this);
            _uiDocument.enabled = true;
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
            _inventoryPage?.Dispose();
            _craftPage?.Dispose();
            _cheastPage?.Dispose();
            _lutPage?.Dispose();
        }

        protected void Start()
        {
            _inventoryPage = new InventoryPageElement(itemsPage: this, document: _uiDocument, itemContainer: _inventoryItemContainer);
            var inventoryCA = new ContainerAndPage(_inventoryItemContainer, _inventoryPage);
            _containersAndPages.Add(inventoryCA);

            _craftPage = new CraftPageElement(itemsPage: this, document: _uiDocument, itemContainer: _craftItemContainer);
            var craftCA = new ContainerAndPage(_craftItemContainer, _craftPage);
            _containersAndPages.Add(craftCA);

            _cheastPage = new CheastPageElement(itemsPage: this, document: _uiDocument, itemContainer: _cheastItemContainer);
            var cheastCA = new ContainerAndPage(_cheastItemContainer, _cheastPage);
            _containersAndPages.Add(cheastCA);

            _lutPage = new LutPageElement(itemsPage: this, document: _uiDocument, itemContainer: _lutItemContainer, _inventoryPage);
            var lutCA = new ContainerAndPage(_lutItemContainer, _lutPage);
            _containersAndPages.Add(lutCA);

            _itemTooltip = new ItemTooltip();
            _uiDocument.rootVisualElement.Add(_itemTooltip);

            _globalGameProperty = ServiceProvider.Get<GlobalBox>()?.GlobalGameProperty;

            CloseAll();
        }

        private void OnRootMouseMove(MouseMoveEvent evt)
        {
            _mouseUILocalPosition = evt.localMousePosition;
        }

        private void Update()
        {
            if (!_uiDocument.isActiveAndEnabled)
                return;

            if (_currentDraggedItem == null)
                return;

            if (Input.GetMouseButtonDown(1))
                _currentDraggedItem.Rotate();

            SetDraggedItemPosition();
        }

        /// <summary>
        /// Устанавливает позицию перетаскиваемого предмета на UI в соответствии с положением мыши.
        /// </summary>
        public void SetDraggedItemPosition()
        {
            _mousePositionOffset.x = _mouseUILocalPosition.x - (_currentDraggedItem.resolvedStyle.width / 2);
            _mousePositionOffset.y = _mouseUILocalPosition.y - (_currentDraggedItem.resolvedStyle.height / 2);
            _currentDraggedItem.SetPosition(_mousePositionOffset);
        }

        /// <summary>
        /// Обрабатывает логику размещения перетаскиваемого предмета над различными контейнерами.
        /// Определяет возможные конфликты и предлагает позицию для размещения.
        /// </summary>
        /// <param name="draggedItem">Визуальный элемент перетаскиваемого предмета.</param>
        /// <returns>Результаты размещения, включающие информацию о конфликте и предложенной позиции.</returns>
        public PlacementResults HandleItemPlacement(ItemVisual draggedItem)
        {
            //Debug.Log($"[ItemsPage][Placement] Начало HandleItemPlacement для {draggedItem.ItemDefinition.name}");
            //Debug.Log($"[ЛОG] Проверяю страницу инвентаря ({_inventoryPage.Root.name}).");
            PlacementResults resultsPage = _inventoryPage.ShowPlacementTarget(draggedItem);
            //Debug.Log($"[ItemsPage][Placement] Результат для инвентаря: {resultsPage.Conflict}");
            if (resultsPage.Conflict == ReasonConflict.None || resultsPage.Conflict == ReasonConflict.StackAvailable || resultsPage.Conflict == ReasonConflict.SwapAvailable)
            {
                //Debug.Log($"[ЛОГ] Страница инвентаря активна. Конфликт: {resultsPage.Conflict}. Скрываю телеграф крафта.");
                _craftPage.Telegraph.Hide();
                return resultsPage.Init(resultsPage.Conflict, resultsPage.Position, resultsPage.SuggestedGridPosition, resultsPage.OverlapItem, _inventoryPage);
            }

            // -----
            if (_cheastPage.Root.enabledSelf)
            {
                //Debug.Log($"[ЛОG] Проверяю страницу сундука ({_cheastPage.Root.name}).");
                PlacementResults resultsCheast = _cheastPage.ShowPlacementTarget(draggedItem);
                //Debug.Log($"[ItemsPage][Placement] Результат для сундука: {resultsCheast.Conflict}");
                if (resultsCheast.Conflict != ReasonConflict.beyondTheGridBoundary)
                {
                    //Debug.Log($"[ЛОГ] Страница сундука активна. Конфликт: {resultsCheast.Conflict}. Скрываю телеграф инвентаря.");
                    _inventoryPage.Telegraph.Hide();
                    return resultsCheast.Init(resultsCheast.Conflict, resultsCheast.Position, resultsCheast.SuggestedGridPosition, resultsCheast.OverlapItem, _cheastPage);
                }
            }
            // -----
            if (_lutPage.Root.enabledSelf)
            {
                //Debug.Log($"[ЛОG] Проверяю страницу лута ({_lutPage.Root.name}).");
                PlacementResults lutCheast = _lutPage.ShowPlacementTarget(draggedItem);
                //Debug.Log($"[ItemsPage][Placement] Результат для лута: {lutCheast.Conflict}");
                if (lutCheast.Conflict != ReasonConflict.beyondTheGridBoundary)
                {
                    //Debug.Log($"[ЛОГ] Страница лута активна. Конфликт: {lutCheast.Conflict}. Скрываю телеграф инвентаря.");
                    _inventoryPage.Telegraph.Hide();
                    return lutCheast.Init(lutCheast.Conflict, lutCheast.Position, lutCheast.SuggestedGridPosition, lutCheast.OverlapItem, _lutPage);
                }
            }

            // -----
            if (_craftPage.Root.enabledSelf)
            {
                if (_globalGameProperty != null && _globalGameProperty.MakeCraftAccessible)
                {
                    PlacementResults resultsTwo = _craftPage.ShowPlacementTarget(draggedItem);
                    //Debug.Log($"[ItemsPage][Placement] Результат для крафта: {resultsTwo.Conflict}");
                    if (resultsTwo.Conflict != ReasonConflict.beyondTheGridBoundary)
                    {
                        //Debug.Log($"[ЛОГ] Страница крафта активна. Конфликт: {resultsTwo.Conflict}. Скрываю телеграф инвентаря.");
                        _inventoryPage.Telegraph.Hide();
                        return resultsTwo.Init(resultsTwo.Conflict, resultsTwo.Position, resultsTwo.SuggestedGridPosition, resultsTwo.OverlapItem, _craftPage);
                    }
                }
            }

            //Debug.Log($"[ItemsPage][Placement] Возвращаем beyondTheGridBoundary для {draggedItem.ItemDefinition.name}");
            _inventoryPage.Telegraph.Hide();
            _craftPage.Telegraph.Hide();
            _cheastPage.Telegraph.Hide();
            _lutPage.Telegraph.Hide();

            return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
        }

        /// <summary>
        /// Завершает операцию перетаскивания для всех страниц.
        /// </summary>
        /// <param name="draggedItem">Визуальный элемент перетаскиваемого предмета.</param>
        public void FinalizeDragOfItem(ItemVisual draggedItem)
        {
            //Debug.Log($"[ItemsPage][FinalizeDragOfItem] Вызван для '{draggedItem.name}'. Parent: {(draggedItem.parent != null ? draggedItem.parent.name : "NULL")}. CurrentDraggedItem до сброса: {(CurrentDraggedItem != null ? CurrentDraggedItem.name : "NULL")}.");

            _inventoryPage.FinalizeDrag();
            _craftPage.FinalizeDrag();
            _cheastPage.FinalizeDrag();
            _lutPage.FinalizeDrag();

            if (CurrentDraggedItem != null)
            {
                if (CurrentDraggedItem.parent == _uiDocument.rootVisualElement)
                {
                    //Debug.Log($"[ItemsPage][FinalizeDragOfItem] Удаляем '{CurrentDraggedItem.name}' из rootVisualElement.");
                    CurrentDraggedItem.RemoveFromHierarchy();
                }
                CurrentDraggedItem = null;
                //Debug.Log($"[ItemsPage][FinalizeDragOfItem] CurrentDraggedItem сброшен до NULL.");
            }
            else
            {
                //Debug.Log($"[ItemsPage][FinalizeDragOfItem] CurrentDraggedItem уже NULL.");
            }
        }

        /// <summary>
        /// Перемещает предмет между контейнерами.
        /// </summary>
        /// <param name="draggedItem">Визуальный элемент перетаскиваемого предмета.</param>
        /// <param name="sourceInventory">Исходный контейнер (инвентарь, откуда был взят предмет).</param>
        /// <param name="targetInventory">Целевой контейнер (инвентарь, куда помещается предмет).</param>
        /// <param name="gridPosition">Позиция в сетке целевого контейнера.</param>
        public void TransferItemBetweenContainers(ItemVisual draggedItem, IDropTarget sourceInventory, IDropTarget targetInventory, Vector2Int gridPosition)
        {
            //Debug.Log($"[ItemsPage][Transfer] Dragged: {draggedItem.ItemDefinition.name}");
            //Debug.Log($"[ItemsPage][Transfer] Source Inventory Type: {sourceInventory?.GetType().Name}");
            //Debug.Log($"[ItemsPage][Transfer] Target Inventory Type: {targetInventory?.GetType().Name}");

            var itemToMove = draggedItem.ItemDefinition;

            GridPageElementBase sourceGridPage = sourceInventory as GridPageElementBase;
            GridPageElementBase targetGridPage = targetInventory as GridPageElementBase;
            EquipmentSlot sourceEquipSlot = sourceInventory as EquipmentSlot;
            EquipmentSlot targetEquipSlot = targetInventory as EquipmentSlot;

            //Debug.Log($"[ItemsPage][Transfer] SourceGridPage: {sourceGridPage != null}, TargetGridPage: {targetGridPage != null}, SourceEquipSlot: {sourceEquipSlot != null}, TargetEquipSlot: {targetEquipSlot != null}");

            if (sourceGridPage != null && targetGridPage != null)
            {
                //Debug.Log($"Случай 1: Из инвентаря в инвентарь");

                var sourceContainer = sourceGridPage.ItemContainer;
                var targetContainer = targetGridPage.ItemContainer;

                if (sourceContainer == null || targetContainer == null)
                {
                    //Debug.LogError("Не удалось найти контейнеры для перемещения предмета!");
                    return;
                }

                //Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToMove.name}' был забран из контейнера '{sourceContainer.name}'.");
                sourceContainer.RemoveItem(itemToMove, destroy: false);

                bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToMove, gridPosition);

                if (addedToTarget)
                {
                    //Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToMove.name}' был положен в контейнер '{targetContainer.name}' в позицию: {gridPosition}.");
                }
                else
                {
                    sourceContainer.AddItems(new List<ItemBaseDefinition> { itemToMove });
                }
            }
        }

        public void SetItemDescription(ItemBaseDefinition itemBaseDefinition)
        {
            _inventoryPage.SetItemDescription(itemBaseDefinition);           
        }

        /// <summary>
        /// Запускает задержку перед показом всплывающей подсказки для предмета.
        /// </summary>
        /// <param name="itemVisual">Визуальный элемент предмета, для которого показывается подсказка.</param>
        public void StartTooltipDelay(ItemVisual itemVisual)
        {
            if (CurrentDraggedItem != null)
                return;

            StopTooltipDelayAndHideTooltip();
            _tooltipShowCoroutine = StartCoroutine(ShowTooltipCoroutine(itemVisual));
        }

        /// <summary>
        /// Останавливает задержку показа всплывающей подсказки и скрывает её.
        /// </summary>
        public void StopTooltipDelayAndHideTooltip()
        {
            if (_tooltipShowCoroutine != null)
            {
                StopCoroutine(_tooltipShowCoroutine);
                _tooltipShowCoroutine = null;
            }
            _itemTooltip.HideTooltip();
        }

        private IEnumerator ShowTooltipCoroutine(ItemVisual itemVisual)
        {
            yield return new WaitForSeconds(_tooltipDelay);
            _itemTooltip.ShowTooltip(itemVisual.ItemDefinition, itemVisual.worldBound.center);
        }

        /// <summary>
        /// Откроет инвентарь для выбора предмета, который найдет по индексу.
        /// Если предмет не будет найден, инвентарь не откроется.
        /// </summary>
        /// <param name="itemID">WrapperIndex искомого предмета.</param>
        internal void OpenInventoryFromGiveItem(int itemID, bool tracing)
        {
            _givenItem.DesiredProduct = _inventoryItemContainer.GetItemByItemID(itemID);
            GiveItemSettings(_givenItem.DesiredProduct, tracing);
            if (_givenItem.DesiredProduct != null)
            {
                //Debug.Log($"Открываю для выбора {_givenItem.DefinitionName}");
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                OpenInventoryNormal();
            }
        }

        /// <summary>
        /// Откроет инвентарь для выбора указанного предмета.
        /// Если ссылка на предмет null, инвентарь не откроется.
        /// </summary>
        /// <param name="item">Предмет, который нужно выбрать.</param>
        internal void OpenInventoryGiveItem(ItemBaseDefinition item, bool tracing)
        {
            GiveItemSettings(item, tracing);
            if (_givenItem.DesiredProduct != null)
            {
                //Debug.Log($"Открываю для выбора {_givenItem.DefinitionName}");
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                OpenInventoryNormal();
            }
        }

        private void GiveItemSettings(ItemBaseDefinition item, bool tracing)
        {
            _givenItem.DesiredProduct = item;
            _givenItem.Visual = _inventoryPage.GetItemVisual(_givenItem.DesiredProduct);
            _givenItem.Tracing = tracing;

            if (_givenItem.Visual != null)
                ApplyVisualItemHighlight(_givenItem.Visual, isZeroWidth: false);
        }

        private void ApplyVisualItemHighlight(ItemVisual visualToHighlight, bool isZeroWidth, int attempt = 0)
        {
            // Просто вызываем без attempt, он будет 0 по умолчанию
            if (visualToHighlight == null)
                return;

            int curWidth = isZeroWidth == true ? _givenItem.TracingZero : _givenItem.TracingWidth;
            int maxAttempts = _givenItem.MaxAttempt;
            visualToHighlight.schedule.Execute(() =>
            {
                //Debug.LogWarning($"visualToHighlight.resolvedStyle.width {visualToHighlight.worldBound}.");
                if (visualToHighlight.worldBound.width > maxAttempts)
                {
                    visualToHighlight.SetBorderColor(_givenItem.TracingColor);
                    visualToHighlight.SetBorderWidth(curWidth);
                }
                else if (attempt < maxAttempts)
                {
                    //Debug.LogWarning($"[ApplyVisualItemHighlight] Размер для {visualToHighlight.name} ({visualToHighlight.resolvedStyle.width}x{visualToHighlight.resolvedStyle.height}) не соответствует ожидаемому {expectedSize}. Повторная попытка {attempt + 1}.");
                    ApplyVisualItemHighlight(visualToHighlight, isZeroWidth, attempt + 1);
                }
                else
                {
                    //Debug.LogWarning($"Применяем сброс как стандартное решение {visualToHighlight.name}.");
                    visualToHighlight.SetBorderColor(ColorExt.GetColorTransparent());
                    visualToHighlight.SetBorderWidth(_givenItem.TracingZero);
                }
            }).ExecuteLater(250);
        }

        /// <summary>
        /// Открывает обычный режим отображения инвентаря.
        /// </summary>
        internal void OpenInventoryAndCraft()
        {
            _givenItem.DesiredProduct = null;
            OpenInventoryNormal();
            OpenCraft();
        }

        public void OpenInventoryNormal()
        {
            SetPage(_inventoryPage.Root, display: true, visible: true, enabled: true);
            _uiDocument.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
            _inventoryPage.DisableItemDescription();
        }

        /// <summary>
        /// Открывает страницу крафта. Доступность зависит от глобального свойства <see cref="GlobalGameProperty.MakeCraftAccessible"/>.
        /// </summary>
        private void OpenCraft()
        {
            SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
            if (_globalGameProperty != null && _globalGameProperty.MakeCraftAccessible)
                SetPage(_craftPage.Root, display: true, visible: true, enabled: true);
        }

        /// <summary>
        /// Открывает страницу сундука.
        /// </summary>
        internal void OpenCheast()
        {
            SetPage(_craftPage.Root, display: false, visible: false, enabled: false);
            OpenInventoryNormal();
            SetPage(_cheastPage.Root, display: true, visible: true, enabled: true);
        }

        /// <summary>
        /// Открывает страницу лута.
        /// </summary>
        internal void OpenLut()
        {
            SetPage(_craftPage.Root, display: false, visible: false, enabled: false);
            OpenInventoryNormal();
            SetPage(_lutPage.Root, display: true, visible: true, enabled: true);
        }

        /// <summary>
        /// Закрывает все страницы.
        /// </summary>
        public void CloseAll()
        {
            SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
            SetAllSelfPage(display: false, visible: false, enabled: false);
            _uiDocument.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);

            var equipPage = ServiceProvider.Get<EquipPage>();
            equipPage?.SystemClosePage();

            if (_givenItem.Visual != null)
                ApplyVisualItemHighlight(_givenItem.Visual, isZeroWidth: true);
        }

        private void SetPage(VisualElement pageRoot, bool display, bool visible, bool enabled)
        {
            pageRoot.SetVisibility(visible);
            pageRoot.SetDisplay(display);
            pageRoot.SetEnabled(enabled);
        }

        private void SetAllSelfPage(bool display, bool visible, bool enabled)
        {
            _craftPage.Root.SetDisplay(display);
            _craftPage.Root.SetVisibility(visible);
            _craftPage.Root.SetEnabled(enabled);

            _cheastPage.Root.SetDisplay(display);
            _cheastPage.Root.SetVisibility(visible);
            _cheastPage.Root.SetEnabled(enabled);

            _lutPage.Root.SetDisplay(display);
            _lutPage.Root.SetVisibility(visible);
            _lutPage.Root.SetEnabled(enabled);
        }
    }
}