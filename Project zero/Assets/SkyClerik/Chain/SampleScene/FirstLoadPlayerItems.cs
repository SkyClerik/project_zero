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
            var gameStateManager = ServiceProvider.Get<GlobalManager>();
            if (gameStateManager == null)
                return;

            var loadService = gameStateManager.LoadService;
            var globalProperty = gameStateManager.GlobalGameProperty;

            if (globalProperty.IsNewGame)
                return;

            // slotIndex будет 0 всегда так как мы не планируем слоты сохранения
            var slotFolderPath = loadService.GetSaveSlotFolderPath(slotIndex: 0);
            loadService.LoadGlobalState(globalProperty, slotFolderPath);
            loadService.LoadAll(globalProperty, slotFolderPath);
        }
    }
}