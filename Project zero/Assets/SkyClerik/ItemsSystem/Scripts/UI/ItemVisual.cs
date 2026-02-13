using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет визуальный элемент предмета в UI сетке.
    /// Отвечает за отображение предмета, его взаимодействие с мышью (перетаскивание, поворот)
    /// и координацию с логикой размещения на странице.
    /// </summary>
    public class ItemVisual : VisualElement
    {
        private InventoryStorage _inventoryStorage;
        private IDropTarget _ownerInventory;
        private ItemBaseDefinition _itemDefinition;
        private Vector2Int _originalGridPosition;
        private Vector2Int _originalScale;
        private bool _isDragging;
        private bool _hasNoHome = false;
        private PlacementResults _placementResults;
        private VisualElement _icon;
        private Label _pcsText;
        private bool _singleRotationMode;
        private float _saveAngle;
        private const string _iconName = "Icon";
        private const int _iconPadding = 30;

        public IDropTarget OwnerInventory => _ownerInventory;

        /// <summary>
        /// Возвращает определение предмета (<see cref="ItemBaseDefinition"/>), связанное с этим визуальным элементом.
        /// </summary>
        public ItemBaseDefinition ItemDefinition
        {
            get => _itemDefinition;
            set => _itemDefinition = value;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemVisual"/>.
        /// </summary>
        /// <param name="inventoryStorage">Ссылка на главную страницу предметов.</param>
        /// <param name="ownerInventory">Инвентарь-владелец, которому принадлежит этот визуальный предмет.</param>
        /// <param name="itemDefinition">Определение предмета, связанное с этим визуальным элементом.</param>
        /// <param name="gridPosition">Начальная позиция предмета в сетке.</param>
        /// <param name="gridSize">Размер предмета в ячейках сетки.</param>
        /// <param name="singleRotationMode">Если true, предмет поворачивается только на 90 градусов (вертикально/горизонтально).</param>
        public ItemVisual(InventoryStorage inventoryStorage, IDropTarget ownerInventory, ItemBaseDefinition itemDefinition, Vector2Int gridPosition, Vector2Int gridSize, bool singleRotationMode = true)
        {
            _inventoryStorage = inventoryStorage;
            _ownerInventory = ownerInventory;
            _itemDefinition = itemDefinition;
            _singleRotationMode = singleRotationMode;
            name = _itemDefinition.DefinitionName;
            style.position = Position.Absolute;
            style.alignContent = Align.Center;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            //Debug.Log($"[ItemVisual Constructor] Создан ItemVisual для предмета: {_itemDefinition.DefinitionName}. ID: {ItemDefinition.ID}. Owner: {_ownerInventory?.GetType().Name}.");

            style.left = gridPosition.x * _ownerInventory.CellSize.x;
            style.top = gridPosition.y * _ownerInventory.CellSize.y;
            style.width = gridSize.x * _ownerInventory.CellSize.x;
            style.height = gridSize.y * _ownerInventory.CellSize.y;

            if (_itemDefinition.Icon == null)
                Debug.LogWarning($"Тут иконка не назначена в предмет {_itemDefinition.name}");

            _icon = new VisualElement
            {
                name = _iconName,
                style =
                {
                    backgroundImage = new StyleBackground(_itemDefinition.Icon),
                    rotate = new Rotate(_itemDefinition.Dimensions.Angle),
                    position = Position.Absolute,
                }
            };

            SetSize();
            UpdateIconLayout();

            if (_itemDefinition.Stackable && _itemDefinition.ViewStackable)
            {
                _pcsText = new Label
                {
                    style =
                    {
                        width = _itemDefinition.Dimensions.Width * _ownerInventory.CellSize.x,
                        height = _itemDefinition.Dimensions.Height * _ownerInventory.CellSize.y,
                        fontSize = 40,
                        color = new StyleColor(Color.blue),
                        alignItems = Align.FlexStart,
                        alignContent = Align.FlexStart,
                        justifyContent = Justify.FlexStart,
                    }
                };
                _pcsText.SetMargin(1);
                _pcsText.SetPadding(1);
                UpdatePcs();
                _icon.Add(_pcsText);
            }

            Add(_icon);

            RegisterCallback<PointerUpEvent>(OnMouseUp);
            RegisterCallback<PointerDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        }

        ~ItemVisual()
        {

            UnregisterCallback<PointerUpEvent>(OnMouseUp);
            UnregisterCallback<PointerDownEvent>(OnMouseDown);
            UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
        }

        /// <summary>
        /// Устанавливает позицию визуального элемента предмета на UI.
        /// </summary>
        /// <param name="pos">Новая позиция элемента (top, left).</param>
        public void SetPosition(Vector2 pos)
        {
            //Debug.Log($"[ItemVisual SetPosition] Устанавливаю позицию для {ItemDefinition.name}. ID: {ItemDefinition.ID}. Новая позиция: {pos}. Текущие top/left: {style.top.value}/{style.left.value}.");
            style.top = pos.y;
            style.left = pos.x;
            //Debug.Log($"[ItemVisual SetPosition] Позиция установлена для {ItemDefinition.name}. ID: {ItemDefinition.ID}. Новые top/left: {style.top.value}/{style.left.value}.");
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (_isDragging)
                return;

            //_itemsPage.StartTooltipDelay(this);            
        }

        /// <summary>
        /// Поворачивает визуальный элемент предмета, если его ширина и высота не равны.
        /// </summary>
        public void Rotate()
        {
            if (_itemDefinition.Dimensions.Width == _itemDefinition.Dimensions.Height)
                return;

            _itemDefinition.Dimensions.Swap();
            SetSize();
            UpdateIconLayout();
            RotateIconRight();
        }

        private void SetSize()
        {
            this.style.width = _itemDefinition.Dimensions.Width * _ownerInventory.CellSize.x;
            this.style.height = _itemDefinition.Dimensions.Height * _ownerInventory.CellSize.y;
        }

        private void UpdateIconLayout()
        {
            var parentWidth = this.style.width.value.value;
            var parentHeight = this.style.height.value.value;

            float angle = _itemDefinition.Dimensions.Angle;
            int logicalWidth = _itemDefinition.Dimensions.Width;
            int logicalHeight = _itemDefinition.Dimensions.Height;

            //float iconWidth, iconHeight;

            //if (angle == 90 || angle == 270)
            //{
            //    iconWidth = logicalHeight * _ownerInventory.CellSize.x;
            //    iconHeight = logicalWidth * _ownerInventory.CellSize.y;
            //}
            //else
            //{
            //    iconWidth = logicalWidth * _ownerInventory.CellSize.x;
            //    iconHeight = logicalHeight * _ownerInventory.CellSize.y;
            //}

            float iconWidth = (angle == 90 || angle == 270) ? logicalHeight * _ownerInventory.CellSize.x : logicalWidth * _ownerInventory.CellSize.x;
            float iconHeight = (angle == 90 || angle == 270) ? logicalWidth * _ownerInventory.CellSize.y : logicalHeight * _ownerInventory.CellSize.y;

            _icon.style.width = iconWidth - _iconPadding;
            _icon.style.height = iconHeight - _iconPadding;
        }

        private void RotateIconRight()
        {
            float angle;
            if (_singleRotationMode)
            {
                angle = (_itemDefinition.Dimensions.Angle == 0) ? 90 : 0;
            }
            else
            {
                angle = _itemDefinition.Dimensions.Angle + 90;
                if (angle >= 360)
                    angle = 0;
            }

            RotateIcon(angle);
            SaveCurrentAngle(angle);
            UpdateIconLayout();
        }

        private void RotateIcon(float angle) => _icon.style.rotate = new Rotate(angle);

        private void SaveCurrentAngle(float angle) => _itemDefinition.Dimensions.Angle = angle;

        private void OnMouseUp(PointerUpEvent mouseEvent)
        {
            //Debug.Log($"[ItemVisual OnMouseUp] MouseUp для {ItemDefinition.name}. IsDragging: {_isDragging}. CurrentDraggedItem: {InventoryContainer.CurrentDraggedItem?.ItemDefinition.DefinitionName ?? "NULL"}.");
            if (mouseEvent.button == 0 && mouseEvent.clickCount == 1)
            {
                if (InventoryStorage.CurrentDraggedItem == null)
                    return;

                if (!_isDragging)
                    return;

                bool flowControl = FromContainers();
                if (!flowControl)
                {
                    _isDragging = false;
                    return;
                }
            }
        }

        private bool FromContainers()
        {
            //Debug.Log($"[ItemVisual FromContainers] Начало FromContainers для {ItemDefinition.name}. ID: {ItemDefinition.ID}."); // ЭТОТ ЛОГ!
            _placementResults = _inventoryStorage.HandleItemPlacement(this);
            //Debug.Log($"[ItemVisual FromContainers] После HandleItemPlacement. Item: {ItemDefinition.name}. ID: {ItemDefinition.ID}. Conflict: {_placementResults.Conflict}. TargetInventory: {_placementResults.TargetInventory?.GetType().Name ?? "NULL"}. SuggestedGridPosition: {_placementResults.SuggestedGridPosition}.");
            switch (_placementResults.Conflict)
            {
                case ReasonConflict.SwapAvailable:
                    HandleSwap();
                    return false;
                case ReasonConflict.StackAvailable:
                    //Debug.Log($"[ItemVisual FromContainers] Conflict: StackAvailable для {ItemDefinition.name}. ID: {ItemDefinition.ID}.");
                    var targetItemVisual = _placementResults.OverlapItem;
                    int spaceAvailable = targetItemVisual.ItemDefinition.MaxStack - targetItemVisual.ItemDefinition.Stack;
                    int amountToTransfer = Mathf.Min(spaceAvailable, this.ItemDefinition.Stack);

                    if (amountToTransfer > 0)
                    {
                        //Debug.Log($"[ItemVisual FromContainers] StackAvailable: Передача {amountToTransfer} стека. Item: {ItemDefinition.name}. ID: {ItemDefinition.ID}.");
                        targetItemVisual.ItemDefinition.AddStack(amountToTransfer, out _);
                        this.ItemDefinition.RemoveStack(amountToTransfer);
                        targetItemVisual.UpdatePcs();
                        this.UpdatePcs();
                    }

                    _placementResults.TargetInventory.FinalizeDrag();

                    if (this.ItemDefinition.Stack <= 0)
                    {
                        _isDragging = false;
                        InventoryStorage.CurrentDraggedItem = null;

                        //Debug.Log($"[ItemVisual FromContainers] Устанавливаю DisplayStyle.None для {ItemDefinition.name}. ID: {ItemDefinition.ID}. Причина: Stack <= 0. (Предполагается удаление)");
                        this.style.display = DisplayStyle.None;

                        this.schedule.Execute(() =>
                        {
                            var ownerGrid = _ownerInventory as GridPageElementBase;
                            if (ownerGrid != null)
                            {
                                Debug.Log($"TransferItemBetweenContainers : {ItemContainer.ItemRemoveReason.Transfer}");
                                ownerGrid.ItemContainer.RemoveItem(this.ItemDefinition, ItemContainer.ItemRemoveReason.Destroy);
                            }
                            //Debug.Log($"[ItemVisual FromContainers] Вызываю RemoveFromHierarchy() для {ItemDefinition.name}. ID: {ItemDefinition.ID}. Причина: Stack <= 0. (Предполагается удаление)");
                            this.RemoveFromHierarchy();
                        }).ExecuteLater(1);
                    }
                    else
                    {
                        //Debug.Log($"[ItemVisual FromContainers] StackAvailable: Стек > 0. Вызываю TryDropBack() для {ItemDefinition.name}. ID: {ItemDefinition.ID}.");
                        TryDropBack();
                    }
                    return false;
            }

            //Debug.Log($"[ItemVisual FromContainers] Не SwapAvailable/StackAvailable. Устанавливаю _isDragging = false и style.opacity = 1f для {ItemDefinition.name}. ID: {ItemDefinition.ID}.");
            _isDragging = false;
            style.opacity = 1f;

            switch (_placementResults.Conflict)
            {
                case ReasonConflict.None:
                    //Debug.Log($"[ItemVisual FromContainers] Conflict: None для {ItemDefinition.name}. ID: {ItemDefinition.ID}. TargetInventory: {_placementResults.TargetInventory?.GetType().Name ?? "NULL"}.");
                    if (_placementResults.TargetInventory == _ownerInventory)
                    {
                        Debug.Log($"<color=purple>[Self-Drop] Возвращаю '{this.ItemDefinition.name}' в тот же контейнер '{(_ownerInventory as GridPageElementBase).Root.name}' на позицию {_placementResults.SuggestedGridPosition}</color>");

                        _ownerInventory.AddItemToInventoryGrid(this);
                        _ownerInventory.Drop(this, _placementResults.SuggestedGridPosition);
                        //SetPosition(_placementResults.Position);
                    }
                    else
                    {
                        //Debug.Log($"[ItemVisual FromContainers] Conflict: None. TargetInventory != OwnerInventory. Вызываю Placement() для {ItemDefinition.name}. ID: {ItemDefinition.ID}.");
                        //Placement(_placementResults.SuggestedGridPosition);
                        _inventoryStorage.TransferItemBetweenContainers(this, _ownerInventory, _placementResults.TargetInventory, _placementResults.SuggestedGridPosition);
                    }
                    break;
                default:
                    //Debug.Log($"[ItemVisual FromContainers] Conflict: {_placementResults.Conflict} (Default case). Вызываю TryDropBack() для {ItemDefinition.name}. ID: {ItemDefinition.ID}.");
                    TryDropBack();
                    break;
            }

            _inventoryStorage.FinalizeDragOfItem();
            //Debug.Log($"[ItemVisual FromContainers] Конец FromContainers для {ItemDefinition.name}. ID: {ItemDefinition.ID}. Возвращаю true.");
            return true;
        }

        private void OnMouseDown(PointerDownEvent mouseEvent)
        {
            //Debug.Log($"[ItemVisual OnMouseDown] MouseDown для {ItemDefinition.name}. ID: {ItemDefinition.ID}. Button: {mouseEvent.button}. PointerId: {mouseEvent.pointerId}. CurrentDraggedItem: {InventoryContainer.CurrentDraggedItem?.ItemDefinition.DefinitionName ?? "NULL"}.");
            if (mouseEvent.button == 0 || mouseEvent.pointerId == 0)
            {
                if (InventoryStorage.CurrentDraggedItem != null)
                    return;

                _inventoryStorage.SetItemDescription(_itemDefinition);
                //Debug.Log($"[ItemVisual][OnMouseDown][{this.name}] Событие MouseDown. GiveItem: {(_itemsPage.GiveItem != null ? _itemsPage.GiveItem.name : "NULL")}. CurrentDraggedItem: {(ItemsPage.CurrentDraggedItem != null ? ItemsPage.CurrentDraggedItem.name : "NULL")}. Этот ItemVisual: {this.name}.");

                if (_inventoryStorage.GivenItem.DesiredProduct != null)
                {
                    ServiceProvider.Get<InventoryAPI>().RiseItemGiveEvent(_itemDefinition);
                }
                else
                {
                    _inventoryStorage.MouseUILocalPosition = mouseEvent.position;
                    if (InventoryStorage.CurrentDraggedItem != this)
                    {
                        //Debug.Log($"[ItemVisual][OnMouseDown][{this.name}] ItemsPage.CurrentDraggedItem НЕ равен этому ItemVisual. Вызываем PickUp.");
                        PickUp();
                        SetDraggedItemPosition(mouseEvent.position, mouseEvent.position);
                    }
                    else
                    {
                        //Debug.Log($"[ItemVisual][OnMouseDown][{this.name}] ItemsPage.CurrentDraggedItem РАВЕН этому ItemVisual. НЕ вызываем PickUp (уже перетаскивается).");
                    }
                }
            }
        }

        private void SetDraggedItemPosition(Vector2 globalClickPosition, Vector2 localClickOffset)
        {
            Vector2 rootOffset = _ownerInventory.GetDocument.rootVisualElement.worldBound.position;
            Vector2 correctedGlobalClickPosition = globalClickPosition - rootOffset;
            float desiredLeft = correctedGlobalClickPosition.x - localClickOffset.x;
            float desiredTop = correctedGlobalClickPosition.y - localClickOffset.y;
            Vector2 newPositionForSet = new Vector2(desiredLeft, desiredTop);
            this.SetPosition(newPositionForSet);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_isDragging)
                return;

            _placementResults = _inventoryStorage.HandleItemPlacement(this);
        }

        private void PickUp(bool isSwap = false)
        {
            _isDragging = true;
            _hasNoHome = isSwap;
            style.opacity = 0.7f;

            if (!_hasNoHome)
                _originalGridPosition = _itemDefinition.GridPosition;

            _saveAngle = _itemDefinition.Dimensions.Angle;
            _originalScale = new Vector2Int(_itemDefinition.Dimensions.Width, _itemDefinition.Dimensions.Height);

            this.style.position = Position.Absolute;
            _ownerInventory.PickUp(this);
            _placementResults = _inventoryStorage.HandleItemPlacement(this);
        }

        public void SetOwnerInventory(IDropTarget dropTarget)
        {
            _ownerInventory = dropTarget;
        }

        public void UpdatePcs()
        {
            if (_pcsText != null)
                _pcsText.text = $"{_itemDefinition.Stack}";
        }

        private void HandleSwap()
        {
            // 1. Определяем стороны обмена
            var equippedItemVisual = _placementResults.OverlapItem;
            var equipSlotInventory = (GridPageElementBase)_placementResults.TargetInventory;
            var originalInventory = (GridPageElementBase)this.OwnerInventory;

            var equippedItemData = equippedItemVisual.ItemDefinition;
            var draggedItemData = this.ItemDefinition;

            // 2. Подавляем автоматическое создание визуалов
            originalInventory.SuppressNextVisualCreation = true;
            equipSlotInventory.SuppressNextVisualCreation = true;

            // 3. Удаляем данные из обоих контейнеров
            originalInventory.ItemContainer.RemoveItem(draggedItemData, ItemContainer.ItemRemoveReason.Transfer);
            equipSlotInventory.ItemContainer.RemoveItem(equippedItemData, ItemContainer.ItemRemoveReason.Transfer);

            // 4a. Помещаем предмет ИЗ ЭКИПИРОВКИ в ИНВЕНТАРЬ
            if (originalInventory.ItemContainer.TryAddItemAtPosition(equippedItemData, _originalGridPosition))
            {
                originalInventory.AdoptExistingVisual(equippedItemVisual);
                ServiceProvider.Get<InventoryAPI>().RiseItemDrop(equippedItemVisual, originalInventory);
            }
            else
            {
                Debug.LogError("Обмен не удался: Не удалось поместить экипированный предмет в исходный слот инвентаря.");
                equipSlotInventory.ItemContainer.TryAddItemAtPosition(equippedItemData, Vector2Int.zero);
                TryDropBack();
                return;
            }

            // 4b. Помещаем ПЕРЕТАСКИВАЕМЫЙ предмет в СЛОТ ЭКИПИРОВКИ
            if (equipSlotInventory.ItemContainer.TryAddItemAtPosition(draggedItemData, Vector2Int.zero))
            {
                equipSlotInventory.AdoptExistingVisual(this);
                ServiceProvider.Get<InventoryAPI>().RiseItemDrop(this, equipSlotInventory);
            }
            else
            {
                Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА ОБМЕНА: Не удалось поместить перетаскиваемый предмет в пустой слот экипировки.");
                //originalInventory.ItemContainer.RemoveItem(equippedItemData, ItemContainer.ItemRemoveReason.Transfer);
                //originalInventory.ItemContainer.TryAddItemAtPosition(equippedItemData, _originalGridPosition);
                TryDropBack();
                return;
            }

            _inventoryStorage.FinalizeDragOfItem();
        }

        private void TryDropBack()
        {
            Debug.Log($"<color=purple>[TryDropBack] Возвращаю предмет '{ItemDefinition.DefinitionName}' в контейнер '{(_ownerInventory as GridPageElementBase).Root.name}' на позицию {_originalGridPosition}</color>");

            if (_hasNoHome)
            {
                PickUp(isSwap: true);
                return;
            }

            RestoreSizeAndRotate();
            _ownerInventory.AddItemToInventoryGrid(this);
            _ownerInventory.Drop(this, _originalGridPosition);
        }

        private void RestoreSizeAndRotate()
        {
            _itemDefinition.Dimensions.Angle = _saveAngle;
            _itemDefinition.Dimensions.Width = _originalScale.x;
            _itemDefinition.Dimensions.Height = _originalScale.y;

            SetSize();
            UpdateIconLayout();
            RotateIcon(_saveAngle);
        }
    }
}