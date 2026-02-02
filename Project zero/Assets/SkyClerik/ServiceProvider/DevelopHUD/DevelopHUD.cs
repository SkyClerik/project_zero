using SkyClerik.Inventory;
using System.IO;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik.Utils
{
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

        private Button _bSave;
        private const string _bSaveID = "b_save";
        private Button _bLoad;
        private const string _bLoadID = "b_load";

        private Button _bTestUnityJson; // Кнопка для теста Unity JsonUtility
        private const string _bTestUnityJsonID = "b_test"; // ID для тестовой кнопки

        private Button _bExitGame;
        private const string _bExitGameID = "b_exit_game";
        private ItemsPage _itemsPage;
        [SerializeField]
        private ItemContainerDefinition _developLut;

        [SerializeField]
        private ItemContainerDefinition _developLut2; // Для десериализованного объекта

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
            _bSave = root.Q<Button>(_bSaveID);
            _bLoad = root.Q<Button>(_bLoadID);
            _bTestUnityJson = root.Q<Button>(_bTestUnityJsonID); // Находим тестовую кнопку

            _bExitGame = root.Q<Button>(_bExitGameID);

            _bInventoryNormal.clicked += _bInventory_clicked;
            _bInventoryGive.clicked += _bInventoryGive_clicked;
            _bTrueCraft.clicked += _bTrueCraft_clicked;
            _bAddItem.clicked += _bAddItem_clicked;
            _bSave.clicked += _bSave_clicked;
            _bLoad.clicked += _bLoad_clicked;
            _bTestUnityJson.clicked += _bTestUnityJson_clicked; // Привязываем обработчик для тестовой кнопки

            _bExitGame.clicked += _bExitGame_clicked;
        }

        private void OnDestroy()
        {
            _bInventoryNormal.clicked -= _bInventory_clicked;
            _bTrueCraft.clicked -= _bTrueCraft_clicked;
            _bExitGame.clicked -= _bExitGame_clicked;
            // Отписываемся от события тестовой кнопки
            if (_bTestUnityJson != null)
            {
                _bTestUnityJson.clicked -= _bTestUnityJson_clicked;
            }
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
                _itemsPage.OpenInventoryGiveItem(itemId: 0);
        }

        private void _bTrueCraft_clicked()
        {
            _itemsPage.MakeCraftAccessible = !_itemsPage.MakeCraftAccessible;
        }

        private void _bAddItem_clicked()
        {
            //if (_itemsPage?.InventoryPage != null && _developLut != null)
            //   _itemsPage.InventoryPage.AddLoot(_developLut);
            //else
            //Debug.LogError("Не удалось получить доступ к инвентарю или лут-контейнеру!");
        }

        private void _bSave_clicked()
        {
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager == null)
            {
                //Debug.LogError("GameStateManager не найден в ServiceProvider!");
                return;
            }

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            if (itemsPage == null)
            {
                //Debug.LogError("ItemsPage не найден в ServiceProvider!");
                return;
            }

            var saveService = gameStateManager.SaveService;
            var globalState = gameStateManager.GlobalGameState;
            string slotFolderPath = saveService.GetSaveSlotFolderPath(globalState.CurrentSaveSlotIndex);

            saveService.SaveItemContainer(itemsPage.InventoryItemContainer, slotFolderPath);
            saveService.SaveItemContainer(itemsPage.CraftItemContainer, slotFolderPath);

            //Debug.Log($"[DevelopHUD] Сохранение контейнеров в слот {globalState.CurrentSaveSlotIndex} завершено.");
        }

        private void _bLoad_clicked()
        {
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager == null)
            {
                //Debug.LogError("GameStateManager не найден в ServiceProvider!");
                return;
            }

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            if (itemsPage == null)
            {
                //Debug.LogError("ItemsPage не найден в ServiceProvider!");
                return;
            }

            var loadService = gameStateManager.LoadService;
            var globalState = gameStateManager.GlobalGameState;
            string slotFolderPath = loadService.GetSaveSlotFolderPath(globalState.CurrentSaveSlotIndex);

            loadService.LoadItemContainer(itemsPage.InventoryItemContainer, slotFolderPath);
            loadService.LoadItemContainer(itemsPage.CraftItemContainer, slotFolderPath);

            //Debug.Log($"[DevelopHUD] Загрузка контейнеров из слота {globalState.CurrentSaveSlotIndex} завершена.");

        }

        private void TestJsonUtilityScenario()
        {
            Debug.Log("\n--- Test Unity JsonUtility Scenario Started ---");

            if (_developLut == null)
            {
                Debug.LogError("[JsonUtility Test] _developLut не назначен в инспекторе! Невозможно протестировать.", this);
                return;
            }

            // Сериализуем _developLut в JSON
            string json = JsonUtility.ToJson(_developLut);
            Debug.Log($"[JsonUtility Test] Serialized JSON:\n{json}");

            // Создаем новый экземпляр ItemContainerDefinition для десериализации
            // _developLut2 уже объявлен в классе DevelopHUD
            _developLut2 = ScriptableObject.CreateInstance<ItemContainerDefinition>();
            _developLut2.name = "Deserialized Test Container (Unity JsonUtility)";

            // Десериализуем JSON в _developLut2
            JsonUtility.FromJsonOverwrite(json, _developLut2);

            // Проверяем результаты
            if (_developLut2 != null)
            {
                Debug.Log($"[JsonUtility Test] Deserialized Container: Name='{_developLut2.name}', GUID='{_developLut2.ContainerGuid}', InstanceID={_developLut2.GetInstanceID()}, Items.Count={_developLut2.Items.Count}");

                Debug.Log($"[JsonUtility Test] ReferenceEquals(_developLut, _developLut2): {ReferenceEquals(_developLut, _developLut2)}");
                Debug.Log($"[JsonUtility Test] _developLut.GetInstanceID() == _developLut2.GetInstanceID(): {_developLut.GetInstanceID() == _developLut2.GetInstanceID()}");

                if (!ReferenceEquals(_developLut, _developLut2) && _developLut.GetInstanceID() != _developLut2.GetInstanceID())
                {
                    Debug.Log("[JsonUtility Test] Test Passed: Deserialized object is a new instance (not a reference to the original).");
                    if (_developLut.ContainerGuid == _developLut2.ContainerGuid)
                    {
                        Debug.Log("[JsonUtility Test] Test Passed: GUIDs match.");
                    }
                    else
                    {
                        Debug.LogError("[JsonUtility Test] Test Failed: GUIDs do NOT match!");
                    }
                }
                else
                {
                    Debug.LogError("[JsonUtility Test] Test Failed: Deserialized object is the SAME instance as the original!");
                }

                // Проверка предметов (особенно на полиморфизм)
                if (_developLut.Items.Count > 0 && _developLut2.Items.Count > 0)
                {
                    Debug.Log($"[JsonUtility Test] Original Item[0] Type: {_developLut.Items[0].GetType().Name}");
                    Debug.Log($"[JsonUtility Test] Deserialized Item[0] Type: {_developLut2.Items[0].GetType().Name}");
                    if (_developLut.Items[0].GetType() != _developLut2.Items[0].GetType())
                    {
                        Debug.LogError("[JsonUtility Test] Test Failed: Item types do NOT match! JsonUtility does not support polymorphism.");
                    }
                    else
                    {
                        Debug.Log("[JsonUtility Test] Test Passed: Item types match (for first item).");
                    }
                }
            }
            else
            {
                Debug.LogError("[JsonUtility Test] Test Failed: Deserialization returned null.");
            }
            Debug.Log("--- Test Unity JsonUtility Scenario Finished ---\n");
        }

        private void TestNewtonsoftJsonScenario()
        {
            Debug.Log("--- Test Newtonsoft.Json Scenario Started ---");

            if (_developLut == null)
            {
                Debug.LogError("[Newtonsoft.Json Test] _developLut не назначен в инспекторе! Невозможно протестировать.", this);
                return;
            }

            // Сериализуем _developLut
            string json = JsonScriptableObjectSerializer.SerializeScriptableObject(_developLut);
            Debug.Log($"[Newtonsoft.Json Test] Serialized JSON:\n{json}");

            // Десериализуем обратно
            ItemContainerDefinition deserializedContainer = JsonScriptableObjectSerializer.DeserializeScriptableObject<ItemContainerDefinition>(json);

            // Проверяем результаты
            if (deserializedContainer != null)
            {
                Debug.Log($"[Newtonsoft.Json Test] Deserialized Container: Name='{deserializedContainer.name}', GUID='{deserializedContainer.ContainerGuid}', InstanceID={deserializedContainer.GetInstanceID()}, Items.Count={deserializedContainer.Items.Count}");
                Debug.Log($"[Newtonsoft.Json Test] ReferenceEquals(_developLut, deserializedContainer): {ReferenceEquals(_developLut, deserializedContainer)}");
                Debug.Log($"[Newtonsoft.Json Test] _developLut.GetInstanceID() == deserializedContainer.GetInstanceID(): {_developLut.GetInstanceID() == deserializedContainer.GetInstanceID()}");

                if (!ReferenceEquals(_developLut, deserializedContainer) && _developLut.GetInstanceID() != deserializedContainer.GetInstanceID())
                {
                    Debug.Log("[Newtonsoft.Json Test] Test Passed: Deserialized object is a new instance (not a reference to the original).");
                    if (_developLut.ContainerGuid == deserializedContainer.ContainerGuid)
                    {
                        Debug.Log("[Newtonsoft.Json Test] Test Passed: GUIDs match.");
                    }
                    else
                    {
                        Debug.LogError("[Newtonsoft.Json Test] Test Failed: GUIDs do NOT match!");
                    }
                }
                else
                {
                    Debug.LogError("[Newtonsoft.Json Test] Test Failed: Deserialized object is the SAME instance as the original!");
                }

                // Проверка предметов (особенно на полиморфизм)
                if (_developLut.Items.Count > 0 && deserializedContainer.Items.Count > 0)
                {
                    Debug.Log($"[Newtonsoft.Json Test] Original Item[0] Type: {_developLut.Items[0].GetType().Name}");
                    Debug.Log($"[Newtonsoft.Json Test] Deserialized Item[0] Type: {deserializedContainer.Items[0].GetType().Name}");
                    if (_developLut.Items[0].GetType() != deserializedContainer.Items[0].GetType())
                    {
                        Debug.LogError("[Newtonsoft.Json Test] Test Failed: Item types do NOT match! Newtonsoft.Json with TypeNameHandling SHOULD support polymorphism.");
                    }
                    else
                    {
                        Debug.Log("[Newtonsoft.Json Test] Test Passed: Item types match (for first item).");
                    }
                    Debug.Log($"[Newtonsoft.Json Test] Original Item[0] WrapperIndex: {_developLut.Items[0].WrapperIndex}");
                    Debug.Log($"[Newtonsoft.Json Test] Deserialized Item[0] WrapperIndex: {deserializedContainer.Items[0].WrapperIndex}");
                    if (_developLut.Items[0].WrapperIndex != deserializedContainer.Items[0].WrapperIndex)
                    {
                        Debug.LogError("[Newtonsoft.Json Test] Test Failed: WrapperIndex do NOT match!");
                    }
                    else
                    {
                        Debug.Log("[Newtonsoft.Json Test] Test Passed: WrapperIndex match.");
                    }
                }
            }
            else
            {
                Debug.LogError("[Newtonsoft.Json Test] Test Failed: Deserialization returned null.");
            }
            Debug.Log("--- Test Newtonsoft.Json Scenario Finished ---\n");
        }

        private void _bTestUnityJson_clicked()
        {
            TestNewtonsoftJsonScenario();
        }

        private void _bExitGame_clicked()
        {
            Application.Quit();
        }
    }
}
