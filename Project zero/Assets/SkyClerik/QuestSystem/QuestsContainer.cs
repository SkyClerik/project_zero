using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public class QuestsContainer : MonoBehaviour
    {
        [Header("Динамика")]
        public List<PlayerNPCRelation> relations = new List<PlayerNPCRelation>();
        public List<QuestInfo> quests = new List<QuestInfo>();

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        [ContextMenu("AddRandomQuests")]
        private void AddRandomQuests()
        {
            relations.Clear();
            relations.Add(new PlayerNPCRelation("NPC_01"));
            relations.Add(new PlayerNPCRelation("NPC_02"));

            quests.Clear();
            quests.Add(new QuestInfo("QUEST_ID_01", needTrust: 0));
            quests.Add(new QuestInfo("QUEST_ID_02", needTrust: 0));
            quests.Add(new QuestInfo("QUEST_ID_03", needTrust: 0));
            quests.Add(new QuestInfo("QUEST_ID_04", needTrust: 0));
            quests.Add(new QuestInfo("QUEST_ID_05", needTrust: 0));
        }

        public PlayerNPCRelation GetRelation(string npcID)
        {
            var rel = relations.Find(r => r.npcID == npcID);
            return rel; // Структура вернется по значению, но в списке она обновится через Set
        }

        public void SetRelation(PlayerNPCRelation relation)
        {
            int idx = relations.FindIndex(r => r.npcID == relation.npcID);
            if (idx >= 0) relations[idx] = relation;
            else relations.Add(relation);
        }

        public bool HasActiveQuest(string questID, out QuestInfo quest)
        {
            quest = default;

            Debug.Log($"Пробуем найти {questID}");
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].questID == questID)
                {
                    quest = quests[i];
                    if (quests[i].questInfoState != QuestInfoState.None)
                    {
                        return true;
                    }
                }
            }
            if (quest != null)
                Debug.Log($"Нашли {quest.questID} ");

            //return quests.Exists(quest => quest.questID == questID           && quest.questInfoState != QuestInfoState.None);
            return false;
        }
    }

    // --- ОТНОШЕНИЯ С ИГРОКОМ (Динамика) ---
    // Сохраняется в Save Game
    [Serializable]
    public struct PlayerNPCRelation
    {
        public string npcID;
        [Tooltip("Я знаком")]
        public bool isAcquainted;       // Я знаком
        public int curTrustLevel;
        public int maxTrustLevel;

        public int curActiveQuests;     // Счетчик активных квестов
        public int maxActiveQuests;     // Лимит (например, 5)

        public PlayerNPCRelation(string id)
        {
            npcID = id;
            isAcquainted = false;
            curTrustLevel = 0;
            maxTrustLevel = 3;

            curActiveQuests = 0;
            maxActiveQuests = 5;
        }
    }

    public enum QuestInfoState : byte
    {
        None = 0, // Ожидает
        IsTaken = 10, // Захвачен
        IsCompleted = 20, // Завершен
        IsFailed = 30, // Провален
    }

    // --- КВЕСТ (Динамика) ---
    // Сохраняется в Save Game
    [Serializable]
    public class QuestInfo
    {
        public string questID;
        public string giverNPC_ID;      // Кто выдал (ссылка на npcID)
        public int needTrustLevel;
        public QuestInfoState questInfoState;

        public QuestInfo(string qID, int needTrust)
        {
            questID = qID;
            giverNPC_ID = string.Empty;
            needTrustLevel = needTrust;
            questInfoState = QuestInfoState.None;
        }
    }
}
