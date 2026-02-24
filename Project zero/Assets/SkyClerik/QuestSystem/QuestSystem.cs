using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    [System.Serializable]
    [RequireComponent(typeof(QuestAPI), typeof(QuestsContainer))]
    public class QuestSystem : MonoBehaviour
    {
        private QuestsContainer _questsContainer;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            if (_questsContainer == null)
            {
                if (this.TryGetComponentInChildren(out _questsContainer, includeInactive: false) == false)
                    _questsContainer = ServiceProvider.Get<QuestsContainer>();
            }
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        // Основная логика выдачи квеста
        public bool TryAcceptQuest(ActorsContainer actorsContainer, string npcID, string questID, int needTrust)
        {
            // Проверка существования NPC в базе
            NPCConfig npc = actorsContainer.GetNPCConfig(npcID);
            if (npc.npcID == null) // Структура не найдена
            {
                Debug.Log($"[Ошибка] NPC с ID {npcID} не существует в базе.");
                return false;
            }

            // Получаем или создаем запись об отношениях
            PlayerNPCRelation relation = _questsContainer.GetRelation(npcID);

            // Проверка: является ли NPC квестодателем
            if (!npc.isQuestGiver)
            {
                Debug.Log($"[Диалог] {npc.displayName} не выдает задания.");
                return false;
            }

            // Если записи нет (нулевая структура), создаем новую
            if (relation.npcID == null)
            {
                relation = new PlayerNPCRelation(npcID);
                _questsContainer.SetRelation(relation);
            }

            // Проверка: Знакомы ли?
            if (!relation.isAcquainted)
            {
                Debug.Log($"[Диалог] Сначала нужно познакомиться с {npc.displayName} его ID: {npc.npcID}.");
                return false;
            }

            // Проверка: Лимит квестов
            if (relation.curActiveQuests >= relation.maxActiveQuests)
            {
                Debug.Log($"[Диалог] У {npc.displayName} нет больше заданий (Лимит: {relation.maxActiveQuests}).");
                return false;
            }

            // Проверка: Доверие
            if (relation.curTrustLevel < needTrust)
            {
                Debug.Log($"[Диалог] Недостаточно доверия ({relation.curTrustLevel:P0}).");
                return false;
            }

            // Проверка: Квест уже не взять
            QuestInfo quest;
            if (_questsContainer.HasActiveQuest(questID, out quest))
            {
                Debug.Log($"[Система] Квест {quest.questID} уже активен.");
                return false;
            }

            // --- УСПЕХ: Выдача квеста ---
            if (quest != null)
            {
                quest.giverNPC_ID = npcID;
                quest.questInfoState = QuestInfoState.IsTaken;
                // Обновляем счетчик
                relation.curActiveQuests++;
                _questsContainer.SetRelation(relation);
                Debug.Log($"[Система] Квест '{questID}' принят от {npc.displayName}.");
                return true;
            }

            return false;
        }

        // Завершение квеста
        public void CompleteQuest(string questID)
        {
            int qIdx = _questsContainer.quests.FindIndex(q => q.questID == questID);
            if (qIdx < 0) return;

            QuestInfo quest = _questsContainer.quests[qIdx];
            quest.questInfoState = QuestInfoState.IsCompleted;
            _questsContainer.quests[qIdx] = quest;

            // Освобождаем слот у NPC
            PlayerNPCRelation relation = _questsContainer.GetRelation(quest.giverNPC_ID);
            if (relation.npcID != null)
            {
                relation.curActiveQuests = Mathf.Max(0, relation.curActiveQuests - 1);

                if (relation.maxTrustLevel > relation.curTrustLevel + 1)
                    relation.curTrustLevel++;

                _questsContainer.SetRelation(relation);
            }

            Debug.Log($"[Система] Квест '{quest.questID}' от {quest.giverNPC_ID} закончен.");
        }
    }
}