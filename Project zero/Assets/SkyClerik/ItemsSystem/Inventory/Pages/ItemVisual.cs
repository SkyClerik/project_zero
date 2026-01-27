using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;

namespace Gameplay.Inventory
{
    public class ItemVisual : VisualElement
    {
        private ItemsPage _characterPages;
        private IDropTarget _ownerInventory;
        private ItemBaseDefinition _itemDefinition;
        private Vector2 _originalPosition;
        private Vector2Int _originalScale;
        private float _originalRotate;
        private bool _isDragging;
        private Rect _rect;
        private PlacementResults _placementResults;
        private VisualElement _icon;
        private Label _pcsText;

        private const string _iconName = "Icon";
        private const int IconPadding = 5;
        //private const string _visualIconContainerName = "visual-icon-container";
        //private const string _visualIconName = "visual-icon";

        public ItemBaseDefinition ItemDefinition => _itemDefinition;

        public ItemVisual(ItemsPage itemsPage, IDropTarget ownerInventory, ItemBaseDefinition itemDefinition, Rect rect)
        {
            _characterPages = itemsPage;
            _ownerInventory = ownerInventory;
            _itemDefinition = itemDefinition;
            _rect = rect;

            name = _itemDefinition.DefinitionName;
            // Стили самого контейнера (ItemVisual)
            style.position = Position.Absolute;
            this.SetPadding(IconPadding);

            // Сбрасываем состояние размеров и поворота при создании
            _itemDefinition.Dimensions.CurrentAngle = _itemDefinition.Dimensions.DefaultAngle;
            _itemDefinition.Dimensions.CurrentWidth = _itemDefinition.Dimensions.DefaultWidth;
            _itemDefinition.Dimensions.CurrentHeight = _itemDefinition.Dimensions.DefaultHeight;

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
            
            // Теперь, когда _icon создан, мы можем вызывать SetSize
            SetSize();

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

            Add(_icon);

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
            // Контейнер 'this' всегда имеет РЕЗУЛЬТИРУЮЩИЙ размер (с учетом поворота)
            this.style.width = _itemDefinition.Dimensions.CurrentWidth * _rect.width;
            this.style.height = _itemDefinition.Dimensions.CurrentHeight * _rect.height;

            // Обновляем позицию и размер иконки, чтобы она была по центру
            UpdateIconLayout();
        }

        /// <summary>
        /// Устанавливает размер иконки в ее исходное состояние и центрирует ее внутри родителя.
        /// </summary>
        private void UpdateIconLayout()
        {
            // Размеры родительского контейнера
            var parentWidth = this.style.width.value.value;
            var parentHeight = this.style.height.value.value;

            // Исходные размеры самой иконки
            var iconWidth = _itemDefinition.Dimensions.DefaultWidth * _rect.width;
            var iconHeight = _itemDefinition.Dimensions.DefaultHeight * _rect.height;

            _icon.style.width = iconWidth;
            _icon.style.height = iconHeight;

            // Ручной расчет для идеального центрирования
            _icon.style.left = (parentWidth - iconWidth) / 2;
            _icon.style.top = (parentHeight - iconHeight) / 2;
        }

        private void RotateIconRight()
        {
            var angle = _itemDefinition.Dimensions.CurrentAngle + 90;

            if (angle >= 360)
                angle = 0;

            RotateIcon(angle);
            SaveCurrentAngle(angle);
        }

        private void RotateIcon(float angle) => _icon.style.rotate = new Rotate(angle);

        private void SaveCurrentAngle(float angle) => _itemDefinition.Dimensions.CurrentAngle = angle;

        private void RestoreSizeAndRotate()
        {
            // Восстанавливаем данные модели из сохраненных значений
            _itemDefinition.Dimensions.CurrentAngle = _originalRotate;
            _itemDefinition.Dimensions.CurrentWidth = _originalScale.x;
            _itemDefinition.Dimensions.CurrentHeight = _originalScale.y;

            // Обновляем визуальное представление из теперь уже корректной модели данных
            SetSize();
            RotateIcon(_itemDefinition.Dimensions.CurrentAngle);
        }

