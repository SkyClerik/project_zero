using SkyClerik.Inventory;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class DevelopHUD : MonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private UIDocument _developHudUiDocument;
    private Button _bInventoryNormal;
    private const string _bInventoryNormalID = "b_inventory_normal";
    private Button _bInventoryGive;
    private const string _bInventoryGiveID = "b_inventory_give";
    private Button _bTrueCraft;
    private const string _bTrueCraftID = "b_true_craft";
    private Button _bAddItem;
    private const string _bAddItemID = "b_add_item";
    private Button _bExitGame;
    private const string _bExitGameID = "b_exit_game";
    private ItemsPage _itemsPage;
    [SerializeField]
    private LutContainer _developLut;

    private void OnValidate()
    {
        _developHudUiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
    }

    void Start()
    {
        _itemsPage = ServiceProvider.Get<ItemsPage>();

        _developHudUiDocument.enabled = true;
        var root = _developHudUiDocument.rootVisualElement;
        _bInventoryNormal = root.Q<Button>(_bInventoryNormalID);
        _bInventoryGive = root.Q<Button>(_bInventoryGiveID);
        _bTrueCraft = root.Q<Button>(_bTrueCraftID);
        _bAddItem = root.Q<Button>(_bAddItemID);
        _bExitGame = root.Q<Button>(_bExitGameID);

        _bInventoryNormal.clicked += _bInventory_clicked;
        _bInventoryGive.clicked += _bInventoryGive_clicked;
        _bTrueCraft.clicked += _bTrueCraft_clicked;
        _bAddItem.clicked += _bAddItem_clicked;
        _bExitGame.clicked += _bExitGame_clicked;
    }


    private void OnDestroy()
    {
        _bInventoryNormal.clicked -= _bInventory_clicked;
        _bTrueCraft.clicked -= _bTrueCraft_clicked;
        _bExitGame.clicked -= _bExitGame_clicked;
    }

    private void _bInventory_clicked()
    {
        if (_itemsPage.IsInventoryVisible)
        {
            _itemsPage.CloseInventory();
            _itemsPage.CloseCraft();
        }
        else
        {
            _itemsPage.OpenInventoryNormal();
            _itemsPage.OpenCraft();
        }
    }

    private void _bInventoryGive_clicked()
    {
        if (_itemsPage.IsInventoryVisible)
            _itemsPage.CloseInventory();
        else
            _itemsPage.OpenInventoryGiveItem(_developLut.GetItems()[0]);
    }

    private void _bTrueCraft_clicked()
    {
        _itemsPage.IsCraftVisible = !_itemsPage.IsCraftVisible;
    }

    private void _bAddItem_clicked()
    {
        if (_itemsPage?.InventoryPage != null && _developLut != null)
            _itemsPage.InventoryPage.AddLoot(_developLut);
        else
            Debug.LogError("Не удалось получить доступ к инвентарю или лут-контейнеру!");
    }

    private void _bExitGame_clicked()
    {
        Application.Quit();
    }
}
