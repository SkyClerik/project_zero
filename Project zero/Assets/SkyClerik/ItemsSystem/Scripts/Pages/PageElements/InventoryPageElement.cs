using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;
using System.Collections;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет элемент UI страницы инвентаря, управляющий отображением предметов
    /// и взаимодействием с пользователем. Наследует функциональность базовой страницы сетки.
    /// </summary>
    public class InventoryPageElement : GridPageElementBase
    {
        private const string _titleText = "Инвентарь";
        private VisualElement _itemImage;
        private const string _itemImageID = "item_image";
        private VisualElement _descriptionBackground;
        private const string _descriptionBackgroundID = "description_background";
        private Label _lDescription;
        private const string _lDescriptionID = "l_description";
        private Label _lPriceValue;
        private const string _lPriceValueID = "l_price_value";
        private Button _bClose;
        private const string _bCloseID = "b_close";
        private VisualElement _rotationAreaRoot;
        private const string _rotationAreaRootID = "rotation_area_root";
        private VisualElement _rotationArea;
        private const string _rotationAreaID = "rotation_area";
        private VisualElement _body;
        private const string _bodyID = "body";

        private ItemVisual _draggerItem;
        private Coroutine _overlapCheckCoroutine;
        private bool _rotateOneBox = false;
        private bool _rotateTwoBox = false;

        //Кеша
        private Rect _draggerRect;
        private Rect _rotationAreaRootRect;
        private Rect _rotationAreaRect;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InventoryPageElement"/>.
        /// </summary>
        /// <param name="itemsPage">Ссылка на главную страницу предметов.</param>
        /// <param name="document">UIDocument, содержащий корневой визуальный элемент.</param>
        /// <param name="itemContainer">Контейнер предметов, связанный с этой страницей инвентаря.</param>
        public InventoryPageElement(InventoryStorage itemsPage, UIDocument document, ItemContainer itemContainer)
            : base(itemsPage, document, itemContainer, itemContainer.RootPanelName)
        {
            _body = _root.Q(_bodyID);
            _descriptionBackground = _root.Q(_descriptionBackgroundID);
            _rotationAreaRoot = _root.Q(_rotationAreaRootID);
            _rotationArea = _rotationAreaRoot.Q(_rotationAreaID);
            _bClose = _root.Q<Button>(_bCloseID);
            _itemImage = _root.Q(_itemImageID);
            _lDescription = _root.Q<Label>(_lDescriptionID);
            _lPriceValue = _root.Q<Label>(_lPriceValueID);

            _bClose.clicked += CloseClicked;
            SetDisableRotator(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            _bClose.clicked -= CloseClicked;

            if (_overlapCheckCoroutine != null)
            {
                _itemsPage.StopCoroutine(_overlapCheckCoroutine);
                _overlapCheckCoroutine = null;
            }
        }

        private void SetDisableRotator(bool enable)
        {
            _rotationAreaRoot.SetDisplay(enable);
        }

        private void CheckRotationAreaOverlap()
        {
            if (_draggerItem == null || _rotationAreaRoot.resolvedStyle.display == DisplayStyle.None)
                return;

            _draggerRect = new Rect(_itemsPage.MouseUILocalPosition.x, _itemsPage.MouseUILocalPosition.y, 10, 10);
            _rotationAreaRootRect = _rotationAreaRoot.worldBound;
            _rotationAreaRect = _rotationArea.worldBound;

            if (_rotationAreaRootRect.Overlaps(_draggerRect))
            {
                if (_rotationAreaRect.Overlaps(_draggerRect))
                {
                    _rotateTwoBox = false;
                    if (_rotateOneBox && _rotateTwoBox == false)
                    {
                        _rotateOneBox = false;
                        _rotateTwoBox = true;
                        _draggerItem.Rotate();
                    }
                }
                else
                {
                    _rotateOneBox = true;
                }
            }
        }

        private void CloseClicked()
        {
            if (InventoryStorage.CurrentDraggedItem == null)
            {
                _itemsPage.CloseAll();
            }
        }

        public void SetItemDescription(ItemBaseDefinition itemBaseDefinition)
        {
            _itemImage.SetBackgroundImage(itemBaseDefinition.Icon);
            _lDescription.text = itemBaseDefinition.Description;
            _lPriceValue.text = $"{itemBaseDefinition.Price}";
            _descriptionBackground.SetVisibility(true);
            _itemImage.SetVisibility(true);
        }

        public void DisableItemDescription()
        {
            _itemImage.SetVisibility(false);
            _descriptionBackground.SetVisibility(false);
        }

        private IEnumerator OverlapCheckCoroutine()
        {
            while (true)
            {
                CheckRotationAreaOverlap();
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }

        public override void PickUp(ItemVisual storedItem)
        {
            base.PickUp(storedItem);

            if (storedItem.ItemDefinition.Dimensions.Width == storedItem.ItemDefinition.Dimensions.Height)
                return;

            SetDisableRotator(true);
            _draggerItem = storedItem;
            if (_overlapCheckCoroutine != null)
                _itemsPage.StopCoroutine(_overlapCheckCoroutine);

            _overlapCheckCoroutine = _itemsPage.StartCoroutine(OverlapCheckCoroutine());
        }

        public override void Drop(ItemVisual storedItem, Vector2Int gridPosition)
        {
            base.Drop(storedItem, gridPosition);
            SetDisableRotator(false);

            _draggerItem = null;
            if (_overlapCheckCoroutine != null)
            {
                _itemsPage.StopCoroutine(_overlapCheckCoroutine);
                _overlapCheckCoroutine = null;
            }
        }
    }
}