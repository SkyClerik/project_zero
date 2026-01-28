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
        private bool _hasNoHome = false;
        private Rect _rect;
        private PlacementResults _placementResults;
        private VisualElement _icon;
        private Label _pcsText;

        private bool _singleRotationMode;

        private const string _iconName = "Icon";
        private const int IconPadding = 5;

        public ItemBaseDefinition ItemDefinition => _itemDefinition;

        public ItemVisual(ItemsPage itemsPage, IDropTarget ownerInventory, ItemBaseDefinition itemDefinition, Rect rect, bool singleRotationMode = true)
        {
            _characterPages = itemsPage;
            _ownerInventory = ownerInventory;
            _itemDefinition = itemDefinition;
            _rect = rect;
            _singleRotationMode = singleRotationMode;
            name = _itemDefinition.DefinitionName;
            style.position = Position.Absolute;
            this.SetPadding(IconPadding);

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
            float angle;
            if (_singleRotationMode)
            {
                // В режиме одной ротации переключаемся между 0 и 90 градусами
                angle = (_itemDefinition.Dimensions.CurrentAngle == 0) ? 90 : 0;
            }
            else
            {
                // В обычном режиме делаем полный оборот
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
                // Временно обнуляем, чтобы избежать рекурсивных проверок
                ItemsPage.CurrentDraggedItem = null;
                _placementResults = _characterPages.HandleItemPlacement(this);

                switch (_placementResults.Conflict)
                {
                    case ReasonConflict.None:
                        Placement();
                        break;

                    case ReasonConflict.SwapAvailable:
                        // Выполняем обмен
                        var itemToSwap = _placementResults.OverlapItem;
                        // Кладем текущий предмет
                        Placement();
                        // Поднимаем старый как "бездомный"
                        itemToSwap.PickUp(isSwap: true);
                        break;

                    case ReasonConflict.beyondTheGridBoundary:
                    case ReasonConflict.intersectsObjects:
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
        }

        private void TryDropBack()
        {
            if (_hasNoHome)
            {
                PickUp(isSwap: true);
                return;
            }

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

        public void PickUp(bool isSwap = false)
        {
            _hasNoHome = isSwap;

            // Сразу вызываем проверку потому что обновление происходит только при движении курсора а нам нужно найти место начальное
            _placementResults = _characterPages.HandleItemPlacement(this);

            _isDragging = true;
            style.left = float.MinValue;
            style.opacity = 0.7f;

            if (!_hasNoHome)
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

        public void SetOwnerInventory(IDropTarget dropTarget)
        {
            _ownerInventory = dropTarget;
        }
    }
}