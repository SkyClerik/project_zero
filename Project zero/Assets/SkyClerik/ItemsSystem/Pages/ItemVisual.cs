using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    public class ItemVisual : VisualElement
    {
        private ItemsPage _itemsPage;
        private IDropTarget _ownerInventory;
        private ItemBaseDefinition _itemDefinition;
        private Vector2 _originalPosition;
        private Vector2Int _originalScale;
        private float _originalRotate;
        private bool _isDragging;
        private bool _hasNoHome = false;
        private PlacementResults _placementResults;
        private VisualElement _icon;
        private Label _pcsText;

        private bool _singleRotationMode;

        private const string _iconName = "Icon";
        private const int IconPadding = 5;

        public ItemBaseDefinition ItemDefinition => _itemDefinition;

        public ItemVisual(ItemsPage itemsPage, IDropTarget ownerInventory, ItemBaseDefinition itemDefinition, Vector2Int gridPosition, Vector2Int gridSize, bool singleRotationMode = true)
        {
            _itemsPage = itemsPage;
            _ownerInventory = ownerInventory;
            _itemDefinition = itemDefinition;
            _singleRotationMode = singleRotationMode;
            name = _itemDefinition.DefinitionName;
            style.position = Position.Absolute;
            this.SetPadding(IconPadding);

            // Инициализация ItemBaseDefinition.Dimensions.Current...
            _itemDefinition.Dimensions.CurrentAngle = _itemDefinition.Dimensions.DefaultAngle;
            _itemDefinition.Dimensions.CurrentWidth = itemDefinition.Dimensions.DefaultWidth;
            _itemDefinition.Dimensions.CurrentHeight = itemDefinition.Dimensions.DefaultHeight;

            // Устанавливаем начальную позицию и размер на основе gridPosition, gridSize и CellSize владельца
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
                    rotate = new Rotate(_itemDefinition.Dimensions.CurrentAngle),
                    position = Position.Absolute,
                }
            };

            SetSize();

            if (_itemDefinition.Stackable && _itemDefinition.ViewStackable)
            {
                _pcsText = new Label
                {
                    style =
                    {
                        width = _itemDefinition.Dimensions.DefaultWidth * _ownerInventory.CellSize.x,
                        height = _itemDefinition.Dimensions.DefaultHeight * _ownerInventory.CellSize.y,
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

        public void SetPosition(Vector2 pos)
        {
            style.top = pos.y;
            style.left = pos.x;
        }

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

        public void Rotate()
        {
            if (_itemDefinition.Dimensions.DefaultWidth == _itemDefinition.Dimensions.DefaultHeight)
                return;

            _itemDefinition.Dimensions.Swap();
            SetSize();
            RotateIconRight();
        }

        private void SetSize()
        {
            this.style.width = _itemDefinition.Dimensions.CurrentWidth * _ownerInventory.CellSize.x;
            this.style.height = _itemDefinition.Dimensions.CurrentHeight * _ownerInventory.CellSize.y;

            UpdateIconLayout();
        }

        private void UpdateIconLayout()
        {
            var parentWidth = this.style.width.value.value;
            var parentHeight = this.style.height.value.value;

            var iconWidth = _itemDefinition.Dimensions.DefaultWidth * _ownerInventory.CellSize.x;
            var iconHeight = _itemDefinition.Dimensions.DefaultHeight * _ownerInventory.CellSize.y;

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
                angle = (_itemDefinition.Dimensions.CurrentAngle == 0) ? 90 : 0;
            }
            else
            {
                angle = _itemDefinition.Dimensions.CurrentAngle + 90;
                if (angle >= 360)
                    angle = 0;
            }

            RotateIcon(angle);
            SaveCurrentAngle(angle);
        }

        private void RotateIcon(float angle) => _icon.style.rotate = new Rotate(angle);

        private void SaveCurrentAngle(float angle) => _itemDefinition.Dimensions.CurrentAngle = angle;

        private void RestoreSizeAndRotate()
        {
            _itemDefinition.Dimensions.CurrentAngle = _originalRotate;
            _itemDefinition.Dimensions.CurrentWidth = _originalScale.x;
            _itemDefinition.Dimensions.CurrentHeight = _originalScale.y;

            SetSize();
            RotateIcon(_itemDefinition.Dimensions.CurrentAngle);
        }

        private void OnMouseUp(MouseUpEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (!_isDragging) 
                    return;

                _placementResults = _itemsPage.HandleItemPlacement(this);

                if (_placementResults.Conflict == ReasonConflict.StackAvailable)
                {
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
                    return;
                }

                _isDragging = false;
                style.opacity = 1f;
                ItemsPage.CurrentDraggedItem = null;

                switch (_placementResults.Conflict)
                {
                    case ReasonConflict.None:
                        Placement(_placementResults.SuggestedGridPosition);
                        break;
                    case ReasonConflict.SwapAvailable:
                        var itemToSwap = _placementResults.OverlapItem;
                        itemToSwap.PickUp(isSwap: true);
                        Placement(_placementResults.SuggestedGridPosition);
                        break;
                    default:
                        TryDropBack();
                        break;
                }

                _itemsPage.FinalizeDragOfItem(this);
            }
        }

        private void OnMouseDown(MouseDownEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (_itemsPage.GiveItem != null)
                {
                    _itemsPage.TriggerItemGiveEvent(_itemDefinition);
                }
                else
                {
                    if (ItemsPage.CurrentDraggedItem != this)
                    {
                        PickUp();
                    }
                }
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_isDragging)
                return;

            _placementResults = _itemsPage.HandleItemPlacement(this);
        }

        public void PickUp(bool isSwap = false)
        {
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

            _originalRotate = _itemDefinition.Dimensions.CurrentAngle;
            _originalScale = new Vector2Int(_itemDefinition.Dimensions.CurrentWidth, _itemDefinition.Dimensions.CurrentHeight);

            ItemsPage.CurrentDraggedItem = this;

            _ownerInventory.PickUp(this);

            _placementResults = _itemsPage.HandleItemPlacement(this);

            Vector2 mouseScreenPosition = Input.mousePosition;
            Vector2 mouseLocalPosition = _ownerInventory.GetDocument.rootVisualElement.WorldToLocal(mouseScreenPosition);

            this.style.left = mouseLocalPosition.x - (this.resolvedStyle.width / 2);
            this.style.top = mouseLocalPosition.y - (this.resolvedStyle.height / 2);
            this.style.position = Position.Absolute;
            _ownerInventory.GetDocument.rootVisualElement.Add(this);
        }

        public void SetOwnerInventory(IDropTarget dropTarget)
        {
            _ownerInventory = dropTarget;
        }

        private void Placement(Vector2Int gridPosition)
        {
            _itemsPage.TransferItemBetweenContainers(this, _ownerInventory, _placementResults.TargetInventory, gridPosition);
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

            Vector2Int originalGridPosition = new Vector2Int(
                Mathf.RoundToInt(_originalPosition.x / _ownerInventory.CellSize.x),
                Mathf.RoundToInt(_originalPosition.y / _ownerInventory.CellSize.y)
            );

            _ownerInventory.Drop(this, originalGridPosition);
            SetPosition(_originalPosition);
            RestoreSizeAndRotate();
        }
    }
}