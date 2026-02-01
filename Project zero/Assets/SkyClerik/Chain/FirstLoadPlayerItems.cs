using SkyClerik.Inventory;
using SkyClerik.Utils;
using UnityEngine;
using UnityEngine.DataEditor;
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
                //Debug.LogError("FirstLoadPlayerItems: GameStateManager    ServiceProvider!", this);
                Next();
                return;
            }

            if (gameStateManager.GlobalGameState.IsNewGame)
            {
                //Debug.Log("FirstLoadPlayerItems:  ,     .");
                Next();
                return;
            }

            //Debug.Log("FirstLoadPlayerItems:     ...");

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            if (itemsPage == null)
            {
                //Debug.LogError("FirstLoadPlayerItems: ItemsPage    ServiceProvider!     .", this);

                Next();
                return;
            }

            var containersToLoad = new ItemContainer[]
            {
                   itemsPage.InventoryItemContainer,
                   itemsPage.CraftItemContainer
            };

            gameStateManager.LoadService.LoadAll(gameStateManager.GlobalGameState, containersToLoad);

            //Debug.Log("FirstLoadPlayerItems:   .");
            Next();
        }

        private void Next()
        {
            NextComponent?.ExecuteStep();
            Destroy(this);
        }
    }
}