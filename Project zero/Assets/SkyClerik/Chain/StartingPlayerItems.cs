using UnityEngine;
using UnityEngine.Toolbox;
using System.Collections;
using SkyClerik.GlobalGameStates;

namespace SkyClerik.Inventory
{
    public class StartingPlayerItems : ItemContainer, IChain
    {
        [SerializeField]
        private MonoBehaviour _nextStep;
        public IChain NextComponent { get; set; }
        
        protected override void Awake()
        {
            base.Awake();
            if (_nextStep != null && _nextStep is IChain)
                NextComponent = _nextStep as IChain;
        }

        public void ExecuteStep()
        {
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager == null)
            {
                Debug.LogError("GameStateManager не найден в ServiceProvider!", this);
                return;
            }

            if (gameStateManager.GlobalGameState.IsNewGame)
            {
                StartCoroutine(GiveItemsToPlayer());
            }
            else
            {
                Debug.Log("StartingPlayerItems: Пропускаем шаг, так как это не новая игра.");
                Next();
            }
        }

        private IEnumerator GiveItemsToPlayer()
        {
            // Сначала проверяем наличие исходных предметов
            if (ItemDataStorageSO == null || ItemDataStorageSO.Items == null || ItemDataStorageSO.Items.Count == 0)
            {
                Debug.Log("Нет стартовых предметов для выдачи в StartingPlayerItems.", this);
                Next();
                yield break;
            }

            PlayerItemContainer playerContainer = null;
            float timeout = 5f; // 5-секундный таймаут
            float elapsedTime = 0f;

            // Ждем, пока PlayerItemContainer не будет зарегистрирован, или до истечения таймаута
            while (playerContainer == null && elapsedTime < timeout)
            {
                playerContainer = ServiceProvider.Get<PlayerItemContainer>();
                if (playerContainer == null)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            // Проверяем, нашли ли мы контейнер или вышло время
            if (playerContainer == null)
            {
                Debug.LogError("Не удалось найти PlayerItemContainer через ServiceProvider в течение 5 секунд.", this);
                Next();
                yield break;
            }

            // Выдаем предметы
            foreach (var itemDef in ItemDataStorageSO.Items)
            {
                if (itemDef != null)
                    playerContainer.AddItemAsClone(itemDef);
            }

            Debug.Log("Стартовые предметы успешно переданы в контейнер игрока.", this);
            Next();
        }

        private void Next()
        {
            NextComponent?.ExecuteStep();
            Destroy(this);
        }
    }
}