using SkyClerik.EquipmentSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

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
        private const int IconPadding = 10;

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
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        ~ItemVisual()
        {

            UnregisterCallback<PointerUpEvent>(OnMouseUp);
            UnregisterCallback<PointerDownEvent>(OnMouseDown);
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

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (_isDragging)
                return;

            //_itemsPage.StartTooltipDelay(this);            
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

        private void OnMouseUp(PointerUpEvent mouseEvent)
        {
            if (mouseEvent.button == 0 && mouseEvent.clickCount == 1)
            {
                if (ItemsPage.CurrentDraggedItem == null)
                    return;

                if (!_isDragging)
                    return;

                if (EquipPage.IsShow)
                {
                    bool flowControl = FromEquip();
                    if (!flowControl)
                    {
                        _isDragging = false;
                        return;
                    }
                }
                else
                {
                    bool flowControl = FromContainers();
                    if (!flowControl)
                    {
                        _isDragging = false;
                        return;
                    }
                }
            }
        }

        private bool FromEquip()
        {
            EquipPage equipPage = ServiceProvider.Get<EquipPage>();
            _placementResults = equipPage.ProcessDragFeedback(this, _itemsPage.MouseUILocalPosition);

            if (_placementResults.TargetInventory is EquipmentSlot targetEquipmentSlot)
            {
                switch (_placementResults.Conflict)
                {
                    case ReasonConflict.SwapAvailable:
                        // Сохраняем данные
                        var itemToSwapVisual = _placementResults.OverlapItem; // Визуальный элемент предмета, который был в слоте
                        var itemToSwapDefinition = itemToSwapVisual.ItemDefinition; // Определение предмета, который был в слоте
                        IDropTarget originalOwnerOfDraggedItem = this.OwnerInventory;

                        // Если 'this' пришел из другого слота экипировки возвращаем 'this' на его место.
                        if (_ownerInventory is EquipmentSlot)
                        {
                            TryDropBack();
                            _itemsPage.FinalizeDragOfItem();
                            return false;
                        }

                        // Если 'this' пришел из инвентаря
                        if (originalOwnerOfDraggedItem is GridPageElementBase originalGridPage)
                        {
                            // Вызываем PickUp для itemToSwapVisual. Это очистит слот (_itemVisual = null; _equippedItem = null;)
                            targetEquipmentSlot.PickUp(itemToSwapVisual);
                            // --- 2. Экипируем this (предмет из руки) в targetEquipmentSlot ---
                            targetEquipmentSlot.Drop(this, Vector2Int.zero);

                            // --- 3. Положить itemToSwapVisual в место, откуда взяли this ---
                            var originalContainer = originalGridPage.ItemContainer;
                            if (originalContainer != null)
                            {
                                // Убедимся, что GridPosition itemToSwapDefinition соответствует месту this
                                itemToSwapDefinition.GridPosition = GetOriginalPosition();
                                Debug.LogWarning($"itemToSwapDefinition.GridPosition : {itemToSwapDefinition.GridPosition}");

                                bool added = originalContainer.TryAddItemAtPosition(itemToSwapDefinition, itemToSwapDefinition.GridPosition);
                                if (added)
                                {
                                    // Обновляем владельца и визуальное представление itemToSwap
                                    itemToSwapVisual.SetOwnerInventory(originalGridPage);
                                    originalGridPage.AddItemToInventoryGrid(itemToSwapVisual); // Добавляем визуально в сетку
                                    originalGridPage.RegisterVisual(itemToSwapVisual, new ItemGridData(itemToSwapDefinition, itemToSwapDefinition.GridPosition)); // Регистрируем визуальный элемент
                                    itemToSwapVisual.SetPosition(new Vector2(itemToSwapDefinition.GridPosition.x * originalGridPage.CellSize.x, itemToSwapDefinition.GridPosition.y * originalGridPage.CellSize.y));
                                    itemToSwapVisual.RemoveFromHierarchy();
                                    _itemsPage.FinalizeDragOfItem();
                                    _isDragging = false;
                                    // this лежит в слоте экипировки
                                    style.opacity = 1f;
                                    return false;
                                }
                                else
                                {
                                    // Если по какой-то причине не влез на точное место, пытаемся найти любое свободное.
                                    // Это должно быть очень редким случаем, так как место this было только что освобождено.
                                    Debug.LogWarning($"[ItemVisual][FromEquip][Swap] Не удалось поместить itemToSwap '{itemToSwapDefinition.name}' на оригинальную позицию this({itemToSwapDefinition.GridPosition}). Ищем свободное место.");
                                    originalContainer.AddItems(new List<ItemBaseDefinition> { itemToSwapDefinition });
                                    // Визуальное обновление произойдет по событию OnItemAdded в GridPageElementBase
                                    return false;
                                }
                            }

                            return true;
                        }
                        // это непредвиденный сценарий. Возвращаем 'this' на место.
                        TryDropBack();
                        _itemsPage.FinalizeDragOfItem();
                        return false;
                    case ReasonConflict.None:
                        targetEquipmentSlot.Drop(this, Vector2Int.zero);
                        _itemsPage.FinalizeDragOfItem();
                        return true;
                    default:
                        TryDropBack();
                        _itemsPage.FinalizeDragOfItem();
                        return false;
                }
            }
            else
            {
                EquipmentSlot sourceEquipSlot = _ownerInventory as EquipmentSlot;
                _placementResults = _itemsPage.HandleItemPlacement(this);
                GridPageElementBase targetGridPage = _placementResults.TargetInventory as GridPageElementBase;

                if (sourceEquipSlot != null && targetGridPage != null)
                {
                    //Debug.Log($"[ItemVisual][Placement] Вызов TransferItemBetweenContainers. DraggedItem: {this.name}. Его ownerInventory: {this._ownerInventory?.GetType().Name ?? "NULL"}. TargetInventory: {_placementResults.TargetInventory?.GetType().Name ?? "NULL"}.");
                    var targetContainer = targetGridPage.ItemContainer;
                    //Debug.Log($"[ЭКИПИРОВКА][FromEquip] Предмет '{_itemDefinition.name}' снят из слота '{sourceEquipSlot.Cell.name}'. Попытка добавить в инвентарь '{targetGridPage.Root.name}'.");
                    targetGridPage.SuppressNextVisualCreation = true;
                    bool addedToTarget = targetContainer.TryAddItemAtPosition(_itemDefinition, _placementResults.SuggestedGridPosition);
                    if (addedToTarget)
                    {
                        //Debug.Log($"[ЭКИПИРОВКА][FromEquip] Предмет '{_itemDefinition.name}' успешно помещен в контейнер '{targetContainer.name}' в позицию: {_placementResults.SuggestedGridPosition}.");
                        this.SetOwnerInventory(targetGridPage);
                        //Debug.Log($"[ЭКИПИРОВКА][FromEquip][{this.name}] RemoveFromHierarchy() вызван.");
                        this.RemoveFromHierarchy();
                        //Debug.Log($"[ЭКИПИРОВКА][FromEquip][{this.name}] Добавляем в инвентарную сетку.");
                        targetGridPage.AddItemToInventoryGrid(this);
                        targetGridPage.RegisterVisual(this, new ItemGridData(_itemDefinition, _itemDefinition.GridPosition)); // Регистрируем визуальный элемент
                        this.SetPosition(new Vector2(_placementResults.SuggestedGridPosition.x * targetGridPage.CellSize.x, _placementResults.SuggestedGridPosition.y * targetGridPage.CellSize.y));
                        _isDragging = false;
                        style.opacity = 1f;
                    }
                    else
                    {
                        sourceEquipSlot.Equip(this);
                        //Debug.LogWarning($"[ЭКИПИРОВКА][FromEquip] Не удалось поместить предмет '{_itemDefinition.name}' в инвентарь '{targetContainer.name}'. Возвращен в слот экипировки '{sourceEquipSlot.Cell.name}'.");
                    }

                    targetGridPage.FinalizeDrag();
                    _itemsPage.FinalizeDragOfItem();

                    _isDragging = false;
                    style.opacity = 1f;
                    return false;

                }
                //Debug.Log($"[ЭКИПИРОВКА][FromEquip] Неожиданное состояние: предмет из экипировки в инвентарь, но первый IF не сработал. Передаем в FromContainers.");
                return FromContainers();
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
                    else
                    {
                        TryDropBack();
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

            _itemsPage.FinalizeDragOfItem();
            return true;
        }

        private void OnMouseDown(PointerDownEvent mouseEvent)
        {
            if (mouseEvent.button == 0 || mouseEvent.pointerId == 0)
            {
                if (ItemsPage.CurrentDraggedItem != null)
                    return;

                _itemsPage.SetItemDescription(_itemDefinition);
                //Debug.Log($"[ItemVisual][OnMouseDown][{this.name}] Событие MouseDown. GiveItem: {(_itemsPage.GiveItem != null ? _itemsPage.GiveItem.name : "NULL")}. CurrentDraggedItem: {(ItemsPage.CurrentDraggedItem != null ? ItemsPage.CurrentDraggedItem.name : "NULL")}. Этот ItemVisual: {this.name}.");

                if (_itemsPage.GivenItem.DesiredProduct != null)
                {
                    ServiceProvider.Get<InventoryAPI>().RiseItemGiveEvent(_itemDefinition);
                }
                else
                {
                    _itemsPage.MouseUILocalPosition = mouseEvent.position;
                    if (ItemsPage.CurrentDraggedItem != this)
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

            _placementResults = _itemsPage.HandleItemPlacement(this);
        }

        private void PickUp(bool isSwap = false)
        {
            //Debug.Log($"[ItemVisual][PickUp] PickUp вызывается для {ItemDefinition.name}. isSwap: {isSwap}");
            //Debug.Log($"[ItemVisual][PickUp] ownerInventory Type: {_ownerInventory?.GetType().Name}");

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

            //_itemsPage.StopTooltipDelayAndHideTooltip();
            this.style.position = Position.Absolute;
            _ownerInventory.PickUp(this);
            _placementResults = _itemsPage.HandleItemPlacement(this);
        }

        public void SetOwnerInventory(IDropTarget dropTarget)
        {
            //Debug.Log($"[ItemVisual][SetOwnerInventory] Предмет '{this.name}'. Изменение владельца. Старый: {_ownerInventory?.GetType().Name ?? "NULL"}. Новый: {dropTarget?.GetType().Name ?? "NULL"}.");
            _ownerInventory = dropTarget;
        }

        private void Placement(Vector2Int gridPosition)
        {
            ItemBaseDefinition itemToUnequip = this.ItemDefinition;
            //Debug.Log($"[ItemVisual][Placement] Вызов TransferItemBetweenContainers. DraggedItem: {itemToUnequip.name}. Его ownerInventory: {this._ownerInventory?.GetType().Name ?? "NULL"}. TargetInventory: {_placementResults.TargetInventory?.GetType().Name ?? "NULL"}.");
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

            Vector2Int originalGridPosition = GetOriginalPosition();
            _ownerInventory.Drop(this, originalGridPosition);
            SetPosition(_originalPosition);
        }

        private Vector2Int GetOriginalPosition()
        {
            return new Vector2Int(
                Mathf.RoundToInt(_originalPosition.x / _ownerInventory.CellSize.x),
                Mathf.RoundToInt(_originalPosition.y / _ownerInventory.CellSize.y)
            );
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