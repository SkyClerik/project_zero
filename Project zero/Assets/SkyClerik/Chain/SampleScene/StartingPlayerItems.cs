using UnityEngine;
using UnityEngine.Toolbox;
using System.Collections;
using SkyClerik.Inventory;
using SkyClerik.Utils;

namespace SkyClerik
{
    public class StartingPlayerItems : MonoBehaviour, IChain
    {
        [Header("Хранилище данных")]
        [SerializeField]
        private ItemContainerDefinition _itemDataStorageSO;

        [SerializeField]
        private MonoBehaviour _nextStep;
        public IChain NextComponent { get; set; }
        
        protected void Awake()
        {
            if (_nextStep != null && _nextStep is IChain)
                NextComponent = _nextStep as IChain;
        }

        public void ExecuteStep()
        {
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager == null)
            {
                //Debug.LogError("GameStateManager не найден в ServiceProvider!", this);
                return;
            }

            if (gameStateManager.GlobalGameState.IsNewGame)
            {
                StartCoroutine(GiveItemsToPlayer());
            }
            else
            {
                //Debug.Log("StartingPlayerItems: Пропускаем шаг, так как это не новая игра.");
                Next();
            }
        }

        private IEnumerator GiveItemsToPlayer()
        {
            // Сначала проверяем наличие исходных предметов
            if (_itemDataStorageSO == null || _itemDataStorageSO.Items == null || _itemDataStorageSO.Items.Count == 0)
            {
                //Debug.Log("Нет стартовых предметов для выдачи в StartingPlayerItems.", this);
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
                //Debug.LogError("Не удалось найти PlayerItemContainer через ServiceProvider в течение 5 секунд.", this);
                Next();
                yield break;
            }

            // Выдаем предметы одной командой, используя новую "умную" логику контейнера
            var unplacedItems = playerContainer.AddClonedItems(_itemDataStorageSO.Items);

            if (unplacedItems.Count > 0)
            {
                //Debug.LogWarning($"Не удалось выдать {unplacedItems.Count} стартовых предметов. Не хватило места в инвентаре. Невместившиеся предметы будут уничтожены.");
                foreach(var item in unplacedItems)
                {
                    Destroy(item);
                }
            }

            //Debug.Log("Выдача стартовых предметов завершена.", this);
            Next();
        }

        private void Next()
        {
            NextComponent?.ExecuteStep();
            Destroy(this);
        }
    }
}