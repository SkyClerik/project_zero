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

    private Button _bInventory;
    private const string _bInventoryID = "b_inventory";
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
        _bInventory = root.Q<Button>(_bInventoryID);
        _bTrueCraft = root.Q<Button>(_bTrueCraftID);
        _bAddItem = root.Q<Button>(_bAddItemID);
        _bExitGame = root.Q<Button>(_bExitGameID);

        _bInventory.clicked += _bInventory_clicked;
        _bTrueCraft.clicked += _bTrueCraft_clicked;
        _bAddItem.clicked += _bAddItem_clicked;
        _bExitGame.clicked += _bExitGame_clicked;
    }

    private void OnDestroy()
    {
        _bInventory.clicked -= _bInventory_clicked;
        _bTrueCraft.clicked -= _bTrueCraft_clicked;
        _bExitGame.clicked -= _bExitGame_clicked;
    }

    private void _bInventory_clicked()
    {
        _itemsPage.ShowInventory = !_itemsPage.ShowInventory;
    }

    private void _bTrueCraft_clicked()
    {
        _itemsPage.CraftElementVisible = !_itemsPage.CraftElementVisible;
    }

    private void _bAddItem_clicked()
    {
        if (_itemsPage?.InventoryPage != null && _developLut != null)
        {
            _itemsPage.InventoryPage.AddLoot(_developLut);
        }
        else
        {
            Debug.LogError("Не удалось получить доступ к инвентарю или лут-контейнеру!");
        }
    }

    private void _bExitGame_clicked()
    {
        Application.Quit();
    }
}
