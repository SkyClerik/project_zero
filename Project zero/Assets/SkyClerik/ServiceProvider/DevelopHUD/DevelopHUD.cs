using SkyClerik.Inventory;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;
using static SkyClerik.Inventory.ItemContainer;

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
        private UIDocument _uiDocument;
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

        private Button _bExitGame;
        private const string _bExitGameID = "b_exit_game";
        private GlobalGameProperty _globalGameProperty;
        private InventoryAPI _inventoryAPI;

        [SerializeField]
        private KeyCode _keyCode;

        [SerializeField]
        private LutContainer _developLut;

        private void OnValidate()
        {
            _uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
        }

        private void Awake()
        {
            _uiDocument.enabled = false;
            //_uiDocument.rootVisualElement.SetDisplay(false);
        }
        void Start()
        {
            //_itemsPage = ServiceProvider.Get<ItemsPage>();
            _globalGameProperty = ServiceProvider.Get<GlobalBox>()?.GlobalGameProperty;
            _inventoryAPI = ServiceProvider.Get<InventoryAPI>();

            _uiDocument.enabled = true;
            var root = _uiDocument.rootVisualElement;
            _bInventoryNormal = root.Q<Button>(_bInventoryNormalID);
            _bInventoryNormal.style.minHeight = 80;
            _bEquip = root.Q<Button>(_bEquipID);
            _bEquip.style.minHeight = 80;
            _bInventoryGive = root.Q<Button>(_bInventoryGiveID);
            _bInventoryGive.style.minHeight = 80;
            _bTrueCraft = root.Q<Button>(_bTrueCraftID);
            _bTrueCraft.style.minHeight = 80;
            _bAddItem = root.Q<Button>(_bAddItemID);
            _bAddItem.style.minHeight = 80;
            _bSave = root.Q<Button>(_bSaveID);
            _bSave.style.minHeight = 80;
            _bCheast = root.Q<Button>(_bCheastID);
            _bCheast.style.minHeight = 80;

            _bExitGame = root.Q<Button>(_bExitGameID);

            _bInventoryNormal.clicked += _bInventory_clicked;
            _bEquip.clicked += _bEquip_clicked;
            _bInventoryGive.clicked += _bInventoryGive_clicked;
            _bTrueCraft.clicked += _bTrueCraft_clicked;
            _bAddItem.clicked += _bAddItem_clicked;
            _bSave.clicked += _bSave_clicked;
            _bCheast.clicked += _bCheast_clicked;

            _bExitGame.clicked += _bExitGame_clicked;

            _uiDocument.enabled = false;
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
        }

        private void Update()
        {
            if (Input.GetKeyDown(_keyCode))
                _bInventory_clicked();
        }

        private void _bInventory_clicked()
        {
            if (_inventoryAPI.IsInventoryVisible)
            {
                _inventoryAPI.CloseAll();
            }
            else
            {
                // Открыть окно инвентаря (и попробовать открыть крафт потому что его доступность решается глобальным логическим свойством)
                _inventoryAPI.OpenInventoryAndCraft();
            }
        }

        private void _bEquip_clicked()
        {
            if (_inventoryAPI.IsInventoryVisible)
            {
                _inventoryAPI.CloseAll();
            }
            else
            {
                // Открыть окно инвентаря и страницы экипировки
                _inventoryAPI.OpenInventoryAndEquip();
            }
        }

        // открыть инвентарь для выбора предмета (отписки обязательные)
        private void _bInventoryGive_clicked()
        {
            if (_inventoryAPI.IsInventoryVisible)
            {
                _inventoryAPI.OnItemGiven -= OnItemGivenCallback;
                _inventoryAPI.CloseAll();
            }
            else
            {
                _inventoryAPI.OnItemGiven += OnItemGivenCallback;
                _inventoryAPI.OpenInventoryFromGiveItem(itemID: 0, tracing: true);
            }
        }

        private void OnItemGivenCallback(ItemBaseDefinition itemBaseDefinition)
        {
            var id = 0;
            var needCount = 3;
            if (itemBaseDefinition.ID == id)
            {
                _inventoryAPI.OnItemGiven -= OnItemGivenCallback;
                Debug.Log($"выбран нужный предмет : {itemBaseDefinition.ID} - {itemBaseDefinition}");
                RemoveResult RemoveResult = _inventoryAPI.TryRemoveItemInPlayerInventory(id, count: needCount);
                if (RemoveResult.IDidLook)
                {
                    Debug.Log($"Есть хоть один предмет");
                }
                if (RemoveResult.IsDeleted)
                {
                    Debug.Log($"Еще один лог о удалении предмета с id : {id}");
                }
                Debug.Log($"Мне не хватает: {RemoveResult.NotEnough}");

                _inventoryAPI.CloseAll();
            }
            else
            {
                Debug.Log($"выбран не нужный предмет : {itemBaseDefinition.ID} - {itemBaseDefinition}");
            }
        }

        private void _bTrueCraft_clicked()
        {
            // Изменить доступность крафта для игрока
            _globalGameProperty.MakeCraftAccessible = !_globalGameProperty.MakeCraftAccessible;
        }

        private void _bAddItem_clicked()
        {
            // Попробовать отправить предмет в инвентарь игрока
            if (_inventoryAPI.TryAddItemsToPlayerInventory(itemID: 0, out ItemBaseDefinition item))
            {
                Debug.Log($"Добавление в инвентарь предмета с id : {item.ID} Name : {item.DefinitionName} Price : {item.Price}");
            }
            else
            {
                Debug.Log($"Не удалось добавление в инвентарь предмет с id : 0");
            }
        }

        private void _bSave_clicked()
        {
            var globalBox = ServiceProvider.Get<GlobalBox>();
            if (globalBox == null)
                return;

            var saveService = globalBox.SaveService;
            var globalState = globalBox.GlobalGameProperty;

            // Сохраняем и глобальные настройки и инвентарь
            // slotIndex будет 0 всегда так как мы не планируем слоты сохранения            
            saveService.SaveAll(globalState, 0);
        }

        private void _bCheast_clicked()
        {
            if (_inventoryAPI.IsCheastVisible)
            {
                _inventoryAPI.CloseAll();
            }
            else
            {
                // Открыть сундук игрока
                _inventoryAPI.OpenCheast();
            }
        }

        private void _bExitGame_clicked()
        {
            Application.Quit();
        }
    }
}
