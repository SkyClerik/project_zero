using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;

namespace Gameplay.Inventory
{
    public class ItemVisual : VisualElement
    {
        private IDropTarget _ownerInventory;
        private ItemBaseDefinition _itemDefinition;
        private Vector2 _originalPosition;
        private (int, int) _originalScale;
        private float _originalRotate;
        private bool _isDragging;
        private Rect _rect;
        private PlacementResults _placementResults;
        private VisualElement _intermediate;
        private VisualElement _icon;
        private Label _pcsText;

        private const string _intermediateName = "Intermediate";
        private const string _iconName = "Icon";
        private const string _visualIconContainerName = "visual-icon-container";
        private const string _visualIconName = "visual-icon";

        public ItemBaseDefinition ItemDefinition => _itemDefinition;

        public ItemVisual(IDropTarget ownerInventory, ItemBaseDefinition itemDefinition, Rect rect)
        {
            _ownerInventory = ownerInventory;
            _itemDefinition = itemDefinition;
            _rect = rect;

            name = _itemDefinition.DefinitionName;
            //style.visibility = Visibility.Hidden;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.position = Position.Absolute;
            style.width = _itemDefinition.Dimensions.DefaultWidth * rect.width;
            style.height = _itemDefinition.Dimensions.DefaultHeight * rect.height;
            //AddToClassList(_visualIconContainerName);
            //SetSize();

            _intermediate = new VisualElement
            {
                style =
                {
                    width = _itemDefinition.Dimensions.DefaultWidth * rect.width,
                    height = _itemDefinition.Dimensions.DefaultHeight * rect.height,
                    rotate = new Rotate(_itemDefinition.Dimensions.DefaultAngle),
                },
                name = _intermediateName
            };
            _intermediate.SetPadding(5);

            _itemDefinition.Dimensions.CurrentAngle = _itemDefinition.Dimensions.DefaultAngle;
            _itemDefinition.Dimensions.CurrentWidth = _itemDefinition.Dimensions.DefaultWidth;
            _itemDefinition.Dimensions.CurrentHeight = _itemDefinition.Dimensions.DefaultHeight;

            if (_itemDefinition.Icon == null)
                Debug.LogWarning($"Тут иконка не назначена в предмет {_itemDefinition.name}");

            _icon = new VisualElement
            {
                style =
                {
                    backgroundImage = new StyleBackground(_itemDefinition.Icon),
                    width = _itemDefinition.Dimensions.DefaultWidth * rect.width,
                    height = _itemDefinition.Dimensions.DefaultHeight * rect.height,
                },
                name = _iconName,
            };

            //_icon.AddToClassList(_visualIconName);
            if (_itemDefinition.Stackable)
            {
                _pcsText = new Label
                {
                    style =
                    {
                         width = _itemDefinition.Dimensions.DefaultWidth * rect.width,
                        height = _itemDefinition.Dimensions.DefaultHeight * rect.height,
                        fontSize = 20,
                        color = new StyleColor(Color.red),
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

            Add(_intermediate);
            _intermediate.Add(_icon);

            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        ~ItemVisual()
        {
            UnregisterCallback<MouseUpEvent>(OnMouseUp);
            UnregisterCallback<MouseDownEvent>(OnMouseDown);
            UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        public void SetPosition(Vector2 pos)
        {
            style.top = pos.y;
            style.left = pos.x;
        }

        private void Rotate()
        {
            SwapOwnerSize();
            SetSize();
            RotateIconRight();
        }

        private void SwapOwnerSize()
        {
            _itemDefinition.Dimensions.Swap();
        }

        private void SetSize()
        {
            var width = _itemDefinition.Dimensions.CurrentWidth * _rect.width;
            var height = _itemDefinition.Dimensions.CurrentHeight * _rect.height;

            this.style.width = width;
            this.style.height = height;
            _intermediate.style.width = width;
            _intermediate.style.height = height;
            _icon.style.width = width;
            _icon.style.height = height;
        }

        private void RotateIconRight()
        {
            var angle = _itemDefinition.Dimensions.CurrentAngle + 90;

            if (angle >= 360)
                angle = 0;

            RotateIntermediate(angle);
            SaveCurrentAngle(angle);
        }

        private void RotateIntermediate(float angle) => _intermediate.style.rotate = new Rotate(angle);

        private void SaveCurrentAngle(float angle) => _itemDefinition.Dimensions.CurrentAngle = angle;

        private void RestoreSizeAndRotate()
        {
            _itemDefinition.Dimensions.CurrentAngle = _itemDefinition.Dimensions.DefaultAngle;
            _itemDefinition.Dimensions.CurrentWidth = _itemDefinition.Dimensions.DefaultWidth;
            _itemDefinition.Dimensions.CurrentHeight = _itemDefinition.Dimensions.DefaultHeight;

            SetSize();
            RotateIntermediate(_originalRotate);
            SaveCurrentAngle(_originalRotate);
        }

        private void OnMouseUp(MouseUpEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (!_isDragging)
                    return;

                _isDragging = false;
                style.opacity = 1f;
                CharacterPages.CurrentDraggedItem = null;
                //_ownerInventory.GetDocument.sortingOrder = 0;

                _placementResults = _ownerInventory.ShowPlacementTarget(this);

                switch (_placementResults.Conflict)
                {
                    case ReasonConflict.None:
                        // Конфликтов нет, можно разместить
                        Debug.Log($"[OnMouseUp] No conflict. Dropping item at new position.");
                        _ownerInventory.Drop(this, _placementResults.Position);
                        SetPosition(_placementResults.Position - parent.worldBound.position);
                        break;

                    case ReasonConflict.beyondTheGridBoundary:
                        Debug.Log($"[OnMouseUp] Conflict: Beyond the grid boundary. Dropping back.");
                        TryDropBack();
                        break;

                    case ReasonConflict.intersectsObjects:
                        Debug.Log($"[OnMouseUp] Conflict: Intersects with another object. Dropping back.");
                        TryDropBack();
                        break;

                    case ReasonConflict.invalidSlotType:
                        Debug.Log($"[OnMouseUp] Conflict: Invalid slot type. Dropping back.");
                        TryDropBack();
                        return;
                }

                _ownerInventory.FinalizeDrag();
            }
        }

        public void UpdatePcs()
        {
            _pcsText.text = $"{_itemDefinition.Stack}";
            //MarkDirtyRepaint();
        }

        private void TryDropBack()
        {
            Debug.Log($"Возврат предмета");
            _ownerInventory.AddStoredItem(this);
            _ownerInventory.AddItemToInventoryGrid(this);
            SetPosition(_originalPosition);
            RestoreSizeAndRotate();
        }

        private void OnMouseDown(MouseDownEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (CharacterPages.CurrentDraggedItem != this)
                    PickUp();
            }
        }

        public void PickUp()
        {
            // Сразу вызываем проверку потому что обновление происходит только при движении курсора а нам нужно найти место начальное
            _ownerInventory.ShowPlacementTarget(this);

            Debug.Log($"Визуальный элемент: Поднят предмет");
            _isDragging = true;
            style.left = float.MinValue;
            style.opacity = 0.7f;

            _originalPosition = worldBound.position - parent.worldBound.position;

            _originalRotate = _itemDefinition.Dimensions.CurrentAngle;
            _originalScale = (_itemDefinition.Dimensions.CurrentWidth, _itemDefinition.Dimensions.CurrentHeight);

            this.style.position = Position.Absolute;
            _ownerInventory.GetDocument.rootVisualElement.Add(this);
            CharacterPages.CurrentDraggedItem = this;

            _ownerInventory.PickUp(this);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_isDragging)
                return;

            if (Input.GetMouseButtonDown(1))
                Rotate();

            _placementResults = _ownerInventory.ShowPlacementTarget(this);
            if (_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary)
                Debug.Log($"За пределами всех сеток");
            else
                Debug.Log($"Внутри какой то сетки");
        }

        private void OnOverlapItem(IDropTarget target)
        {
            //if (target is EquipmentPage equipment)
            //{
            //    Debug.Log($"target {equipment}");
            //    // Если слот занят, выталкиваем старый предмет и делаем его перетаскиваемым
            //    if (_placementResults.OverlapItem != null)
            //    {
            //        if (_placementResults.OverlapItem.ItemVisual == null)
            //        {
            //            Debug.Log($"Не понятно как но объект есть а данных нет");
            //        }
            //        else
            //        {
            //            Debug.Log($"Поднимаем старый предмет в руку");
            //            _placementResults.OverlapItem.ItemVisual.PickUp();
            //        }
            //    }
            //    // Размещаем текущий перетаскиваемый предмет в слот
            //    target.Drop(this, _placementResults.Position);
            //}
            if (target is InventoryPage inventory)
            {
                Debug.Log($"target {inventory}");
                // Логика для обычного инвентаря (стэки и т.д.)
                if (_placementResults.OverlapItem?.ItemDefinition.ID == _itemDefinition.ID)
                {
                    if (_itemDefinition.Stackable)
                    {
                        Debug.Log($"Stack");
                        _placementResults.OverlapItem.ItemDefinition.AddStack(_itemDefinition.Stack, out int remainder);
                        _placementResults.OverlapItem.UpdatePcs();
                        _itemDefinition.Stack = remainder;
                        UpdatePcs();

                        if (remainder == 0)
                        {
                            this.RemoveFromHierarchy();
                            CharacterPages.CurrentDraggedItem = null;
                        }
                    }
                }
                else
                {
                    // Логика для инвентаря, если предметы не совпадают
                    //Drop(target);
                    //_placementResults.OverlapItem.RootVisual.PickUp();
                }
            }
        }

        public void SetOwnerInventory(IDropTarget dropTarget)
        {
            _ownerInventory = dropTarget;
        }
    }
}