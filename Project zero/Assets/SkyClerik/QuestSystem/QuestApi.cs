using System;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    [RequireComponent(typeof(Quests))]
    public class QuestAPI : MonoBehaviour
    {
        private Quests _questsContainer;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            if (_questsContainer == null)
            {
                if (this.TryGetComponentInChildren(out _questsContainer, includeInactive: false) == false)
                {
                    _questsContainer = ServiceProvider.Get<Quests>();
                }
            }
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        // Взял задание
        public event Action<QuestInfo, NpcConfigBase> OnQuestAccept;
        // Закончил задание
        public event Action<QuestInfo, NpcConfigBase> OnQuestComplate;

        public void RiseQuestAccept(QuestInfo questInfo, NpcConfigBase npcConfigBase)
        {
            OnQuestAccept?.Invoke(questInfo, npcConfigBase);
        }

        public void RiseQuestComplate(QuestInfo questInfo, NpcConfigBase npcConfigBase)
        {
            OnQuestComplate?.Invoke(questInfo, npcConfigBase);
        }

        /// <summary>
        /// Системный механизм получения ссылки на NPC
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NpcConfigBase Npc(NpcID id)
        {
            return _questsContainer != null ? _questsContainer.Npc(id) : null;
        }

        /// <summary>
        /// Получить ссылку на NPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Npc<T>() where T : NpcConfigBase
        {
            if (_questsContainer == null)
                return null;

            foreach (var npc in _questsContainer.Npcs)
            {
                if (npc is T t)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Получить значение максимального числа квестов для любого NPC
        /// </summary>
        public int MaxActiveQuests => NpcConfigBase.MaxActiveQuests;
    }
}
