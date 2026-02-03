using SkyClerik.Inventory;
using SkyClerik.Utils;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public class FirstLoadPlayerItems : MonoBehaviour, IChain
    {
        [SerializeField]
        private MonoBehaviour _nextStep;
        public IChain NextComponent { get; set; }

        private void Awake()
        {
            if (_nextStep != null && _nextStep is IChain)
                NextComponent = _nextStep as IChain;
        }

        public void ExecuteStep()
        {
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager == null)
            {
                Next();
                return;
            }

            if (gameStateManager.GlobalGameState.IsNewGame)
            {
                Next();
                return;
            }

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            if (itemsPage == null)
            {
                Next();
                return;
            }

            var containersToLoad = new ItemContainer[]
            {
                   itemsPage.InventoryItemContainer,
                   itemsPage.CraftItemContainer
            };

            LoadItems();
            Next();
        }

        private void Next()
        {
            NextComponent?.ExecuteStep();
            Destroy(this);
        }

        private void LoadItems()
        {
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager == null)
                return;

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            if (itemsPage == null)
                return;

            var loadService = gameStateManager.LoadService;
            var globalState = gameStateManager.GlobalGameState;
            string slotFolderPath = loadService.GetSaveSlotFolderPath(globalState.CurrentSaveSlotIndex);

            loadService.LoadItemContainer(itemsPage.InventoryItemContainer, slotFolderPath);
            loadService.LoadItemContainer(itemsPage.CraftItemContainer, slotFolderPath);

            Debug.Log($"[DevelopHUD] «агрузка контейнеров из слота {globalState.CurrentSaveSlotIndex} завершена.");
        }
    }
}