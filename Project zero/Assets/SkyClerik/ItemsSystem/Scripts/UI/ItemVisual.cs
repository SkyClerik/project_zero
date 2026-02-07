using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;
using SkyClerik.EquipmentSystem;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет визуальный элемент предмета в UI сетке.
    /// Отвечает за отображение предмета, его взаимодействие с мышью (перетаскивание, поворот)
    /// и координацию с логикой размещения на странице.
    /// </summary>
    public class ItemVisual : VisualElement
    {
        private ItemsPage _itemsPage;
        private IDropTarget _ownerInventory;
        private ItemBaseDefinition _itemDefinition;
        private Vector2 _originalPosition;
        private Vector2Int _originalScale;
        private bool _isDragging;
        private bool _hasNoHome = false;
        private PlacementResults _placementResults;
        private VisualElement _icon;
        private Label _pcsText;
        private bool _singleRotationMode;
        private float _saveAngle;
        private const string _iconName = "Icon";
        private const int IconPadding = 5;

        public IDropTarget OwnerInventory => _ownerInventory;

        /// <summary>
        /// Возвращает определение предмета (<see cref="ItemBaseDefinition"/>), связанное с этим визуальным элементом.
        /// </summary>
        public ItemBaseDefinition ItemDefinition
        {
            get => _itemDefinition;
            set => _itemDefinition = value; // Добавляем сеттер
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemVisual"/>.
        /// </summary>
        /// <param name="itemsPage">Ссылка на главную страницу предметов.</param>
        /// <param name="ownerInventory">Инвентарь-владелец, которому принадлежит этот визуальный предмет.</param>
        /// <param name="itemDefinition">Определение предмета, связанное с этим визуальным элементом.</param>
        /// <param name="gridPosition">Начальная позиция предмета в сетке.</param>
        /// <param name="gridSize">Размер предмета в ячейках сетки.</param>
        /// <param name="singleRotationMode">Если true, предмет поворачивается только на 90 градусов (вертикально/горизонтально).</param>
        public ItemVisual(ItemsPage itemsPage, IDropTarget ownerInventory, ItemBaseDefinition itemDefinition, Vector2Int gridPosition, Vector2Int gridSize, bool singleRotationMode = true)
        {
            _itemsPage = itemsPage;
            _ownerInventory = ownerInventory;
            _itemDefinition = itemDefinition;
            _singleRotationMode = singleRotationMode;
            name = _itemDefinition.DefinitionName;
            style.position = Position.Absolute;
            this.SetPadding(IconPadding);

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

            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        ~ItemVisual()
        {
            UnregisterCallback<MouseUpEvent>(OnMouseUp);
            UnregisterCallback<MouseDownEvent>(OnMouseDown);
            UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
            UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        /// <summary>
        /// Устанавливает позицию визуального элемента предмета на UI.
        /// </summary>
        /// <param name="pos">Новая позиция элемента (top, left).</param>
        public void SetPosition(Vector2 pos)
        {
            style.top = pos.y;
            style.left = pos.x;
        }

        /// <summary>
        /// Возвращает текущую позицию визуального элемента предмета на UI.
        /// </summary>
        public Vector2 GetPosition => new Vector2(style.top.value.value, style.left.value.value);

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (_isDragging)
                return;

            _itemsPage.StartTooltipDelay(this);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _itemsPage.StopTooltipDelayAndHideTooltip();
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

            float iconWidth, iconHeight;

            if (angle == 90 || angle == 270)
            {
                iconWidth = logicalHeight * _ownerInventory.CellSize.x;
                iconHeight = logicalWidth * _ownerInventory.CellSize.y;
            }
            else
            {
                iconWidth = logicalWidth * _ownerInventory.CellSize.x;
                iconHeight = logicalHeight * _ownerInventory.CellSize.y;
            }

            _icon.style.width = iconWidth;
            _icon.style.height = iconHeight;

            _icon.style.left = (parentWidth - iconWidth) / 2;
            _icon.style.top = (parentHeight - iconHeight) / 2;
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

        private void OnMouseUp(MouseUpEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (!_isDragging)
                    return;

                if (EquipPage.IsShow)
                {
                    Debug.Log($"[ItemVisual][HandleEquipmentPlacementToInventory] После SetOwnerInventory, _ownerInventory: {_ownerInventory?.GetType().Name ?? "NULL"}");
                    bool flowControl = FromEquip();
                    if (!flowControl)
                    {
                        return;
                    }
                }
                else
                {
                    Debug.Log($"[ItemVisual][HandleEquipmentPlacementToInventory] После SetOwnerInventory, _ownerInventory: {_ownerInventory?.GetType().Name ?? "NULL"}");
                    bool flowControl = FromContainers();
                    if (!flowControl)
                    {
                        return;
                    }
                }
                _isDragging = false;
            }
        }
        private bool FromEquip()
        {
            EquipPage equipPage = ServiceProvider.Get<EquipPage>();
            _placementResults = equipPage.ProcessDragFeedback(this, _itemsPage.MouseUILocalPosition); // Получаем результаты от экипировки

            if (_placementResults.TargetInventory is EquipmentSlot targetEquipmentSlot) // Если цель - слот экипировки
            {
                return HandleEquipmentDrop(targetEquipmentSlot);
            }
            else // Если цель - НЕ EquipmentSlot (т.е., инвентарь или за пределами UI)
            {
                // Определяем, откуда пришел предмет
                EquipmentSlot sourceEquipSlot = _ownerInventory as EquipmentSlot;
                GridPageElementBase targetGridPage = _placementResults.TargetInventory as GridPageElementBase;

                if (sourceEquipSlot != null && targetGridPage != null) // Если из экипировки в инвентарь
                {
                    HandleUnequipToInventory(_placementResults.TargetInventory, _placementResults.SuggestedGridPosition);
                    return true; // Возвращаем true, так как обработка завершена
                }
                else // Если предмет из инвентаря (или другого контейнера) в инвентарь
                {
                    // Получаем правильные _placementResults для инвентаря.
                    // Затем передаем управление FromContainers(), который уже знает, как работать с этими результатами.
                    _placementResults = _itemsPage.HandleItemPlacement(this);
                    return FromContainers();
                }
            }
        }

        private bool HandleEquipmentDrop(EquipmentSlot targetSlot)
        {
            switch (_placementResults.Conflict)
            {
                case ReasonConflict.SwapAvailable:
                    HandleEquipmentSwap(targetSlot);
                    return false; // Завершаем drag, так как произошел swap, и ItemsPage.CurrentDraggedItem должен быть сброшен в другом месте
                case ReasonConflict.None:
                    HandleEquipmentToEmptySlot(targetSlot);
                    return true;
                default:
                    // Если слот не подходит по типу или какая-то другая причина, возвращаем на место
                    HandleEquipmentFailedPlacement();
                    return false;
            }
        }

        private void HandleEquipmentSwap(EquipmentSlot targetSlot)
        {
            var itemToSwap = _placementResults.OverlapItem;
            targetSlot.PickUp(itemToSwap);
            targetSlot.Drop(this, Vector2Int.zero);
            _itemsPage.FinalizeDragOfItem(this);
        }

        private void HandleEquipmentToEmptySlot(EquipmentSlot targetSlot)
        {
            targetSlot.Drop(this, Vector2Int.zero);
            _itemsPage.FinalizeDragOfItem(this);
        }

        private void HandleEquipmentPlacementToInventory(IDropTarget targetInventory, Vector2Int suggestedGridPosition)
        {
            EquipmentSlot sourceEquipSlot = _ownerInventory as EquipmentSlot;
            GridPageElementBase sourceGridPage = _ownerInventory as GridPageElementBase;
            GridPageElementBase targetGridPage = targetInventory as GridPageElementBase;

            // Если предмет пришел из экипировки (старая логика)
            if (sourceEquipSlot != null && targetGridPage != null)
            {
                var itemToMove = this.ItemDefinition;
                var targetContainer = targetGridPage.ItemContainer;
                bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToMove, suggestedGridPosition);

                if (addedToTarget)
                {
                    this.SetOwnerInventory(targetInventory);
                    Debug.Log($"[ItemVisual][HandleEquipmentPlacementToInventory] После SetOwnerInventory, _ownerInventory: {_ownerInventory?.GetType().Name ?? "NULL"}");
                    targetGridPage.AddItemToInventoryGrid(this);
                    SetPosition(new Vector2(suggestedGridPosition.x * targetGridPage.CellSize.x, suggestedGridPosition.y * targetGridPage.CellSize.y));
                }
                else
                    sourceEquipSlot.Equip(this);
            }
            // Если предмет пришел из инвентаря (новая логика, взятая из FromContainers)
            else if (sourceGridPage != null)
            {
                if (targetInventory == _ownerInventory) // Если перемещаем внутри того же инвентаря
                {
                    _ownerInventory.AddItemToInventoryGrid(this);
                    _ownerInventory.Drop(this, suggestedGridPosition);
                    SetPosition(new Vector2(suggestedGridPosition.x * _ownerInventory.CellSize.x, suggestedGridPosition.y * _ownerInventory.CellSize.y));
                }
                else // Если перемещаем в другой инвентарь (например, из инвентаря в сундук, когда открыта экипировка)
                {
                    Placement(suggestedGridPosition);
                }
            }

            targetInventory.FinalizeDrag();
            _itemsPage.FinalizeDragOfItem(this);
        }

        private void HandleEquipmentFailedPlacement()
        {
            TryDropBack();
            _itemsPage.FinalizeDragOfItem(this);
        }

        private bool HandleEquipmentToInventoryOrDropBack(IDropTarget targetInventory, Vector2Int suggestedGridPosition)
        {
            switch (_placementResults.Conflict)
            {
                case ReasonConflict.None:
                    HandleEquipmentPlacementToInventory(targetInventory, suggestedGridPosition);
                    _itemsPage.FinalizeDragOfItem(this);
                    return true;
                default:
                    HandleEquipmentFailedPlacement();
                    return false;
            }
        }


        private bool FromContainers()
        {
            _placementResults = _itemsPage.HandleItemPlacement(this);

            switch (_placementResults.Conflict)
            {
                case ReasonConflict.SwapAvailable:
                    HandleSwap();
                    return false;
                case ReasonConflict.StackAvailable:
                    var targetItemVisual = _placementResults.OverlapItem;
                    int spaceAvailable = targetItemVisual.ItemDefinition.MaxStack - targetItemVisual.ItemDefinition.Stack;
                    int amountToTransfer = Mathf.Min(spaceAvailable, this.ItemDefinition.Stack);

                    if (amountToTransfer > 0)
                    {
                        targetItemVisual.ItemDefinition.AddStack(amountToTransfer, out _);
                        this.ItemDefinition.RemoveStack(amountToTransfer);
                        targetItemVisual.UpdatePcs();
                        this.UpdatePcs();
                    }

                    _placementResults.TargetInventory.FinalizeDrag();

                    if (this.ItemDefinition.Stack <= 0)
                    {
                        _isDragging = false;
                        ItemsPage.CurrentDraggedItem = null;

                        this.style.display = DisplayStyle.None;

                        this.schedule.Execute(() =>
                        {
                            var ownerGrid = _ownerInventory as GridPageElementBase;
                            if (ownerGrid != null)
                            {
                                ownerGrid.ItemContainer.RemoveItem(this.ItemDefinition, destroy: true);
                            }
                            this.RemoveFromHierarchy();
                        }).ExecuteLater(1);
                    }
                    return false;
            }

            _isDragging = false;
            style.opacity = 1f;

            switch (_placementResults.Conflict)
            {
                case ReasonConflict.None:
                    if (_placementResults.TargetInventory == _ownerInventory)
                    {
                        _ownerInventory.AddItemToInventoryGrid(this);
                        _ownerInventory.Drop(this, _placementResults.SuggestedGridPosition);
                        SetPosition(_placementResults.Position);
                    }
                    else
                    {
                        Placement(_placementResults.SuggestedGridPosition);
                    }
                    break;
                default:
                    TryDropBack();
                    break;
            }

            _itemsPage.FinalizeDragOfItem(this);
            return true;
        }

        private void OnMouseDown(MouseDownEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (_itemsPage.GiveItem != null)
                {
                    ServiceProvider.Get<InventoryAPI>().RiseItemGiveEvent(_itemDefinition);
                }
                else
                {
                    if (ItemsPage.CurrentDraggedItem != this)
                    {
                        PickUp();
                        SetDraggedItemPosition(mouseEvent.mousePosition, mouseEvent.localMousePosition);
                    }
                }
            }
        }

        public void SetDraggedItemPosition(Vector2 globalClickPosition, Vector2 localClickOffset)
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

            _placementResults = _itemsPage.HandleItemPlacement(this);
        }

        /// <summary>
        /// Поднимает визуальный элемент предмета, инициируя процесс перетаскивания.
        /// </summary>
        /// <param name="isSwap">Указывает, является ли подъем частью операции обмена.</param>
        public void PickUp(bool isSwap = false)
        {
            Debug.Log($"[ItemVisual][PickUp] PickUp вызывается для {ItemDefinition.name}. isSwap: {isSwap}");
            Debug.Log($"[ItemVisual][PickUp] ownerInventory Type: {_ownerInventory?.GetType().Name}");

            _isDragging = true;
            _hasNoHome = isSwap;
            style.opacity = 0.7f;

            if (!_hasNoHome)
            {
                ItemGridData currentGridData = _ownerInventory.GetItemGridData(this);
                if (currentGridData != null)
                    _originalPosition = new Vector2(currentGridData.GridPosition.x * _ownerInventory.CellSize.x, currentGridData.GridPosition.y * _ownerInventory.CellSize.y);
                else
                    _originalPosition = Vector2.zero;
            }

            _saveAngle = _itemDefinition.Dimensions.Angle;
            _originalScale = new Vector2Int(_itemDefinition.Dimensions.Width, _itemDefinition.Dimensions.Height);

            _itemsPage.StopTooltipDelayAndHideTooltip();
            this.style.position = Position.Absolute;
            _ownerInventory.PickUp(this);
            _placementResults = _itemsPage.HandleItemPlacement(this);
        }

        /// <summary>
        /// Устанавливает инвентарь-владелец для этого визуального элемента предмета.
        /// </summary>
        /// <param name="dropTarget">Новый инвентарь-владелец.</param>
        public void SetOwnerInventory(IDropTarget dropTarget)
        {
            Debug.Log($"[ItemVisual][SetOwnerInventory] Предмет '{this.name}'. Изменение владельца. Старый: {_ownerInventory?.GetType().Name ?? "NULL"}. Новый: {dropTarget?.GetType().Name ?? "NULL"}.");
            _ownerInventory = dropTarget;
            if (ItemsPage.CurrentDraggedItem != null)
            {
                // Используем _ownerInventory (приватное поле), так как публичного свойства OwnerInventory нет.
                Debug.Log($"[ItemVisual][SetOwnerInventory] ItemsPage.CurrentDraggedItem: {ItemsPage.CurrentDraggedItem.name}. Его владелец: {ItemsPage.CurrentDraggedItem._ownerInventory?.GetType().Name ?? "NULL"}");
            }
        }

        private void Placement(Vector2Int gridPosition)
        {
            Debug.Log($"[ItemVisual][Placement] Вызов TransferItemBetweenContainers. DraggedItem: {this.name}. Его ownerInventory: {this._ownerInventory?.GetType().Name ?? "NULL"}. TargetInventory: {_placementResults.TargetInventory?.GetType().Name ?? "NULL"}.");
            _itemsPage.TransferItemBetweenContainers(this, _ownerInventory, _placementResults.TargetInventory, gridPosition);
        }

        private void HandleSwap()
        {
            var itemToSwap = _placementResults.OverlapItem;
            var targetInventory = _placementResults.TargetInventory;
            var swapPosition = _placementResults.SuggestedGridPosition;

            itemToSwap.PickUp(isSwap: true);
            _itemsPage.TransferItemBetweenContainers(this, _ownerInventory, targetInventory, swapPosition);
            targetInventory.FinalizeDrag();
        }

        /// <summary>
        /// Обновляет отображение количества стаков предмета (PCS - Pieces).
        /// </summary>
        public void UpdatePcs()
        {
            if (_pcsText != null)
                _pcsText.text = $"{_itemDefinition.Stack}";
        }

        private void TryDropBack()
        {
            if (_hasNoHome)
            {
                PickUp(isSwap: true);
                return;
            }

            RestoreSizeAndRotate();

            Vector2Int originalGridPosition = new Vector2Int(
                Mathf.RoundToInt(_originalPosition.x / _ownerInventory.CellSize.x),
                Mathf.RoundToInt(_originalPosition.y / _ownerInventory.CellSize.y)
            );

            _ownerInventory.Drop(this, originalGridPosition);
            SetPosition(_originalPosition);
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

        private void HandleUnequipToInventory(IDropTarget targetInventory, Vector2Int gridPosition)
        {
            GridPageElementBase targetGridPage = targetInventory as GridPageElementBase;
            EquipmentSlot sourceEquipSlot = _ownerInventory as EquipmentSlot; // _ownerInventory в этот момент - EquipmentSlot

            if (targetGridPage == null || sourceEquipSlot == null)
            {
                Debug.LogError($"[ItemVisual][HandleUnequipToInventory] Некорректные типы инвентаря для перетаскивания из экипировки в инвентарь. Source: {_ownerInventory?.GetType().Name}, Target: {targetInventory?.GetType().Name}");
                return;
            }

            // Логика из "Случая 3"
            var targetContainer = targetGridPage.ItemContainer;
            EquipPage equipPage = ServiceProvider.Get<EquipPage>();
            if (equipPage != null)
            {
                ItemBaseDefinition itemToUnequip = this.ItemDefinition;
                // sourceEquipSlot.Unequip(); // Этот вызов уже происходит в EquipmentSlot.PickUp(), поэтому он закомментирован

                Debug.Log($"[ЭКИПИРОВКА][HandleUnequipToInventory] Предмет '{itemToUnequip.name}' снят из слота '{sourceEquipSlot.Cell.name}'. Попытка добавить в инвентарь '{targetGridPage.Root.name}'.");

                bool addedToTarget = targetContainer.TryAddItemAtPosition(itemToUnequip, gridPosition);
                if (addedToTarget)
                {
                    Debug.Log($"[ЭКИПИРОВКА][HandleUnequipToInventory] Предмет '{itemToUnequip.name}' успешно помещен в контейнер '{targetContainer.name}' в позицию: {gridPosition}.");
                    this.SetOwnerInventory(targetGridPage);
                    targetGridPage.AddItemToInventoryGrid(this); // Добавляем ItemVisual в сетку
                    this.SetPosition(new Vector2(gridPosition.x * targetGridPage.CellSize.x, gridPosition.y * targetGridPage.CellSize.y));
                }
                else
                {
                    // Если не удалось добавить в инвентарь, возвращаем его обратно в слот экипировки
                    sourceEquipSlot.Equip(this);
                    Debug.LogWarning($"[ЭКИПИРОВКА][HandleUnequipToInventory] Не удалось поместить предмет '{itemToUnequip.name}' в инвентарь '{targetContainer.name}'. Возвращен в слот экипировки '{sourceEquipSlot.Cell.name}'.");
                }
            }

            // Завершаем drag в ItemVisual
            targetInventory.FinalizeDrag(); // FinalizeDrag для целевого инвентаря
            _itemsPage.FinalizeDragOfItem(this); // Общая финализация
        }
    }
}