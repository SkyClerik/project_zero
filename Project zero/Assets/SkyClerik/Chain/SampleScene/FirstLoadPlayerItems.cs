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

        private GlobalBox _globalBox;

        private void Awake()
        {
            if (_nextStep != null && _nextStep is IChain)
                NextComponent = _nextStep as IChain;
        }

        public void ExecuteStep()
        {
            _globalBox = ServiceProvider.Get<GlobalBox>();
            if (_globalBox == null)
                return;

            if (!_globalBox.GlobalGameProperty.IsNewGame)
                LoadItems();
            else
                Next();
        }

        private void Next()
        {
            NextComponent?.ExecuteStep();
            Destroy(this);
        }

        private void LoadItems()
        {
            var loadService = _globalBox.LoadService;
            var globalProperty = _globalBox.GlobalGameProperty;

            // slotIndex будет 0 всегда так как мы не планируем слоты сохранения
            var slotFolderPath = loadService.GetSaveSlotFolderPath(slotIndex: 0);
            loadService.LoadGlobalState(globalProperty, slotFolderPath);
            loadService.LoadAll(globalProperty, slotFolderPath);
        }
    }
}