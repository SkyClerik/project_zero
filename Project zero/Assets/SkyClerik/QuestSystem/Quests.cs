using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public enum NpcID : byte
    {
        Unknown = 0,
        //[InspectorName("Просто Боб")]
        NPC_ID_BOB = 1,
        //[InspectorName("Коннор МакМанус из Бундока")]
        NPC_ID_CONNOR = 2,
    }

    [Serializable]
    public class Quests : MonoBehaviour
    {
        [SerializeReference, SubclassSelector]
        private List<NpcConfigBase> _npcs = new List<NpcConfigBase>();
        public IReadOnlyList<NpcConfigBase> Npcs => _npcs;

        private void OnValidate()
        {
            RemoveNewDuplicatesByNpcId();

            foreach (NpcConfigBase item in _npcs)
            {
                if (item == null)
                    continue;

                item.ElementName = $"{item.NpcID}";
                foreach (QuestInfo quest in item.Quests)
                    quest.Validate();
            }
        }

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }

        public NpcConfigBase Npc(NpcID npcID)
        {
            foreach (NpcConfigBase npc in _npcs)
            {
                if (npc != null && npc.NpcID == npcID)
                    return npc;
            }
            return null;
        }

        public List<NpcConfigBase> GetCurrentQuestNpc()
        {
            List<NpcConfigBase> result = new List<NpcConfigBase>();
            foreach (NpcConfigBase npc in _npcs)
            {
                foreach (var quest in npc.Quests)
                {
                    if (quest.QuestInfoState == QuestInfoState.IsAccepted)
                    {
                        result.Add(npc);
                    }
                }
            }
            return result;
        }

#if UNITY_EDITOR
        [ContextMenu("Quests / Clear missing SerializeReference NPCs")]
        private void ClearMissingNpcs()
        {
            if (_npcs == null || _npcs.Count == 0)
            {
                Debug.Log("[Quests] Список NPC пуст, чистить нечего.");
                return;
            }

            var log = new StringBuilder();
            int removedCount = 0;

            // идём с конца, чтобы RemoveAt не ломал индексы
            for (int i = _npcs.Count - 1; i >= 0; i--)
            {
                var npc = _npcs[i];

                // Для битого SerializeReference Unity даёт null, но элемент в списке остаётся.
                if (npc == null)
                {
                    removedCount++;
                    log.AppendLine($"  - Удалён NPC по индексу {i}: null (missing SerializeReference type)");
                    _npcs.RemoveAt(i);
                    continue;
                }

                // можно дополнительно проверить странные случаи, если нужно
            }

            if (removedCount == 0)
            {
                Debug.Log("[Quests] Битых ссылок SerializeReference не найдено.");
            }
            else
            {
                Debug.Log($"[Quests] Очищено битых NPC: {removedCount}\n{log}");
            }
        }
#endif

        private void RemoveNewDuplicatesByNpcId()
        {
            if (_npcs == null || _npcs.Count == 0)
                return;

            // Для каждого ID сначала собираем всех кандидатов
            var byId = new Dictionary<NpcID, List<NpcConfigBase>>();

            foreach (var npc in _npcs)
            {
                if (npc == null)
                    continue;

                var id = npc.NpcID;
                if (id == NpcID.Unknown)
                    continue;

                if (!byId.TryGetValue(id, out var list))
                {
                    list = new List<NpcConfigBase>();
                    byId.Add(id, list);
                }

                list.Add(npc);
            }

            // Теперь для каждого ID решаем, кого оставить
            foreach (var pair in byId)
            {
                var list = pair.Value;
                if (list.Count <= 1)
                    continue;

                // Разделяем на "старых" (есть квесты) и "новых" (квестов 0)
                var withQuests = new List<NpcConfigBase>();
                var withoutQuests = new List<NpcConfigBase>();

                foreach (var npc in list)
                {
                    if (HasQuests(npc))
                        withQuests.Add(npc);
                    else
                        withoutQuests.Add(npc);
                }

                // Кого удалять:
                // 1) Все без квестов, если есть хоть один с квестами.
                // 2) Если все без квестов (ни у кого нет квестов) — оставляем первый, остальные удаляем.
                var toRemove = new List<NpcConfigBase>();

                if (withQuests.Count > 0)
                {
                    Debug.LogWarning($"Никогда не меняй тип у уже реализованного элемента списка. Я не проверяю это кодом, за этим важно следить");
                    // есть хотя бы один "старый" — всё без квестов считаем новыми и вычищаем
                    toRemove.AddRange(withoutQuests);
                }
                else
                {
                    // у всех квестов 0 — выбираем первый как "основной", остальные считаем лишними
                    for (int i = 1; i < withoutQuests.Count; i++)
                        toRemove.Add(withoutQuests[i]);
                }

                // Физически удаляем из списка контейнера
                if (toRemove.Count > 0)
                {
                    for (int i = _npcs.Count - 1; i >= 0; i--)
                    {
                        if (toRemove.Contains(_npcs[i]))
                            _npcs.RemoveAt(i);
                    }
                }
            }

            bool HasQuests(NpcConfigBase npc)
            {
                var quests = npc.Quests;
                return quests != null && quests.Count > 0;
            }
        }

    }
}