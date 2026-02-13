using SkyClerik.Utils;
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

        public ItemBaseDefinition DesiredProduct { get => _desiredProduct; set => _desiredProduct = value; }
        public ItemVisual Visual { get => _visual; set => _visual = value; }
        public bool Tracing { get => _tracing; set => _tracing = value; }
        public Color TracingColor { get => _tracingColor; set => _tracingColor = value; }
        public int TracingWidth { get => _tracingWidth; set => _tracingWidth = value; }
        public int TracingZero => _tracingZero;
        public int MaxAttempt { get => _maxAttempt; set => _maxAttempt = value; }
    }

    public class InventoryStorage : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private UIDocument _uiDocument;
        [SerializeField]
        private ItemContainer _inventoryItemContainer;
        [SerializeField]
        private ItemContainer _craftItemContainer;
        [SerializeField]
        private ItemContainer _cheastItemContainer;
        [SerializeField]
        private List<EquipmentWrapper> _equipmentWrapper = new List<EquipmentWrapper>();

        private Vector2 _mousePositionOffset;
        private Vector2 _draggedMouseUILocalPosition;
        private Vector2 _draggedItemHalfSize;
        private GlobalGameProperty _globalGameProperty;
        private GivenItem _givenItem = new GivenItem();
        private Vector2 _mouseUILocalPosition;
        private InventoryPageElement _inventoryPage;
        private CraftPageElement _craftPage;
        private CheastPageElement _cheastPage;
        private List<ContainerAndPage> _containersAndPages = new List<ContainerAndPage>();

        public GivenItem GivenItem => _givenItem;
        internal Vector2 MouseUILocalPosition { get => _mouseUILocalPosition; set => _mouseUILocalPosition = value; }
        internal bool IsInventoryVisible { get => _inventoryPage.Root.enabledSelf; set => _inventoryPage.Root.SetEnabled(value); }
        internal bool IsCraftVisible { get => _craftPage.Root.enabledSelf; set => _craftPage.Root.SetEnabled(value); }
        internal bool IsCheastVisible { get => _cheastPage.Root.enabledSelf; set => _cheastPage.Root.SetEnabled(value); }
        internal List<ContainerAndPage> ContainersAndPages => _containersAndPages;

        public static Telegraph _mainTelegraph;
        public static Telegraph MainTelegraph => _mainTelegraph;

        private static ItemVisual _currentDraggedItem;
        public static ItemVisual CurrentDraggedItem { get => _currentDraggedItem; set => _currentDraggedItem = value; }

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

            foreach (ContainerAndPage containerAndPage in _containersAndPages)
            {
                containerAndPage.Page.Dispose();
            }
        }

        protected void Start()
        {
            Input.multiTouchEnabled = false;

            _inventoryPage = new InventoryPageElement(itemsPage: this, document: _uiDocument, itemContainer: _inventoryItemContainer);
            var inventoryCAP = new ContainerAndPage(_inventoryItemContainer, _inventoryPage);
            _containersAndPages.Add(inventoryCAP);

            _craftPage = new CraftPageElement(inventoryStorage: this, document: _uiDocument, itemContainer: _craftItemContainer);
            var craftCAP = new ContainerAndPage(_craftItemContainer, _craftPage);
            _containersAndPages.Add(craftCAP);

            _cheastPage = new CheastPageElement(inventoryStorage: this, document: _uiDocument, itemContainer: _cheastItemContainer);
            var cheastCAP = new ContainerAndPage(_cheastItemContainer, _cheastPage);
            _containersAndPages.Add(cheastCAP);

            foreach (EquipmentWrapper equipmentWrapper in _equipmentWrapper)
            {
                equipmentWrapper.EquipPageElement = new EquipPageElement(this, _uiDocument, equipmentWrapper.ItemContainer, equipmentWrapper.ItemContainer.RootPanelName);
                var cap = new ContainerAndPage(equipmentWrapper.ItemContainer, equipmentWrapper.EquipPageElement);
                _containersAndPages.Add(cap);
            }

            _globalGameProperty = ServiceProvider.Get<GlobalBox>()?.GlobalGameProperty;

            CloseAll();

            _mainTelegraph = new Telegraph();
            _uiDocument.rootVisualElement.Add(_mainTelegraph);
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

            if (Application.platform != RuntimePlatform.Android)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    _currentDraggedItem.Rotate();
                }
            }

            SetDraggedItemPosition();
        }

        /// <summary>
        /// Устанавливает позицию перетаскиваемого предмета на UI в соответствии с положением мыши.
        /// </summary>
        public void SetDraggedItemPosition()
        {
            _draggedItemHalfSize.x = Mathf.Round(_currentDraggedItem.resolvedStyle.width / 2);
            _draggedItemHalfSize.y = Mathf.Round(_currentDraggedItem.resolvedStyle.height / 2);

            _draggedMouseUILocalPosition = new Vector2(Mathf.Round(_mouseUILocalPosition.x), Mathf.Round(_mouseUILocalPosition.y));
            _mousePositionOffset.x = _draggedMouseUILocalPosition.x - _draggedItemHalfSize.x;
            _mousePositionOffset.y = _draggedMouseUILocalPosition.y - _draggedItemHalfSize.y;
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
            if (_containersAndPages.Count != 0)
            {
                foreach (ContainerAndPage containerAndPage in _containersAndPages)
                {
                    var page = containerAndPage.Page;
                    // Только если корневой элемент страницы активен и курсор мыши находится над ее сеткой.
                    if (page.Root.enabledSelf && page.HoverableGridWorldBound.Contains(_mouseUILocalPosition))
                    {
                        //Debug.Log($"[ЛОГ] Страница {containerAndPage.Container.RootPanelName} активна и мышь над ней.");
                        PlacementResults results = page.ShowPlacementTarget(draggedItem);
                        // Если результат не "за пределами сетки" (т.е. мышка находится над действительной областью сетки),
                        // то возвращаем этот результат.
                        if (results.Conflict != ReasonConflict.beyondTheGridBoundary)
                        {
                            return results.Init(results.Conflict, results.Position, results.SuggestedGridPosition, results.OverlapItem, page);
                        }
                    }
                }
            }

            // Если ни одна активная страница не была найдена под курсором или все они вернули "за пределами сетки",
            // то возвращаем общий конфликт "за пределами сетки".
            return new PlacementResults().Init(ReasonConflict.beyondTheGridBoundary, Vector2.zero, Vector2Int.zero, null, null);
        }

        /// <summary>
        /// Завершает операцию перетаскивания для всех страниц.
        /// </summary>
        /// <param name="draggedItem">Визуальный элемент перетаскиваемого предмета.</param>
        public void FinalizeDragOfItem()
        {
            foreach (ContainerAndPage containerAndPage in _containersAndPages)
            {
                containerAndPage.Page.FinalizeDrag();
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

            if (sourceGridPage != null && targetGridPage != null)
            {
                var sourceContainer = sourceGridPage.ItemContainer;
                var targetContainer = targetGridPage.ItemContainer;

                if (sourceContainer == null || targetContainer == null)
                {
                    //Debug.LogError("Не удалось найти контейнеры для перемещения предмета!");
                    return;
                }

                //Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToMove.name}' был забран из контейнера '{sourceContainer.name}'.");
                //Debug.Log($"TransferItemBetweenContainers : {ItemContainer.ItemRemoveReason.Transfer}");
                sourceContainer.RemoveItem(itemToMove, ItemContainer.ItemRemoveReason.Transfer);

                bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToMove, gridPosition);

                if (addedToTarget)
                {
                    //Debug.Log($"[ИНВЕНТАРЬ] Предмет '{itemToMove.name}' был положен в контейнер '{targetContainer.name}' в позицию: {gridPosition}.");
                    ServiceProvider.Get<InventoryAPI>().RiseItemDrop(draggedItem, targetGridPage);
                    targetGridPage.AdoptExistingVisual(draggedItem);
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
        /// Откроет инвентарь для выбора предмета, который найдет по индексу.
        /// Если предмет не будет найден, инвентарь не откроется.
        /// </summary>
        /// <param name="itemID">WrapperIndex искомого предмета.</param>
        internal void OpenInventoryFromGiveItem(int itemID, bool tracing)
        {
            _givenItem.DesiredProduct = _inventoryItemContainer.GetOriginalItemByItemID(itemID);
            GiveItemSettings(_givenItem.DesiredProduct, tracing);

            if (_givenItem.DesiredProduct != null)
            {
                //Debug.Log($"Открываю для выбора {_givenItem.DefinitionName}");
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                OpenInventoryNormal();
            }
            else
            {
                ServiceProvider.Get<InventoryAPI>().RiseItemFindFall(itemID, this.GetType());
            }
        }

        internal void OpenInventoryGiveItem(ItemBaseDefinition item, bool tracing)
        {
            GiveItemSettings(item, tracing);
            if (_givenItem.DesiredProduct != null)
            {
                //Debug.Log($"Открываю для выбора {_givenItem.DefinitionName}");
                SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
                OpenInventoryNormal();
            }
            else
            {
                ServiceProvider.Get<InventoryAPI>().RiseItemFindFall(item.ID, this.GetType());
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

        internal void OpenInventoryAndCraft()
        {
            OpenInventoryNormal();
            OpenCraft();
        }

        internal void OpenInventoryAndEquip()
        {
            OpenInventoryNormal();
            OpenEquip();
        }

        public void OpenInventoryNormal()
        {
            Time.timeScale = 0;
            SetPage(_inventoryPage.Root, display: true, visible: true, enabled: true);
            _uiDocument.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
            _inventoryPage.DisableItemDescription();
        }

        private void OpenCraft()
        {
            SetPage(_craftPage.Root, display: true, visible: false, enabled: false);
            if (_globalGameProperty != null && _globalGameProperty.MakeCraftAccessible)
                SetPage(_craftPage.Root, display: true, visible: true, enabled: true);
        }

        internal void OpenCheast()
        {
            SetPage(_craftPage.Root, display: false, visible: false, enabled: false);
            OpenInventoryNormal();
            SetPage(_cheastPage.Root, display: true, visible: true, enabled: true);
        }

        internal void OpenEquip()
        {
            foreach (EquipmentWrapper equipmentWrapper in _equipmentWrapper)
            {
                SetPage(equipmentWrapper.EquipPageElement.Root, display: true, visible: true, enabled: true);
            }
        }

        public void CloseAll()
        {
            foreach (ContainerAndPage containerAndPage in _containersAndPages)
            {
                SetPage(containerAndPage.Page.Root, display: false, visible: false, enabled: false);
            }

            _uiDocument.rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnRootMouseMove);

            if (_givenItem != null)
            {
                if (_givenItem.DesiredProduct != null)
                    _givenItem.DesiredProduct = null;

                if (_givenItem.Visual != null)
                    ApplyVisualItemHighlight(_givenItem.Visual, isZeroWidth: true);
            }

            Time.timeScale = 1;
        }

        private void SetPage(VisualElement pageRoot, bool display, bool visible, bool enabled)
        {
            pageRoot.SetVisibility(visible);
            pageRoot.SetDisplay(display);
            pageRoot.SetEnabled(enabled);
        }

    }
}