        private void OnMouseUp(MouseUpEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (!_isDragging)
                    return;

                _isDragging = false;
                style.opacity = 1f;
                ItemsPage.CurrentDraggedItem = null;
                _placementResults = _characterPages.HandleItemPlacement(this);

                switch (_placementResults.Conflict)
                {
                    case ReasonConflict.None:
                        Placement();
                        break;
                    case ReasonConflict.beyondTheGridBoundary:
                        TryDropBack();
                        break;
                    case ReasonConflict.intersectsObjects:
                        TryDropBack();
                        break;
                    case ReasonConflict.invalidSlotType:
                        TryDropBack();
                        return;
                    default:
                        break;
                }

                _characterPages.FinalizeDragOfItem(this);
            }
        }

        private void Placement()
        {
            _characterPages.TransferItemBetweenContainers(this, _ownerInventory, _placementResults.TargetInventory, _placementResults.Position);
        }

        public void UpdatePcs()
        {
            _pcsText.text = $"{_itemDefinition.Stack}";
            //MarkDirtyRepaint();
        }

        private void TryDropBack()
        {
            _ownerInventory.AddStoredItem(this);
            _ownerInventory.AddItemToInventoryGrid(this);
            SetPosition(_originalPosition);
            RestoreSizeAndRotate();
        }

        private void OnMouseDown(MouseDownEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (ItemsPage.CurrentDraggedItem != this)
                    PickUp();
            }
        }

        public void PickUp()
        {
            // Сразу вызываем проверку потому что обновление происходит только при движении курсора а нам нужно найти место начальное
            _placementResults = _characterPages.HandleItemPlacement(this);

            _isDragging = true;
            style.left = float.MinValue;
            style.opacity = 0.7f;

            _originalPosition = worldBound.position - parent.worldBound.position;

            _originalRotate = _itemDefinition.Dimensions.CurrentAngle;
            _originalScale = new Vector2Int(_itemDefinition.Dimensions.CurrentWidth, _itemDefinition.Dimensions.CurrentHeight);

            this.style.position = Position.Absolute;
            _ownerInventory.GetDocument.rootVisualElement.Add(this);
            ItemsPage.CurrentDraggedItem = this;

            _ownerInventory.PickUp(this);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_isDragging)
                return;

            if (Input.GetMouseButtonDown(1))
                Rotate();

            _placementResults = _characterPages.HandleItemPlacement(this);
            if (_placementResults.Conflict == ReasonConflict.beyondTheGridBoundary)
            { }
            else
            { }
        }

        private void OnOverlapItem(IDropTarget target)
        {
            //if (target is EquipmentPage equipment)
            //{
            //    // Если слот занят, выталкиваем старый предмет и делаем его перетаскиваемым
            //    if (_placementResults.OverlapItem != null)
            //    {
            //        if (_placementResults.OverlapItem.ItemVisual == null)
            //        {
            //            // Не понятно как но объект есть а данных нет
            //        }
            //        else
            //        {
            //            // Поднимаем старый предмет в руку
            //            _placementResults.OverlapItem.PickUp();
            //        }
            //    }
            //    // Размещаем текущий перетаскиваемый предмет в слот
            //    target.Drop(this, _placementResults.Position);
            //}
            if (target is InventoryPageElement inventory)
            {
                // Логика для обычного инвентаря (стэки и т.д.)
                if (_placementResults.OverlapItem?.ItemDefinition.ID == _itemDefinition.ID)
                {
                    if (_itemDefinition.Stackable)
                    {
                        // Stack
                        _placementResults.OverlapItem.ItemDefinition.AddStack(_itemDefinition.Stack, out int remainder);
                        _placementResults.OverlapItem.UpdatePcs();
                        _itemDefinition.Stack = remainder;
                        UpdatePcs();

                        if (remainder == 0)
                        {
                            this.RemoveFromHierarchy();
                            ItemsPage.CurrentDraggedItem = null;
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