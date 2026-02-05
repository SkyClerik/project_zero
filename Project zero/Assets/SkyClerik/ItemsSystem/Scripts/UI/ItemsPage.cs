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
    /// <summary>
    /// Представляет собой связку между экземпляром контейнера предметов и его UI-страницей.
    /// Используется для унификации управления страницами инвентаря.
    /// </summary>
    public class ContainerAndPage
    {
        [SerializeField]
        private ItemContainer _container;
        [SerializeField]
        private GridPageElementBase _page;

        /// <summary>
        /// Возвращает связанный контейнер предметов.
        /// </summary>
        public ItemContainer Container => _container;
        /// <summary>
        /// Возвращает связанную UI-страницу сетки.
        /// </summary>
        public GridPageElementBase Page => _page;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ContainerAndPage"/>.
        /// </summary>
        /// <param name="itemContainer">Контейнер предметов.</param>
        /// <param name="gridPageElementBase">UI-страница сетки.</param>
        public ContainerAndPage(ItemContainer itemContainer, GridPageElementBase gridPageElementBase)
        {
            _container = itemContainer;
            _page = gridPageElementBase;
        }
    }

    /// <summary>
    /// Главный контроллер всех страниц UI инвентаря (инвентарь, крафт, сундук, лут).
    /// Отвечает за координацию отображения, перетаскивания предметов и взаимодействие с глобальными игровыми состояниями.
    /// </summary>
    public class ItemsPage : MonoBehaviour
    {
        private UIDocument _document;
        private Vector2 _mousePositionOffset;
        private GlobalGameProperty _globalGameProperty;
        private ItemTooltip _itemTooltip;
        private Coroutine _tooltipShowCoroutine;
        private const float _tooltipDelay = 0.5f;

        private Vector2 _mouseUILocalPosition;
        /// <summary>
        /// Текущая локальная позиция мыши в пространстве UI.
        /// </summary>
        public Vector2 MouseUILocalPosition => _mouseUILocalPosition;

        private static ItemVisual _currentDraggedItem = null;
        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

        private ItemBaseDefinition _givenItem = null;
        /// <summary>
        /// Предмет, который был выбран для отдачи (например, при передаче NPC).
        /// </summary>
        public ItemBaseDefinition GiveItem => _givenItem;

        [SerializeField]
        private ItemContainer _inventoryItemContainer;
        private InventoryPageElement _inventoryPage;
        /// <summary>
        /// Определяет, виден ли UI инвентаря.
        /// </summary>
        public bool IsInventoryVisible { get => _inventoryPage.Root.enabledSelf; set => _inventoryPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _craftItemContainer;
        private CraftPageElement _craftPage;
        /// <summary>
        /// Определяет, виден ли UI страницы крафта.
        /// </summary>
        public bool IsCraftVisible { get => _craftPage.Root.enabledSelf; set => _craftPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _cheastItemContainer;
        private CheastPageElement _cheastPage;
        /// <summary>
        /// Определяет, виден ли UI страницы сундука.
        /// </summary>
        public bool IsCheastVisible { get => _cheastPage.Root.enabledSelf; set => _cheastPage.Root.SetEnabled(value); }

        [SerializeField]
        private ItemContainer _lutItemContainer;
        private LutPageElement _lutPage;
        /// <summary>
        /// Определяет, виден ли UI страницы лута.
        /// </summary>
        public bool IsLutVisible { get => _lutPage.Root.enabledSelf; set => _lutPage.Root.SetEnabled(value); }


        [SerializeField]
        private EquipmentContainer _quipmentContainer;
        private EquipmentPageElement _equipPage;
        public bool IsEquipVisible { get => _equipPage.Root.enabledSelf; set => _equipPage.Root.SetEnabled(value); }

        private List<ContainerAndPage> _containersAndPages = new List<ContainerAndPage>();
        /// <summary>
        /// Список всех зарегистрированных связок контейнеров и их UI-страниц.
        /// </summary>
        public List<ContainerAndPage> ContainersAndPages => _containersAndPages;

        /// <summary>
        /// Делегат для события "предмет отдан".
        /// </summary>
        /// <param name="item">Предмет, который был отдан.</param>
        public delegate void OnItemGivenDelegate(ItemBaseDefinition item);
        /// <summary>
        /// Событие, вызываемое при отдаче предмета (например, при взаимодействии с NPC).
        /// </summary>
        public event OnItemGivenDelegate OnItemGiven;
        /// <summary>
        /// Вызывает событие <see cref="OnItemGiven"/>.
        /// </summary>
        /// <param name="item">Предмет, который был отдан.</param>
        public void RiseItemGiveEvent(ItemBaseDefinition item)
        {
            Debug.Log($"TriggerItemGiveEvent: {item}");
            OnItemGiven?.Invoke(item);
        }

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
            _inventoryPage?.Dispose();
            _craftPage?.Dispose();
            _cheastPage?.Dispose();
            _lutPage?.Dispose();
            _equipPage?.Dispose();
        }

        protected void Start()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: false);
            _document.enabled = true;

            _inventoryPage = new InventoryPageElement(itemsPage: this, document: _document, itemContainer: _inventoryItemContainer);
            var inventoryCA = new ContainerAndPage(_inventoryItemContainer, _inventoryPage);
            _containersAndPages.Add(inventoryCA);

            _craftPage = new CraftPageElement(itemsPage: this, document: _document, itemContainer: _craftItemContainer);
            var craftCA = new ContainerAndPage(_craftItemContainer, _craftPage);
            _containersAndPages.Add(craftCA);

            _cheastPage = new CheastPageElement(itemsPage: this, document: _document, itemContainer: _cheastItemContainer);
            var cheastCA = new ContainerAndPage(_cheastItemContainer, _cheastPage);
            _containersAndPages.Add(cheastCA);

            _lutPage = new LutPageElement(itemsPage: this, document: _document, itemContainer: _lutItemContainer, _inventoryPage);
            var lutCA = new ContainerAndPage(_lutItemContainer, _lutPage);
            _containersAndPages.Add(lutCA);

            if (_quipmentContainer != null)
                _equipPage = new EquipmentPageElement(itemsPage: this, document: _document, equipmentContainer: _quipmentContainer);

            _itemTooltip = new ItemTooltip();
            _document.rootVisualElement.Add(_itemTooltip);

            _globalGameProperty = ServiceProvider.Get<GlobalManager>()?.GlobalGameProperty;

            CloseAll();
        }

        private void OnRootMouseMove(MouseMoveEvent evt)
        {
            _mouseUILocalPosition = evt.localMousePosition;
        }

        private void Update()
        {
            if (!_document.isActiveAndEnabled)
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
            //Debug.Log($"[ЛОG] Проверяю страницу инвентаря ({_inventoryPage.Root.name}).");
            PlacementResults resultsPage = _inventoryPage.ShowPlacementTarget(draggedItem);
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
                    if (resultsTwo.Conflict != ReasonConflict.beyondTheGridBoundary)
                    {
                        //Debug.Log($"[ЛОГ] Страница крафта активна. Конфликт: {resultsTwo.Conflict}. Скрываю телеграф инвентаря.");
                        _inventoryPage.Telegraph.Hide();
                        return resultsTwo.Init(resultsTwo.Conflict, resultsTwo.Position, resultsTwo.SuggestedGridPosition, resultsTwo.OverlapItem, _craftPage);
                    }
                }
            }

            // -----
            if (_equipPage != null && _equipPage.Root.enabledSelf)
            {
                PlacementResults resultsEquip = _equipPage.ShowPlacementTarget(draggedItem);
                if (resultsEquip.Conflict != ReasonConflict.beyondTheGridBoundary)
                {
                    _inventoryPage.Telegraph.Hide();
                    _craftPage.Telegraph.Hide();
                    _cheastPage.Telegraph.Hide();
                    _lutPage.Telegraph.Hide();
                    return resultsEquip.Init(resultsEquip.Conflict, resultsEquip.Position, resultsEquip.SuggestedGridPosition, resultsEquip.OverlapItem, _equipPage);
                }
            }

            //Debug.Log("[ЛОГ] Ни одна страница не подходит. Скрываю оба телеграфа.");
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
            _inventoryPage.FinalizeDrag();
            _craftPage.FinalizeDrag();
            _cheastPage.FinalizeDrag();
            _lutPage.FinalizeDrag();
            _equipPage.FinalizeDrag();
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
            var itemToMove = draggedItem.ItemDefinition;
            var sourceContainer = (sourceInventory as GridPageElementBase)?.ItemContainer;
            var targetContainer = (targetInventory as GridPageElementBase)?.ItemContainer;

            if (sourceContainer == null || targetContainer == null)
            {
                //Debug.LogError("Не удалось найти контейнеры для перемещения предмета!");
                return;
            }

            // Удаляем предмет из исходного контейнера (это вызовет OnItemRemoved в UI)
            sourceContainer.RemoveItem(itemToMove, destroy: false);

            // Пытаемся добавить предмет в целевой контейнер на указанную позицию Это вызовет OnItemAdded в UI, если успешно
            bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToMove, gridPosition);

            if (!addedToTarget)
            {
                //Debug.LogWarning($"Не удалось переместить предмет '{itemToMove.name}' в целевой контейнер на позицию {gridPosition}. Возвращаем в исходный контейнер.");
                // Если не удалось добавить в целевой, возвращаем предмет в исходный контейнер
                // Это может вызвать OnItemAdded в UI исходного контейнера, ItemContainer сам найдет место
                sourceContainer.AddItems(new List<ItemBaseDefinition> { itemToMove });
            }
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
        public void OpenInventoryFromGiveItem(int itemID)
        {
            _givenItem = _inventoryItemContainer.GetItemByItemID(itemID);
            if (_givenItem != null)
            {
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                OpenInventoryNormal();
            }
        }

        /// <summary>
        /// Откроет инвентарь для выбора указанного предмета.
        /// Если ссылка на предмет null, инвентарь не откроется.
        /// </summary>
        /// <param name="item">Предмет, который нужно выбрать.</param>
        public void OpenInventoryGiveItem(ItemBaseDefinition item)
        {
            _givenItem = item;
            if (_givenItem != null)
            {
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                OpenInventoryNormal();
            }
        }

        /// <summary>
        /// Открывает обычный режим отображения инвентаря.
        /// </summary>
        public void OpenInventoryAndCraft()
        {
            OpenInventoryNormal();
            OpenCraft();
        }

        public void OpenInventoryNormal()
        {
            _givenItem = null;
            SetPage(_inventoryPage.Root, display: true, visible: true, enabled: true);
            _document.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
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
        public void OpenCheast()
        {
            OpenInventoryNormal();
            SetPage(_cheastPage.Root, display: true, visible: true, enabled: true);
        }
        /// <summary>
        /// Открывает страницу лута.
        /// </summary>
        public void OpenLut()
        {
            OpenInventoryNormal();
            SetPage(_lutPage.Root, display: true, visible: true, enabled: true);
        }
        /// <summary>
        /// Открывает страницу экипировки.
        /// </summary>
        public void OpenEquip()
        {
            OpenInventoryNormal();
            SetPage(_equipPage.Root, display: true, visible: true, enabled: true);
        }
        /// <summary>
        /// Закрывает все страницы.
        /// </summary>
        public void CloseAll()
        {
            SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
            SetAllSelfPage(display: false, visible: false, enabled: false);
            _document.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);
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

            _equipPage.Root.SetDisplay(display);
            _equipPage.Root.SetVisibility(visible);
            _equipPage.Root.SetEnabled(enabled);
        }

        /// <summary>
        /// Закрывает инвентарь.
        /// </summary>
        //public void CloseInventory()
        //{
        //    SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
        //    SetAllSelfPage(display: false, visible: false, enabled: false);
        //    _document.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);
        //}

        /// <summary>
        /// Закрывает страницу крафта.
        /// </summary>
        //public void CloseCraft()
        //{
        //    SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
        //    SetAllSelfPage(display: false, visible: false, enabled: false);
        //}

        /// <summary>
        /// Закрывает страницу сундука.
        /// </summary>
        //public void CloseCheast()
        //{
        //    SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
        //    SetAllSelfPage(display: false, visible: false, enabled: false);
        //}

        /// <summary>
        /// Закрывает страницу лута.
        /// </summary>
        //public void CloseLut()
        //{
        //    SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
        //    SetAllSelfPage(display: false, visible: false, enabled: false);
        //}

        /// <summary>
        /// Закрывает страницу экипировки.
        /// </summary>
        //public void CloseEquip()
        //{
        //    SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
        //    SetAllSelfPage(display: false, visible: false, enabled: false);
        //}
    }
}