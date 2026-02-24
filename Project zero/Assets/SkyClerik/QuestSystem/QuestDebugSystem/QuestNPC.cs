using UnityEngine;

namespace SkyClerik
{
    public class QuestNPC : MonoBehaviour
    {
        [SerializeField]
        private QuestSystem _questSystem;
        [SerializeField]
        private ActorsContainer _actorsContainer;

        [SerializeField]
        private QuestInfo questInfo;
        [SerializeField]
        private NPCConfig npcConfig;

        private void OnMouseDown()
        {
            TryAcceptQuest();
        }

        private void TryAcceptQuest()
        {
            if (_questSystem.TryAcceptQuest(_actorsContainer, npcConfig.npcID, questInfo.questID, questInfo.needTrustLevel) == false)
            {
                Debug.Log($"Что то пошло не так и мы не смогли добавить квест");
            }
        }

        [ContextMenu("Set_NPC_01_QUEST_FETCH_SWORD")]
        private void Set_NPC_01_QUEST_FETCH_SWORD()
        {
            npcConfig.npcID = "NPC_01";
            questInfo.questID = "QUEST_FETCH_SWORD";
        }
    }
}
