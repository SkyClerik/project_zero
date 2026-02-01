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
                Debug.LogError("FirstLoadPlayerItems: GameStateManager не найден в ServiceProvider!", this);
                Next();
                return;
            }

            if (gameStateManager.GlobalGameState.IsNewGame)
            {
                Debug.Log("FirstLoadPlayerItems: Пропускаем шаг, так как это новая игра.");
                Next();
                return;
            }

            Debug.Log("FirstLoadPlayerItems: Начало загрузки предметов для игрока...");

            var itemsPage = ServiceProvider.Get<ItemsPage>();
            if (itemsPage == null)
            {
                Debug.LogError("FirstLoadPlayerItems: ItemsPage не найден в ServiceProvider! Невозможно определи контейнеры для загрузки.", this);

                Next();
                return;
            }

            var containersToLoad = new ItemContainer[]
            {
                   itemsPage.InventoryItemContainer,
                   itemsPage.CraftItemContainer
            };

            gameStateManager.LoadService.LoadAll(gameStateManager.GlobalGameState, containersToLoad);

            Debug.Log("FirstLoadPlayerItems: Загрузка предметов завершена.");
            Next();
        }

        private void Next()
        {
            NextComponent?.ExecuteStep();
            Destroy(this);
        }
    }
}