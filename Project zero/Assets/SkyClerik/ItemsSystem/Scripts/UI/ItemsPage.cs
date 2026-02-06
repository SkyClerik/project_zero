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
                        Debug.Log($"Проверка слота: {equipSlot.Cell.name}, Rect: {equipSlot.Rect}, Mouse Local in EquipGrid: {MouseUILocalPosition}");

                        // Если мышка над слотом, то делегируем проверку этому EquipmentSlot
                        PlacementResults equipResults = equipSlot.ShowPlacementTarget(draggedItem);
                        Debug.Log($"[ItemsPage][Placement] Результат для слота экипировки '{equipSlot.Cell.name}': {equipResults.Conflict}");
                        if (equipResults.Conflict != ReasonConflict.beyondTheGridBoundary)
                        {
                            Debug.Log($"Мышка над слотом: {equipSlot.Cell.name}"); // Новый Debug.Log
                            return equipResults;
                        }
                    }
                }
            }

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

            // --- NEW LOGIC TO HANDLE THE DRAGGED ITEM VISUAL ---
            if (CurrentDraggedItem != null)
            {
                // Check if the dragged item is still attached to the root (which it should be if it was dragged)
                if (CurrentDraggedItem.parent == _uiDocument.rootVisualElement)
                {
                    CurrentDraggedItem.RemoveFromHierarchy();
                }
                CurrentDraggedItem = null; // Clear the static reference
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
            Debug.Log($"[ItemsPage][Transfer] Dragged: {draggedItem.ItemDefinition.name}");
            Debug.Log($"[ItemsPage][Transfer] Source Inventory Type: {sourceInventory?.GetType().Name}");
            Debug.Log($"[ItemsPage][Transfer] Target Inventory Type: {targetInventory?.GetType().Name}");

            var itemToMove = draggedItem.ItemDefinition;

            GridPageElementBase sourceGridPage = sourceInventory as GridPageElementBase;
            GridPageElementBase targetGridPage = targetInventory as GridPageElementBase;
            EquipmentSlot sourceEquipSlot = sourceInventory as EquipmentSlot;
            EquipmentSlot targetEquipSlot = targetInventory as EquipmentSlot;

            Debug.Log($"[ItemsPage][Transfer] SourceGridPage: {sourceGridPage != null}, TargetGridPage: {targetGridPage != null}, SourceEquipSlot: {sourceEquipSlot != null}, TargetEquipSlot: {targetEquipSlot != null}");

            // Случай 1: Из инвентаря в инвентарь
            if (sourceGridPage != null && targetGridPage != null)
            {
                Debug.Log($"Случай 1: Из инвентаря в инвентарь");

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
                Debug.Log($"Cлучай 2: Из инвентаря в слот экипировки");

               var sourceContainer = sourceGridPage.ItemContainer;
                EquipPage equipPage = ServiceProvider.Get<EquipPage>();
                if (equipPage != null)
                {
                    equipPage.HandleItemTransfer(draggedItem, sourceContainer, targetEquipSlot);
                }
            }
            // Случай 3: Из слота экипировки в инвентарь
            else if (sourceEquipSlot != null && targetGridPage != null)
            {
                Debug.Log($"Случай 3: Из слота экипировки в инвентарь");

                var targetContainer = targetGridPage.ItemContainer;
                EquipPage equipPage = ServiceProvider.Get<EquipPage>();
                if (equipPage != null)
                {
                    equipPage.HandleItemTransferFromEquip(draggedItem, sourceEquipSlot, targetContainer, gridPosition);
                }
            }
            // Случай 4: Из слота экипировки в слот экипировки
            else if (sourceEquipSlot != null && targetEquipSlot != null)
            {
                Debug.Log($"Случай 4: Из слота экипировки в слот экипировки");

                // draggedItem - это предмет, который был "поднят" из sourceEquipSlot.
                // sourceEquipSlot уже пуст после Unequip, вызванного при поднятии предмета.

                var itemToSwapFromTarget = targetEquipSlot.EquippedItem; // Предмет, который находится в целевом слоте (может быть null, если слот пуст)
                ItemVisual itemToSwapFromTargetVisual = targetEquipSlot.IsEmpty ? null : targetEquipSlot.ItemVisual; // Визуальный элемент предмета в целевом слоте

                // 1. Проверяем, может ли draggedItem быть экипирован в targetEquipSlot
                bool canDraggedItemEquipTarget = targetEquipSlot.CanEquip(draggedItem.ItemDefinition);

                // 2. Если targetEquipSlot пуст И draggedItem подходит для него
                if (targetEquipSlot.IsEmpty && canDraggedItemEquipTarget)
                {
                    targetEquipSlot.Equip(draggedItem);
                    Debug.Log($"[ЭКИПИРОВКА] Предмет '{draggedItem.ItemDefinition.name}' успешно перемещен из исходного слота в пустой целевой слот '{targetEquipSlot.Cell.name}'.");
                }
                // 3. Если targetEquipSlot занят И draggedItem подходит для него И itemToSwapFromTarget подходит для sourceEquipSlot
                else if (!targetEquipSlot.IsEmpty && canDraggedItemEquipTarget && sourceEquipSlot.CanEquip(itemToSwapFromTarget))
                {
                    // Выполняем свап
                    Debug.Log($"[ЭКИПИРОВКА] Выполняем свап между '{draggedItem.ItemDefinition.name}' ({sourceEquipSlot.Cell.name}) и '{itemToSwapFromTarget.name}' ({targetEquipSlot.Cell.name}).");

                    // Экипируем draggedItem в целевой слот
                    // Equip в EquipmentSlot уже позаботится об Unequip'е старого предмета из целевого слота
                    // и ItemsPage.CurrentDraggedItem будет ItemVisual от itemToSwapFromTarget
                    targetEquipSlot.Equip(draggedItem);

                    // Теперь ItemsPage.CurrentDraggedItem - это ItemVisual, который был в targetEquipSlot
                    // Экипируем его в исходный слот
                    sourceEquipSlot.Equip(ItemsPage.CurrentDraggedItem);

                    Debug.Log($"[ЭКИПИРОВКА] Свап завершен. '{draggedItem.ItemDefinition.name}' теперь в '{targetEquipSlot.Cell.name}', а '{itemToSwapFromTarget.name}' в '{sourceEquipSlot.Cell.name}'.");
                }
                // 4. Если свап невозможен, или draggedItem не подходит для targetEquipSlot
                else
                {
                    // Возвращаем draggedItem обратно в sourceEquipSlot
                    sourceEquipSlot.Equip(draggedItem);
                    Debug.LogWarning($"[ЭКИПИРОВКА] Перемещение '{draggedItem.ItemDefinition.name}' из '{sourceEquipSlot.Cell.name}' в '{targetEquipSlot.Cell.name}' невозможно. Предмет возвращен в исходный слот.");
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
    }
}