using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
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
        private PlacementResults _placementResults;
        private VisualElement _icon;
        private Label _pcsText;

        private bool _singleRotationMode;

        private const string _iconName = "Icon";
        private const int IconPadding = 5;

        public ItemBaseDefinition ItemDefinition => _itemDefinition;

        public ItemVisual(ItemsPage itemsPage, IDropTarget ownerInventory, ItemBaseDefinition itemDefinition, Vector2Int gridPosition, Vector2Int gridSize, bool singleRotationMode = true)
        {
            _characterPages = itemsPage;
            _ownerInventory = ownerInventory;
            _itemDefinition = itemDefinition;
            _singleRotationMode = singleRotationMode;
            name = _itemDefinition.DefinitionName;
            style.position = Position.Absolute;
            this.SetPadding(IconPadding);

            _itemDefinition.Dimensions.CurrentAngle = _itemDefinition.Dimensions.DefaultAngle;
            _itemDefinition.Dimensions.CurrentWidth = itemDefinition.Dimensions.DefaultWidth; // Теперь CurrentWidth и CurrentHeight должны быть размером в ячейках
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

            // SetSize() теперь будет вызвана позже, после того как ItemVisual будет добавлен в иерархию и CellSize будет доступен через _ownerInventory
            // Или мы можем сразу вызвать SetSize, так как _ownerInventory.CellSize уже доступен
            SetSize();

            if (_itemDefinition.Stackable && _itemDefinition.ViewStackable)
            {
                _pcsText = new Label
                {
                    style =
                    {
                         // Ширина и высота Label теперь должны рассчитываться от размера элемента, а не _rect
                         width = _itemDefinition.Dimensions.DefaultWidth * _ownerInventory.CellSize.x,
                        height = _itemDefinition.Dimensions.DefaultHeight * _ownerInventory.CellSize.y,
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
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        ~ItemVisual()
        {
            UnregisterCallback<MouseUpEvent>(OnMouseUp);
            UnregisterCallback<MouseDownEvent>(OnMouseDown);
            UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            UnregisterCallback<MouseEnterEvent>(OnMouseEnter); // Отмена регистрации
            UnregisterCallback<MouseLeaveEvent>(OnMouseLeave); // Отмена регистрации
        }

        public void SetPosition(Vector2 pos)
        {
            style.top = pos.y;
            style.left = pos.x;
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (_isDragging) // Не показываем тултип, если предмет перетаскивается
                return;

            _characterPages.StartTooltipDelay(this);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _characterPages.StopTooltipDelayAndHideTooltip();
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
            this.style.width = _itemDefinition.Dimensions.CurrentWidth * _ownerInventory.CellSize.x;
            this.style.height = _itemDefinition.Dimensions.CurrentHeight * _ownerInventory.CellSize.y;

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
            var iconWidth = _itemDefinition.Dimensions.DefaultWidth * _ownerInventory.CellSize.x;
            var iconHeight = _itemDefinition.Dimensions.DefaultHeight * _ownerInventory.CellSize.y;

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
                Debug.Log($"[ItemVisual.OnMouseUp] After HandleItemPlacement. Conflict: {_placementResults.Conflict}, SuggestedGridPosition: {_placementResults.SuggestedGridPosition}, OverlapItem: {(_placementResults.OverlapItem != null ? _placementResults.OverlapItem.name : "None")}");

                // Для размещения нам нужна gridPosition
                Vector2Int targetGridPosition = new Vector2Int(
                    Mathf.RoundToInt(_placementResults.Position.x / _ownerInventory.CellSize.x),
                    Mathf.RoundToInt(_placementResults.Position.y / _ownerInventory.CellSize.y)
                );
                Debug.Log($"[ItemVisual.OnMouseUp] Calculated targetGridPosition: {targetGridPosition}");

                switch (_placementResults.Conflict)
                {
                    case ReasonConflict.None:
                        Debug.Log("[ItemVisual.OnMouseUp] Conflict: None. Performing placement.");
                        Placement(targetGridPosition);
                        break;

                    case ReasonConflict.SwapAvailable:
                        Debug.Log("[ItemVisual.OnMouseUp] Conflict: SwapAvailable. Performing swap.");
                        // Выполняем обмен
                        var itemToSwap = _placementResults.OverlapItem;
                        // Кладем текущий предмет
                        Placement(targetGridPosition);
                        // Поднимаем старый как "бездомный"
                        itemToSwap.PickUp(isSwap: true);
                        break;

                    case ReasonConflict.beyondTheGridBoundary:
                        Debug.Log("[ItemVisual.OnMouseUp] Conflict: beyondTheGridBoundary. Trying to drop back.");
                        TryDropBack();
                        return;
                    case ReasonConflict.intersectsObjects:
                        Debug.Log("[ItemVisual.OnMouseUp] Conflict: intersectsObjects. Trying to drop back.");
                        TryDropBack();
                        return;
                    case ReasonConflict.invalidSlotType:
                        Debug.Log("[ItemVisual.OnMouseUp] Conflict: invalidSlotType. Trying to drop back.");
                        TryDropBack();
                        return;
                    default:
                        Debug.Log($"[ItemVisual.OnMouseUp] Conflict: {_placementResults.Conflict}. No specific handling, trying to drop back.");
                        TryDropBack();
                        return;
                }

                _characterPages.FinalizeDragOfItem(this);
            }
        }

        private void Placement(Vector2Int gridPosition)
        {
            Debug.Log($"[ItemVisual.Placement] Placing item {name} at gridPosition: {gridPosition}");
            _characterPages.TransferItemBetweenContainers(this, _ownerInventory, _placementResults.TargetInventory, gridPosition);
        }

        public void UpdatePcs()
        {
            if (_pcsText != null)
            {
                _pcsText.text = $"{_itemDefinition.Stack}";
            }
        }

        private void TryDropBack()
        {
            if (_hasNoHome)
            {
                PickUp(isSwap: true);
                return;
            }

            // Нужно получить gridPosition, куда предмет должен вернуться
            // Предполагаем, что _originalPosition - это пиксельные координаты
            Vector2Int originalGridPosition = new Vector2Int(
                Mathf.RoundToInt(_originalPosition.x / _ownerInventory.CellSize.x),
                Mathf.RoundToInt(_originalPosition.y / _ownerInventory.CellSize.y)
            );

            _ownerInventory.Drop(this, originalGridPosition);
            // AddItemToInventoryGrid(this) не нужен, так как AddStoredItem его вызывает
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
            _isDragging = true;
            _hasNoHome = isSwap;

            // Сразу вызываем проверку потому что обновление происходит только при движении курсора а нам нужно найти место начальное
            _placementResults = _characterPages.HandleItemPlacement(this);
            //style.left = float.MinValue; // Удаляем эту строку, она вызывает "прыжки"
            style.opacity = 0.7f;

            // Получаем текущую позицию мыши в мировых координатах
            Vector2 mouseScreenPosition = Input.mousePosition;
            // Преобразуем ее в локальные координаты для rootVisualElement (это самый верхний элемент в иерархии UI, к которому мы цепляем draggedItem)
            Vector2 mouseLocalPosition = _ownerInventory.GetDocument.rootVisualElement.WorldToLocal(mouseScreenPosition);

            // Устанавливаем позицию ItemVisual под курсором сразу
            this.style.left = mouseLocalPosition.x - (this.resolvedStyle.width / 2); // Центрируем по курсору
            this.style.top = mouseLocalPosition.y - (this.resolvedStyle.height / 2); // Центрируем по курсору

            if (!_hasNoHome)
            {
                // Получаем ItemGridData от владельца инвентаря
                ItemGridData currentGridData = _ownerInventory.GetItemGridData(this);
                if (currentGridData != null)
                {
                    _originalPosition = new Vector2(currentGridData.GridPosition.x * _ownerInventory.CellSize.x, currentGridData.GridPosition.y * _ownerInventory.CellSize.y);
                }
                else
                {
                    // Если ItemGridData не найдена, возможно, это новый предмет или ошибка
                    _originalPosition = Vector2.zero; // или установить какое-то дефолтное значение
                }
            }

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

            // Передаем обновленное состояние в HandleItemPlacement для проверки размещения и обновления телеграфа
            _placementResults = _characterPages.HandleItemPlacement(this);
        }

        public void SetOwnerInventory(IDropTarget dropTarget)
        {
            _ownerInventory = dropTarget;
        }
    }
}