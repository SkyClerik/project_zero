using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public class QuestAPI : MonoBehaviour
    {
        private QuestSystem _questSystem;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            if (_questSystem == null)
            {
                if (this.TryGetComponentInChildren(out _questSystem, includeInactive: false) == false)
                {
                    _questSystem = ServiceProvider.Get<QuestSystem>();
                }
            }
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        public bool TryAcceptQuest(ActorsContainer actorsContainer, string npcID, string questID, int needTrust) => _questSystem.TryAcceptQuest(actorsContainer, npcID, questID, needTrust);

        public void CompleteQuest(string questID) => _questSystem.CompleteQuest(questID);
    }
}
