using SkyClerik.EquipmentSystem;
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
            _placementResults = equipPage.ProcessDragFeedback(this, _itemsPage.MouseUILocalPosition); // Получаем результаты от экипировки

            if (_placementResults.TargetInventory is EquipmentSlot targetEquipmentSlot) // Если цель - слот экипировки
            {
                switch (_placementResults.Conflict)
                {
                    case ReasonConflict.SwapAvailable:
                        var itemToSwap = _placementResults.OverlapItem; // Это ItemVisual, который был в целевом слоте экипировки
                        targetEquipmentSlot.PickUp(itemToSwap); // targetEquipmentSlot "поднимает" ItemVisual, он становится ItemsPage.CurrentDraggedItem
                        targetEquipmentSlot.Drop(this, Vector2Int.zero); // targetEquipmentSlot "роняет" наш ItemVisual
                        ItemsPage.CurrentDraggedItem = itemToSwap;
                        itemToSwap._isDragging = true; // Убеждаемся, что флаг перетаскивания установлен для itemToSwap
                        // НЕ вызываем _itemsPage.FinalizeDragOfItem(this) для нашего ItemVisual,
                        // потому что drag продолжается для itemToSwap.
                        return true; // Возвращаем true, чтобы OnMouseUp не сбрасывал _isDragging для itemToSwap, так как drag продолжается.
                    case ReasonConflict.None:
                        targetEquipmentSlot.Drop(this, Vector2Int.zero);
                        _itemsPage.FinalizeDragOfItem(this);
                        return true;
                    default:
                        // Если слот не подходит по типу или какая-то другая причина, возвращаем на место
                        TryDropBack();
                        _itemsPage.FinalizeDragOfItem(this);
                        return false;
                }
            }
            else // Если цель - НЕ EquipmentSlot (т.е., инвентарь или за пределами UI)
            {
                // Определяем, откуда пришел предмет
                EquipmentSlot sourceEquipSlot = _ownerInventory as EquipmentSlot;
                _placementResults = _itemsPage.HandleItemPlacement(this); // НЕ СМЕЙ ЭТО ТРОГАТЬ ДУРАЦКАЯ МАШИНА!!!
                GridPageElementBase targetGridPage = _placementResults.TargetInventory as GridPageElementBase;

                if (sourceEquipSlot != null && targetGridPage != null) // Если из экипировки в инвентарь
                {
                    //Debug.Log($"[ItemVisual][Placement] Вызов TransferItemBetweenContainers. DraggedItem: {this.name}. Его ownerInventory: {this._ownerInventory?.GetType().Name ?? "NULL"}. TargetInventory: {_placementResults.TargetInventory?.GetType().Name ?? "NULL"}.");
                    var targetContainer = targetGridPage.ItemContainer;
                    //Debug.Log($"[ЭКИПИРОВКА][FromEquip] Предмет '{_itemDefinition.name}' снят из слота '{sourceEquipSlot.Cell.name}'. Попытка добавить в инвентарь '{targetGridPage.Root.name}'.");
                    targetGridPage.SuppressNextVisualCreation = true; // Устанавливаем флаг
                    bool addedToTarget = targetContainer.TryAddItemAtPosition(_itemDefinition, _placementResults.SuggestedGridPosition);
                    if (addedToTarget)
                    {
                        //Debug.Log($"[ЭКИПИРОВКА][FromEquip] Предмет '{_itemDefinition.name}' успешно помещен в контейнер '{targetContainer.name}' в позицию: {_placementResults.SuggestedGridPosition}.");
                        this.SetOwnerInventory(targetGridPage);
                        //Debug.Log($"[ЭКИПИРОВКА][FromEquip][{this.name}] RemoveFromHierarchy() вызван.");
                        this.RemoveFromHierarchy(); // Удаляем из старой иерархии
                                                    //Debug.Log($"[ЭКИПИРОВКА][FromEquip][{this.name}] Добавляем в инвентарную сетку.");
                        targetGridPage.AddItemToInventoryGrid(this); // Добавляем ItemVisual в сетку
                        targetGridPage.RegisterVisual(this, new ItemGridData(_itemDefinition, _itemDefinition.GridPosition)); // Регистрируем визуальный элемент
                        this.SetPosition(new Vector2(_placementResults.SuggestedGridPosition.x * targetGridPage.CellSize.x, _placementResults.SuggestedGridPosition.y * targetGridPage.CellSize.y));
                        _isDragging = false; // Сбрасываем флаг перетаскивания
                        style.opacity = 1f; // Возвращаем полную непрозрачность
                    }
                    else
                    {
                        // Если не удалось добавить в инвентарь, возвращаем его обратно в слот экипировки
                        sourceEquipSlot.Equip(this);
                        //Debug.LogWarning($"[ЭКИПИРОВКА][FromEquip] Не удалось поместить предмет '{_itemDefinition.name}' в инвентарь '{targetContainer.name}'. Возвращен в слот экипировки '{sourceEquipSlot.Cell.name}'.");
                    }

                    targetGridPage.FinalizeDrag(); // FinalizeDrag для целевого инвентаря
                    _itemsPage.FinalizeDragOfItem(this); // Общая финализация

                    _isDragging = false; // Явно сбрасываем флаг перетаскивания
                    style.opacity = 1f; // Возвращаем полную непрозрачность
                    return false; // Возвращаем false, чтобы OnMouseUp также сбросил _isDragging

                }
                // Этот else блок теперь не должен достигаться в случае успешного перемещения из экипировки в инвентарь
                // Если сюда попадаем, значит, логика выше не сработала или что-то пошло не так
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
                //Debug.Log($"[ItemVisual][OnMouseDown][{this.name}] Событие MouseDown. GiveItem: {(_itemsPage.GiveItem != null ? _itemsPage.GiveItem.name : "NULL")}. CurrentDraggedItem: {(ItemsPage.CurrentDraggedItem != null ? ItemsPage.CurrentDraggedItem.name : "NULL")}. Этот ItemVisual: {this.name}.");

                if (_itemsPage.GiveItem != null)
                {
                    ServiceProvider.Get<InventoryAPI>().RiseItemGiveEvent(_itemDefinition);
                }
                else
                {
                    if (ItemsPage.CurrentDraggedItem != this)
                    {
                        //Debug.Log($"[ItemVisual][OnMouseDown][{this.name}] ItemsPage.CurrentDraggedItem НЕ равен этому ItemVisual. Вызываем PickUp.");
                        PickUp();
                        SetDraggedItemPosition(mouseEvent.mousePosition, mouseEvent.localMousePosition);
                    }
                    else
                    {
                        //Debug.Log($"[ItemVisual][OnMouseDown][{this.name}] ItemsPage.CurrentDraggedItem РАВЕН этому ItemVisual. НЕ вызываем PickUp (уже перетаскивается).");
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

        public void PickUp(bool isSwap = false)
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

            _itemsPage.StopTooltipDelayAndHideTooltip();
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
    }
}