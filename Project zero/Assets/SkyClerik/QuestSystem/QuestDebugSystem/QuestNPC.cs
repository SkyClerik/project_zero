using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public class QuestNPC : MonoBehaviour
    {
        private NpcBobConfig _bobNpcConfig;

        private void OnMouseDown()
        {
            QuestAPI questAPI = ServiceProvider.Get<QuestAPI>();

            if (_bobNpcConfig == null)
            {
                _bobNpcConfig = questAPI.Npc<NpcBobConfig>();
                //_bobNpcConfig = questAPI.Npc(NpcID.NPC_ID_BOB) as BobNpcConfig; // Так не делай, хотя можешь!
                //_bobNpcConfig = (BobNpcConfig)questAPI.Npc(NpcID.NPC_ID_BOB); // Так тем более не делай!
            }

            // --- 

            if (_bobNpcConfig != null)
            {
                if (_bobNpcConfig.TryAcceptQuest(NpcBobQuest.TradeFirstStep, out QuestAcceptFailedInfo failedInfo) == false)
                {
                    Debug.Log("Не удалось принять квест");

                    if (failedInfo.TrustLackToMax > 0)
                        Debug.Log($"У тебя не хватает еще {failedInfo.TrustLackToMax} доверия");

                    if (failedInfo.CurActiveQuests > 0)
                        Debug.Log($"Ты уже взял {failedInfo.CurActiveQuests} заданий из доступных {questAPI.MaxActiveQuests}");
                }
            }


            // --- 

            if (_bobNpcConfig != null)
            {
                if (_bobNpcConfig.TryGetIdleQuests(out List<QuestInfo> quests))
                {
                    if (quests.Count >= 0)
                        Debug.Log("Получен список доступных для принятия заданий");
                }
            }

            // --- 

            if (_bobNpcConfig != null)
            {
                if (_bobNpcConfig.TryGetAcceptedQuests(out List<QuestInfo> quests))
                {
                    if (quests.Count >= 0)
                        Debug.Log("Получен список заданий активных на данный момент");
                }
            }

            // --- 

            if (_bobNpcConfig != null)
            {
                if (_bobNpcConfig.TryGetQuestsInState(out List<QuestInfo> quests, QuestInfoState.IsCompleted))
                {
                    if (quests.Count >= 0)
                        Debug.Log("Получен список уже завершенных заданий");
                }
            }

            // --- 

            if (_bobNpcConfig != null)
            {
                _bobNpcConfig.CompleteQuest(NpcBobQuest.TradeFirstStep);
            }


            // --- 


            if (_bobNpcConfig.IsWillRewardsFit(NpcBobQuest.TradeFirstStep))
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