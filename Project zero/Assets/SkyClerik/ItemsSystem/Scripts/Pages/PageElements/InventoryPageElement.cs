using Assets.SimpleLocalization.Scripts;
using System.Collections;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Представляет элемент UI страницы инвентаря, управляющий отображением предметов
    /// и взаимодействием с пользователем. Наследует функциональность базовой страницы сетки.
    /// </summary>
    public class InventoryPageElement : GridPageElementBase
    {
        private Label _inventoryPageTitle;
        private const string _inventoryPageTitleID = "inventory_page_title";
        private VisualElement _itemImage;
        private const string _itemImageID = "item_image";
        private VisualElement _descriptionBackground;
        private const string _descriptionBackgroundID = "description_background";
        private Label _lDescription;
        private const string _lDescriptionID = "l_description";
        private Label _lPriceName;
        private const string _lPriceNameID = "l_price_name";
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

        private GlobalItemStorage _itemStorage;

        private const string _localizationPrefix = "Inventory.";

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
            _inventoryPageTitle = _root.Q<Label>(_inventoryPageTitleID);
            _descriptionBackground = _root.Q(_descriptionBackgroundID);
            _rotationAreaRoot = _root.Q(_rotationAreaRootID);
            _rotationArea = _rotationAreaRoot.Q(_rotationAreaID);
            _bClose = _root.Q<Button>(_bCloseID);
            _itemImage = _root.Q(_itemImageID);
            _lDescription = _root.Q<Label>(_lDescriptionID);
            _lPriceName = _root.Q<Label>(_lPriceNameID);
            _lPriceValue = _root.Q<Label>(_lPriceValueID);

            _bClose.clicked += CloseClicked;
            SetDisableRotator(false);

            _itemStorage = ServiceProvider.Get<GlobalItemStorage>();

            LocalizationChanged();
            LocalizationManager.OnLocalizationChanged += LocalizationChanged;

            ServiceProvider.Get<InventoryAPI>().OnItemPickUp += EquipPageElement_OnItemPickUp;
            ServiceProvider.Get<InventoryAPI>().OnItemDrop += EquipPageElement_OnItemDrop;
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

            var inventoryAPI = ServiceProvider.Get<InventoryAPI>();
            if (inventoryAPI != null)
            {
                inventoryAPI.OnItemPickUp -= EquipPageElement_OnItemPickUp;
                inventoryAPI.OnItemDrop -= EquipPageElement_OnItemDrop;
                inventoryAPI.ClearOnItemGivenSubscriptions(); // Очищаем подписки на OnItemGiven
            }

            LocalizationManager.OnLocalizationChanged -= LocalizationChanged;
        }

        private void LocalizationChanged()
        {
            _inventoryPageTitle.text = LocalizationManager.Localize($"{_localizationPrefix}{_inventoryPageTitle.name}");
            _lPriceName.text = LocalizationManager.Localize($"{_localizationPrefix}{_lPriceName.name}");
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
                ServiceProvider.Get<InventoryAPI>().ClearOnItemGivenSubscriptions();
                _itemsPage.CloseAll();
            }
        }

        public void SetItemDescription(ItemBaseDefinition itemBaseDefinition)
        {
            _itemImage.SetBackgroundImage(itemBaseDefinition.Icon);
            //_lDescription.text = itemBaseDefinition.Description;
            _lDescription.text = _itemStorage.GlobalItemsStorageDefinition.GetOriginalItem(itemBaseDefinition.ID).Description;
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

        private void EquipPageElement_OnItemPickUp(ItemVisual item, GridPageElementBase gridPage)
        {
            if (item.ItemDefinition.Dimensions.Width == item.ItemDefinition.Dimensions.Height)
                return;

            SetDisableRotator(true);
            _draggerItem = item;
            if (_overlapCheckCoroutine != null)
                _itemsPage.StopCoroutine(_overlapCheckCoroutine);

            _overlapCheckCoroutine = _itemsPage.StartCoroutine(OverlapCheckCoroutine());
        }

        private void EquipPageElement_OnItemDrop(ItemVisual item, GridPageElementBase gridPage)
        {
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