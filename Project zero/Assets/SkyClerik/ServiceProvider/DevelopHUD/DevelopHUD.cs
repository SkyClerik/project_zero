using SkyClerik.EquipmentSystem;
using SkyClerik.Inventory;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Utils
{
    /// <summary>
    /// Компонент, управляющий UI для режима разработки (DevelopHUD).
    /// Предоставляет кнопки для отладки и быстрого доступа к функциям инвентаря, крафта, сохранения и выхода из игры.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DevelopHUD : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private UIDocument _developHudUiDocument;
        private Button _bInventoryNormal;
        private const string _bInventoryNormalID = "b_inventory_normal";
        private Button _bEquip;
        private const string _bEquipID = "b_equip";
        private Button _bInventoryGive;
        private const string _bInventoryGiveID = "b_inventory_give";
        private Button _bTrueCraft;
        private const string _bTrueCraftID = "b_true_craft";
        private Button _bAddItem;
        private const string _bAddItemID = "b_add_item";
        private Button _bSave;
        private const string _bSaveID = "b_save";
        private Button _bCheast;
        private const string _bCheastID = "b_cheast";
        private Button _bLut;
        private const string _bLutID = "b_lut";

        private Button _bExitGame;
        private const string _bExitGameID = "b_exit_game";
        private ItemsPage _itemsPage;
        private GlobalGameProperty _globalGameProperty;

        [SerializeField]
        private LutContainer _developLut;

        private void OnValidate()
        {
            _developHudUiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
        }

        void Start()
        {
            _itemsPage = ServiceProvider.Get<ItemsPage>();
            _globalGameProperty = ServiceProvider.Get<GlobalServices>()?.GlobalGameProperty;

            _developHudUiDocument.enabled = true;
            var root = _developHudUiDocument.rootVisualElement;
            _bInventoryNormal = root.Q<Button>(_bInventoryNormalID);
            _bEquip = root.Q<Button>(_bEquipID);
            _bInventoryGive = root.Q<Button>(_bInventoryGiveID);
            _bTrueCraft = root.Q<Button>(_bTrueCraftID);
            _bAddItem = root.Q<Button>(_bAddItemID);
            _bSave = root.Q<Button>(_bSaveID);
            _bCheast = root.Q<Button>(_bCheastID);
            _bLut = root.Q<Button>(_bLutID);

            _bExitGame = root.Q<Button>(_bExitGameID);

            _bInventoryNormal.clicked += _bInventory_clicked;
            _bEquip.clicked += _bEquip_clicked;
            _bInventoryGive.clicked += _bInventoryGive_clicked;
            _bTrueCraft.clicked += _bTrueCraft_clicked;
            _bAddItem.clicked += _bAddItem_clicked;
            _bSave.clicked += _bSave_clicked;
            _bCheast.clicked += _bCheast_clicked;
            _bLut.clicked += _bLut_clicked;

            _bExitGame.clicked += _bExitGame_clicked;
        }


        private void OnDestroy()
        {
            _bInventoryNormal.clicked -= _bInventory_clicked;
            _bEquip.clicked -= _bEquip_clicked;
            _bInventoryGive.clicked -= _bInventoryGive_clicked;
            _bTrueCraft.clicked -= _bTrueCraft_clicked;
            _bAddItem.clicked -= _bAddItem_clicked;
            _bExitGame.clicked -= _bExitGame_clicked;
            _bSave.clicked -= _bSave_clicked;
            _bCheast.clicked -= _bCheast_clicked;
            _bLut.clicked -= _bLut_clicked;
        }

        private void _bInventory_clicked()
        {
            if (_itemsPage.IsInventoryVisible)
                _itemsPage.CloseAll();
            else
                _itemsPage.OpenInventoryAndCraft();
        }

        private void _bEquip_clicked()
        {
            if (_itemsPage.IsEquipVisible)
                _itemsPage.CloseAll();
            else
                _itemsPage.OpenEquip();
        }

        private void _bInventoryGive_clicked()
        {
            if (_itemsPage.IsInventoryVisible)
                _itemsPage.CloseAll();
            else
                _itemsPage.OpenInventoryFromGiveItem(itemID: 0);
        }

        private void _bTrueCraft_clicked()
        {
            _globalGameProperty.MakeCraftAccessible = !_globalGameProperty.MakeCraftAccessible;
        }

        private void _bAddItem_clicked()
        {
            _developLut.TransferItemsToPlayerInventoryContainer();
        }

        private void _bSave_clicked()
        {
            var gameStateManager = ServiceProvider.Get<GlobalServices>();
            if (gameStateManager == null)
                return;

            var saveService = gameStateManager.SaveService;
            var globalState = gameStateManager.GlobalGameProperty;

            // slotIndex будет 0 всегда так как мы не планируем слоты сохранения            
            saveService.SaveAll(globalState, 0);
        }

        private void _bCheast_clicked()
        {
            if (_itemsPage.IsCheastVisible)
                _itemsPage.CloseAll();
            else
                _itemsPage.OpenCheast();
        }

        private void _bLut_clicked()
        {
            //if (_itemsPage.IsLutVisible)
            //    _itemsPage.CloseAll();
            //else
            //    _itemsPage.OpenLut();

            if (_itemsPage.IsLutVisible)
                _itemsPage.CloseAll();
            else
                _developLut.OpenLutPage();
        }


        private void _bExitGame_clicked()
        {
            Application.Quit();
        }
    }
}
