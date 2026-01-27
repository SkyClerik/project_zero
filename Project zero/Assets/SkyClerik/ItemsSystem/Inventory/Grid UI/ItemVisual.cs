using UnityEngine.Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Inventory
{
    public class ItemVisual : VisualElement
    {
        private IDropTarget _ownerInventory;
        private StoredItem _ownerStored;
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

        public StoredItem StoredItem => _ownerStored;

        public ItemVisual(IDropTarget ownerInventory, StoredItem ownerStored, Rect rect)
        {
            _ownerInventory = ownerInventory;
            _ownerStored = ownerStored;
            _rect = rect;

            name = _ownerStored.ItemDefinition.DefinitionName;
            //style.visibility = Visibility.Hidden;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.position = Position.Absolute;
            style.width = _ownerStored.ItemDefinition.Dimensions.DefaultWidth * rect.width;
            style.height = _ownerStored.ItemDefinition.Dimensions.DefaultHeight * rect.height;
            //AddToClassList(_visualIconContainerName);
            //SetSize();

            _intermediate = new VisualElement
            {
                style =
                {
                    width = _ownerStored.ItemDefinition.Dimensions.DefaultWidth * rect.width,
                    height = _ownerStored.ItemDefinition.Dimensions.DefaultHeight * rect.height,
                    rotate = new Rotate(_ownerStored.ItemDefinition.Dimensions.DefaultAngle),
                },
                name = _intermediateName
            };
            _intermediate.SetPadding(5);

            _ownerStored.ItemDefinition.Dimensions.CurrentAngle = _ownerStored.ItemDefinition.Dimensions.DefaultAngle;

            if (_ownerStored.ItemDefinition.Icon == null)
                Debug.LogWarning($"Тут иконка не назначена в предмет {_ownerStored.ItemDefinition.name}");

            _icon = new VisualElement
            {
                style =
                {
                    backgroundImage = new StyleBackground(_ownerStored.ItemDefinition.Icon),
                    width = _ownerStored.ItemDefinition.Dimensions.DefaultWidth * rect.width,
                    height = _ownerStored.ItemDefinition.Dimensions.DefaultHeight * rect.height,
                },
                name = _iconName,
            };

            //_icon.AddToClassList(_visualIconName);
            if (_ownerStored.ItemDefinition.Stackable)
            {
                _pcsText = new Label
                {
                    style =
                    {
                         width = _ownerStored.ItemDefinition.Dimensions.DefaultWidth * rect.width,
                        height = _ownerStored.ItemDefinition.Dimensions.DefaultHeight * rect.height,
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
            SetSize(this);
            RotateIconRight();
        }

        private void SwapOwnerSize()
        {
            _ownerStored.ItemDefinition.Dimensions.Swap();
        }

        private void SetSize(VisualElement visualElement)
        {
            visualElement.style.height = _ownerStored.ItemDefinition.Dimensions.CurrentHeight * _rect.height;
            visualElement.style.width = _ownerStored.ItemDefinition.Dimensions.CurrentWidth * _rect.width;
        }

        private void RotateIconRight()
        {
            var angle = _ownerStored.ItemDefinition.Dimensions.CurrentAngle + 90;

            if (angle >= 360)
                angle = 0;

            RotateIntermediate(angle);
            SaveCurrentAngle(angle);
        }

        private void RotateIntermediate(float angle) => _intermediate.style.rotate = new Rotate(angle);

        private void SaveCurrentAngle(float angle) => _ownerStored.ItemDefinition.Dimensions.CurrentAngle = angle;

        private void RestoreSizeAndRotate()
        {
            _ownerStored.ItemDefinition.Dimensions.CurrentWidth = _originalScale.Item1;
            _ownerStored.ItemDefinition.Dimensions.CurrentHeight = _originalScale.Item2;
            SetSize(this);
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

                        if (_placementResults.OverlapItem != null)
                        {
                            if (_placementResults.OverlapItem == _ownerStored)
                            {
                                Debug.Log($"Нашли самого себя! Такое по идее не возможно!!!");
                                TryDropBack();
                            }
                            else
                            {
                                Debug.Log($"Конфликт с одним предметом - {_placementResults} в {_ownerInventory}");
                                OnOverlapItem(_ownerInventory);
                            }
                        }
                        else
                        {
                            Debug.Log($"Нет конфликтов можно расположить в {_ownerInventory}");
                            _ownerInventory.Drop(_ownerStored, _placementResults.Position);
                            SetPosition(_placementResults.Position - parent.worldBound.position);
                        }

                        return;

                    case ReasonConflict.beyondTheGridBoundary:

                        //if (_characterPage.Inventories.Count == _howManyInventoryChecked)
                        //{
                        Debug.Log($"Получается вот тут все проверил и предмет за пределами всех сеток");
                        TryDropBack();
                        //    return;
                        //}

                        //continue;
                        break;

                    case ReasonConflict.intersectsObjects:
                        Debug.Log($"Пересекает несколько объектов");
                        TryDropBack();
                        break;

                    case ReasonConflict.invalidSlotType:
                        Debug.Log($"Не подходит тип - {_placementResults} в {_ownerInventory}");
                        TryDropBack();
                        return;
                }

            }
        }

        public void UpdatePcs()
        {
            _pcsText.text = $"{_ownerStored.ItemDefinition.Stack}";
            //MarkDirtyRepaint();
        }

        private void TryDropBack()
        {
            Debug.Log($"DropBack");
            _placementResults = _ownerInventory.ShowPlacementTarget(this);

            if (_placementResults.Conflict == ReasonConflict.None)
            {
                _ownerInventory.AddStoredItem(_ownerStored);

                //TODO надо перенести в контракт и назвать DropBack или просто вызвать Drop
                _ownerInventory.AddItemToInventoryGrid(this);
                SetPosition(_originalPosition);
                RestoreSizeAndRotate();
            }
            else
            {
                PickUp();
            }
        }

        private void OnMouseDown(MouseDownEvent mouseEvent)
        {
            if (mouseEvent.button == 0)
            {
                if (CharacterPages.CurrentDraggedItem != _ownerStored)
                    PickUp();
            }
        }

        public void PickUp()
        {
            // Сразу вызываем проверку потому что обновление происходит только при движении курсора а нам нужно найти место начальное
            _ownerInventory.ShowPlacementTarget(this);

            Debug.Log($"ItemVisual PickUp Item");
            _isDragging = true;
            style.left = float.MinValue;
            style.opacity = 0.7f;

            _originalPosition = worldBound.position - parent.worldBound.position;
            _originalRotate = _ownerStored.ItemDefinition.Dimensions.CurrentAngle;
            _originalScale = (_ownerStored.ItemDefinition.Dimensions.CurrentWidth, _ownerStored.ItemDefinition.Dimensions.CurrentHeight);

            this.style.position = Position.Absolute;
            _ownerInventory.GetDocument.rootVisualElement.Add(this);
            CharacterPages.CurrentDraggedItem = _ownerStored;

            _ownerInventory.PickUp(_ownerStored);
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
            //    target.Drop(_ownerStored, _placementResults.Position);
            //}
            if (target is InventoryPage inventory)
            {
                Debug.Log($"target {inventory}");
                // Логика для обычного инвентаря (стэки и т.д.)
                if (_placementResults.OverlapItem?.ItemDefinition.ID == _ownerStored.ItemDefinition.ID)
                {
                    if (_ownerStored.ItemDefinition.Stackable)
                    {
                        Debug.Log($"Stack");
                        _placementResults.OverlapItem.ItemDefinition.AddStack(_ownerStored.ItemDefinition.Stack, out int remainder);
                        _placementResults.OverlapItem.ItemVisual.UpdatePcs();
                        _ownerStored.ItemDefinition.Stack = remainder;
                        UpdatePcs();

                        if (remainder == 0)
                        {
                            _ownerStored.ItemVisual.RemoveFromHierarchy();
                            _ownerStored.ItemVisual = null;
                            _ownerStored = null;
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