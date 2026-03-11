using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public class QuestNPC2 : MonoBehaviour
    {
        private NpcConnorConfig _npcConnorConfig;

        private void OnMouseDown()
        {
            QuestAPI questAPI = ServiceProvider.Get<QuestAPI>();

            if (_npcConnorConfig == null)
            {
                _npcConnorConfig = questAPI.Npc<NpcConnorConfig>();
                //_bobNpcConfig = questAPI.Npc(NpcID.NPC_ID_BOB) as BobNpcConfig; // Так не делай, хотя можешь!
                //_bobNpcConfig = (BobNpcConfig)questAPI.Npc(NpcID.NPC_ID_BOB); // Так тем более не делай!
            }

            // --- 

            if (_npcConnorConfig != null)
            {
                if (_npcConnorConfig.TryAcceptQuest(NpcConnorQuest.BringBeer, out QuestAcceptFailedInfo failedInfo) == false)
                {
                    Debug.Log("Не удалось принять квест");

                    if (failedInfo.TrustLackToMax > 0)
                        Debug.Log($"У тебя не хватает еще {failedInfo.TrustLackToMax} доверия");

                    if (failedInfo.CurActiveQuests > 0)
                        Debug.Log($"Ты уже взял {failedInfo.CurActiveQuests} заданий из доступных {questAPI.MaxActiveQuests}");
                }

                if (_npcConnorConfig.TryAcceptQuest(NpcConnorQuest.CleanGarage, out QuestAcceptFailedInfo failedInfo1) == false)
                {
                    Debug.Log("Не удалось принять квест");

                    if (failedInfo1.TrustLackToMax > 0)
                        Debug.Log($"У тебя не хватает еще {failedInfo1.TrustLackToMax} доверия");

                    if (failedInfo1.CurActiveQuests > 0)
                        Debug.Log($"Ты уже взял {failedInfo1.CurActiveQuests} заданий из доступных {questAPI.MaxActiveQuests}");
                }
            }


            // --- 

            if (_npcConnorConfig != null)
            {
                if (_npcConnorConfig.TryGetIdleQuests(out List<QuestInfo> quests))
                {
                    if (quests.Count >= 0)
                        Debug.Log("Получен список доступных для принятия заданий");
                }
            }

            // --- 

            if (_npcConnorConfig != null)
            {
                if (_npcConnorConfig.TryGetAcceptedQuests(out List<QuestInfo> quests))
                {
                    if (quests.Count >= 0)
                        Debug.Log("Получен список заданий активных на данный момент");
                }
            }

            // --- 

            if (_npcConnorConfig != null)
            {
                if (_npcConnorConfig.TryGetQuestsInState(out List<QuestInfo> quests, QuestInfoState.IsCompleted))
                {
                    if (quests.Count >= 0)
                        Debug.Log("Получен список уже завершенных заданий");
                }
            }

            // --- 

            if (_npcConnorConfig != null)
            {
                _npcConnorConfig.CompleteQuest(NpcConnorQuest.CleanGarage);
            }


            // --- 


            if (_npcConnorConfig.IsWillRewardsFit(NpcConnorQuest.CleanGarage))
            {
                Debug.Log($"Все предметы поместятся в инвентарь");
            }
            else
            {
                Debug.Log($"Предметы НЕ поместятся в инвентарь");
            }
        }
    }
}