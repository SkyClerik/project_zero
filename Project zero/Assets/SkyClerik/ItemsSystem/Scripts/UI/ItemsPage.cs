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
        [SerializeField]
        [ReadOnly]
        private UIDocument _uiDocument;
        private Vector2 _mousePositionOffset;
        private GlobalGameProperty _globalGameProperty;
        private ItemTooltip _itemTooltip;
        private Coroutine _tooltipShowCoroutine;
        private const float _tooltipDelay = 0.5f;

        private Vector2 _mouseUILocalPosition;
        /// <summary>
        /// Текущая локальная позиция мыши в пространстве UI.
        /// </summary>
        internal Vector2 MouseUILocalPosition => _mouseUILocalPosition;

        private static ItemVisual _currentDraggedItem;
        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

        private ItemBaseDefinition _givenItem = null;
        /// <summary>
        /// Предмет, который был выбран для отдачи (например, при передаче NPC).
        /// </summary>
        internal ItemBaseDefinition GiveItem => _givenItem;

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
            // Добавляем проверку для EquipPage (который теперь содержит IDropTarget слоты)
            EquipPage equipPage = ServiceProvider.Get<EquipPage>();
            if (equipPage != null && equipPage.isActiveAndEnabled && equipPage._root.resolvedStyle.display != DisplayStyle.None)
            {
                // Сначала скрываем телеграфы всех слотов экипировки, чтобы они не оставались висящими
                foreach (var equipSlot in equipPage.EquipmentSlots)
                {
                    equipSlot.FinalizeDrag();
                }

                // Для каждого EquipmentSlot в EquipPage
                foreach (var equipSlot in equipPage.EquipmentSlots)
                {
                    // Проверяем, находится ли мышка над текущим EquipmentSlot
                    if (equipSlot.Rect.Contains(MouseUILocalPosition))
                    {
                        Debug.Log($"Проверка слота: {equipSlot.Cell.name}, Rect: {equipSlot.Rect}, Mouse Local in EquipGrid: {MouseUILocalPosition}"); // Новый Debug.Log

                        // Если мышка над слотом, то делегируем проверку этому EquipmentSlot
                        PlacementResults equipResults = equipSlot.ShowPlacementTarget(draggedItem);
                        if (equipResults.Conflict != ReasonConflict.beyondTheGridBoundary)
                        {
                            Debug.Log($"Мышка над слотом: {equipSlot.Cell.name}"); // Новый Debug.Log
                            // Скрываем все другие телеграфы
                            _inventoryPage.Telegraph.Hide();
                            _craftPage.Telegraph.Hide();
                            _cheastPage.Telegraph.Hide();
                            _lutPage.Telegraph.Hide();
                            return equipResults;
                        }
                    }
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

            // Добавляем вызов FinalizeDrag для всех слотов EquipPage
            EquipPage equipPage = ServiceProvider.Get<EquipPage>();
            if (equipPage != null)
            {
                foreach (var equipSlot in equipPage.EquipmentSlots)
                {
                    equipSlot.FinalizeDrag();
                }
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
            var itemToMove = draggedItem.ItemDefinition;

            GridPageElementBase sourceGridPage = sourceInventory as GridPageElementBase;
            GridPageElementBase targetGridPage = targetInventory as GridPageElementBase;
            EquipmentSlot sourceEquipSlot = sourceInventory as EquipmentSlot;
            EquipmentSlot targetEquipSlot = targetInventory as EquipmentSlot;

            // Случай 1: Из инвентаря в инвентарь
            if (sourceGridPage != null && targetGridPage != null)
            {
                var sourceContainer = sourceGridPage.ItemContainer;
                var targetContainer = targetGridPage.ItemContainer;

                if (sourceContainer == null || targetContainer == null)
                {
                    Debug.LogError("Не удалось найти контейнеры для перемещения предмета!");
                    return;
                }

                // Логика перемещения между ItemContainer (с debug-логами)
                Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToMove.name}' был забран из контейнера '{sourceContainer.name}'.");
                sourceContainer.RemoveItem(itemToMove, destroy: false);

                bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToMove, gridPosition);

                if (addedToTarget)
                {
                    Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToMove.name}' был положен в контейнер '{targetContainer.name}' в позицию: {gridPosition}.");
                }
                else
                {
                    sourceContainer.AddItems(new List<ItemBaseDefinition> { itemToMove });
                }
            }
            // Случай 2: Из инвентаря в слот экипировки
            else if (sourceGridPage != null && targetEquipSlot != null)
            {
                var sourceContainer = sourceGridPage.ItemContainer;
                TransferItemToEquipSlot(draggedItem, sourceContainer, targetEquipSlot);
            }
            // Случай 3: Из слота экипировки в инвентарь
            else if (sourceEquipSlot != null && targetGridPage != null)
            {
                var targetContainer = targetGridPage.ItemContainer;
                TransferItemFromEquipSlot(draggedItem, sourceEquipSlot, targetContainer, gridPosition);
            }
            // Случай 4: Из слота экипировки в слот экипировки
            else if (sourceEquipSlot != null && targetEquipSlot != null)
            {
                // Если оба - слоты экипировки, это может быть попытка свапа между слотами экипировки
                // или попытка экипировать тот же предмет обратно в другой слот экипировки.
                // В данном случае, так как предмет уже "поднят", мы просто пытаемся его экипировать в целевой слот.
                // Если целевой слот занят, он обработает свап сам (в TransferItemToEquipSlot)
                // Для этого нужно сначала "снять" его с текущего слота (sourceEquipSlot) и затем "надеть" на целевой (targetEquipSlot)
                // НО! Логика TransferItemToEquipSlot уже учитывает свап.
                // Поэтому, мы можем просто снять предмет с sourceEquipSlot и затем передать его в TransferItemToEquipSlot,
                // предполагая, что поднятый предмет - это ItemVisual.CurrentDraggedItem, а он уже имеет ItemDefinition.
                
                // Сначала снимаем предмет с текущего слота (sourceEquipSlot)
                sourceEquipSlot.Unequip();
                Debug.Log($"[ЭКИПИРОВКА] Предмет '{itemToMove.name}' был снят из исходного слота экипировки '{sourceEquipSlot.Cell.name}' для переброски в другой слот экипировки.");
                
                // Теперь пытаемся экипировать его в целевой слот.
                // Важно: draggedItem здесь - это предмет, который был изначально поднят,
                // и он остается ItemsPage.CurrentDraggedItem.
                // Мы передаем его как будто он из "инвентаря" (но без реального контейнера источника)
                // Нужно адаптировать TransferItemToEquipSlot или создать новый метод.
                // В текущей логике, TransferItemToEquipSlot предполагает sourceContainer.
                // Упрощенное решение: если это перетаскивание между двумя EquipmentSlot, то:
                // 1. Снять draggedItem с sourceEquipSlot.
                // 2. Если targetEquipSlot занят, то предмет из targetEquipSlot переносится в sourceEquipSlot.
                // 3. draggedItem экипируется в targetEquipSlot.

                // Временно просто выведем лог, чтобы не сломать текущий функционал, пока не продумаем свап между EquipSlots.
                Debug.LogWarning($"[ЭКИПИРОВКА] Перемещение между слотами экипировки ('{sourceEquipSlot.Cell.name}' -> '{targetEquipSlot.Cell.name}') пока не имеет полной логики свапа. Реализуйте этот сценарий.");

                // Здесь нужно более сложная логика, которая учитывает, что sourceContainer здесь отсутствует.
                // Пока оставим без изменений логики, чтобы не вводить баги.
                // Временное решение - просто вернуть предмет обратно, если он не может быть помещен в целевой слот.
                if (targetEquipSlot.IsEmpty)
                {
                    targetEquipSlot.Equip(draggedItem);
                    Debug.Log($"[ЭКИПИРОВКА] Предмет '{itemToMove.name}' успешно перемещен из '{sourceEquipSlot.Cell.name}' в пустой слот '{targetEquipSlot.Cell.name}'.");
                }
                else
                {
                    // Если целевой слот занят, и это между EquipSlots, то нужно подумать,
                    // как выполнить свап без участия ItemContainer.
                    // Например, обменяться ItemVisual'ами и обновить ItemBaseDefinition.
                    // Для простоты, пока просто вернемDraggedItem обратно, если свап не реализован.
                    sourceEquipSlot.Equip(draggedItem); // Возвращаем обратно в исходный слот
                    Debug.LogWarning($"[ЭКИПИРОВКА] Не удалось переместить '{itemToMove.name}' из '{sourceEquipSlot.Cell.name}' в '{targetEquipSlot.Cell.name}'. Целевой слот занят, и свап между слотами экипировки не реализован.");
                }

            }
            else
            {
                Debug.LogError("Неизвестный тип источника или цели для перемещения предмета!");
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
        internal void OpenInventoryFromGiveItem(int itemID)
        {
            _givenItem = _inventoryItemContainer.GetItemByItemID(itemID);
            if (_givenItem != null)
            {
                Debug.Log($"Открываю для выбора {_givenItem.DefinitionName}");
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                SetPage(_inventoryPage.Root, display: true, visible: true, enabled: true);
                _uiDocument.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
            }
        }

        /// <summary>
        /// Откроет инвентарь для выбора указанного предмета.
        /// Если ссылка на предмет null, инвентарь не откроется.
        /// </summary>
        /// <param name="item">Предмет, который нужно выбрать.</param>
        internal void OpenInventoryGiveItem(ItemBaseDefinition item)
        {
            _givenItem = item;
            if (_givenItem != null)
            {
                Debug.Log($"Открываю для выбора {_givenItem.DefinitionName}");
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                SetPage(_inventoryPage.Root, display: true, visible: true, enabled: true);
                _uiDocument.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
            }
        }

        /// <summary>
        /// Открывает обычный режим отображения инвентаря.
        /// </summary>
        internal void OpenInventoryAndCraft()
        {
            OpenInventoryNormal();
            OpenCraft();
        }

        public void OpenInventoryNormal()
        {
            _givenItem = null;
            SetPage(_inventoryPage.Root, display: true, visible: true, enabled: true);
            _uiDocument.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
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
            SetPage(_craftPage.Root, display: false, visible: true, enabled: false);
            OpenInventoryNormal();
            SetPage(_cheastPage.Root, display: true, visible: true, enabled: true);
        }



        /// <summary>
        /// Открывает страницу лута.
        /// </summary>
        internal void OpenLut()
        {
            SetPage(_craftPage.Root, display: false, visible: true, enabled: false);
            OpenInventoryNormal();
            SetPage(_lutPage.Root, display: true, visible: true, enabled: true);
        }
        /// <summary>
        /// Закрывает все страницы.
        /// </summary>
        internal void CloseAll()
        {
            SetPage(_inventoryPage.Root, display: false, visible: false, enabled: false);
            SetAllSelfPage(display: false, visible: false, enabled: false);
            _uiDocument.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);
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
        /// Перемещает предмет из инвентаря в слот экипировки.
        /// </summary>
        internal void TransferItemToEquipSlot(ItemVisual draggedItem, ItemContainer sourceContainer, EquipmentSlot targetEquipSlot)
        {
            var itemToEquip = draggedItem.ItemDefinition;

            Debug.Log($"[ЭКИПИРОВКА] Попытка экипировать предмет '{itemToEquip.name}' из '{sourceContainer.name}' в слот экипировки.");

            // Если слот экипировки занят, сначала "забираем" оттуда текущий предмет
            if (!targetEquipSlot.IsEmpty)
            {
                ItemBaseDefinition currentlyEquippedItem = targetEquipSlot.EquippedItem;
                Debug.Log($"[ЭКИПИРОВКА] Слот экипировки занят предметом '{currentlyEquippedItem.name}'. Производим обмен.");

                // Убираем текущий предмет из слота экипировки (он становится перетаскиваемым)
                targetEquipSlot.Unequip();
                // Возвращаем снятый предмет обратно в исходный инвентарь (если получится)
                if (sourceContainer.TryAddItemAtPosition(currentlyEquippedItem, currentlyEquippedItem.GridPosition)) // Пытаемся вернуть на старую позицию
                {
                    Debug.Log($"[ЭКИПИРОВКА] Предмет '{currentlyEquippedItem.name}' возвращен в контейнер '{sourceContainer.name}'.");
                }
                else // Если не получилось, просто добавляем куда-нибудь
                {
                    sourceContainer.AddItems(new List<ItemBaseDefinition> { currentlyEquippedItem });
                    Debug.Log($"[ЭКИПИРОВКА] Предмет '{currentlyEquippedItem.name}' возвращен в контейнер '{sourceContainer.name}' (на любое свободное место).");
                }
            }

            // Удаляем предмет из исходного контейнера (инвентаря)
            sourceContainer.RemoveItem(itemToEquip, destroy: false);
            Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToEquip.name}' был забран из контейнера '{sourceContainer.name}'.");

            // Экипируем новый предмет в слот
            targetEquipSlot.Equip(draggedItem);
            Debug.Log($"[ЭКИПИРОВКА] Предмет '{itemToEquip.name}' успешно экипирован в слот '{targetEquipSlot.Cell.name}'.");
        }

        /// <summary>
        /// Перемещает предмет из слота экипировки в инвентарь.
        /// </summary>
        internal void TransferItemFromEquipSlot(ItemVisual draggedItem, EquipmentSlot sourceEquipSlot, ItemContainer targetContainer, Vector2Int gridPosition)
        {
            var itemToUnequip = draggedItem.ItemDefinition;

            Debug.Log($"[ЭКИПИРОВКА] Попытка снять предмет '{itemToUnequip.name}' из слота '{sourceEquipSlot.Cell.name}' и поместить в '{targetContainer.name}'.");

            // Снимаем предмет из слота экипировки
            sourceEquipSlot.Unequip();
            Debug.Log($"[ЭКИПИРОВКА] Предмет '{itemToUnequip.name}' был снят из слота '{sourceEquipSlot.Cell.name}'.");

            // Пытаемся добавить предмет в целевой контейнер (инвентарь)
            bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToUnequip, gridPosition);

            if (addedToTarget)
            {
                Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToUnequip.name}' был положен в контейнер '{targetContainer.name}' в позицию: {gridPosition}.");
            }
            else
            {
                // Если не удалось добавить в инвентарь, возвращаем его обратно в слот экипировки (хотя такого быть не должно при корректной проверке)
                sourceEquipSlot.Equip(draggedItem);
                Debug.LogWarning($"[ЭКИПИРОВКА] Не удалось поместить предмет '{itemToUnequip.name}' в инвентарь '{targetContainer.name}'. Возвращен в слот экипировки.");
            }
        }
        /// <summary>
        /// Создает новый ItemVisual для отображения в слоте экипировки.
        /// </summary>
        /// <param name="itemDefinition">Определение предмета.</param>
        /// <returns>Созданный ItemVisual.</returns>
        public ItemVisual CreateItemVisualForEquipPage(ItemBaseDefinition itemDefinition)
        {
            // Здесь мы используем существующий конструктор ItemVisual.
            // Параметры ownerInventory, gridPosition и gridSize - это заглушки,
            // так как ItemVisual для слотов экипировки не управляется напрямую сеткой инвентаря.
            // ItemsPage.ContainersAndPages[0].Page используется как пример IDropTarget для ownerInventory.
            // В идеале, EquipPage должен будет передавать свой собственный CellSize и UIDocument,
            // но так как ItemVisual менять нельзя, мы используем то, что доступно.
            return new ItemVisual(
                itemsPage: this,
                ownerInventory: ContainersAndPages[0].Page, // Предполагаем, что первый Page в ContainersAndPages существует и является IDropTarget
                itemDefinition: itemDefinition,
                gridPosition: Vector2Int.zero, // Заглушка
                gridSize: Vector2Int.zero // Заглушка
            );
        }
    }
}