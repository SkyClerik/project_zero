using UnityEngine;

namespace SkyClerik.Utils
{
    // TODO Временно не используется потому что его подменяет заглушка главного меню игры
    public class ChainTrigger : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Сюда перетаскиваем первое звено которое начнет цепочку")]
        private MonoBehaviour _firstStep;
        private IChain _chainHead;

        private void Awake()
        {
            if (_firstStep != null && _firstStep is IChain)
                _chainHead = _firstStep as IChain;
            else
                Debug.LogError("Первое звено цепи не назначено или не реализует IChain!", this);
        }

        public void StartChain()
        {
            _chainHead?.ExecuteStep();
        }
    }
}