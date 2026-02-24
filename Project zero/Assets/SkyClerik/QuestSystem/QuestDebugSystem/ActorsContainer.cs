using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyClerik
{
    public class ActorsContainer : MonoBehaviour
    {
        [Header("Статика")]
        public List<NPCConfig> npcConfigs = new List<NPCConfig>();

        [ContextMenu("Add Random NPC")]
        private void AddRandomNPC()
        {
            npcConfigs.Clear();
            npcConfigs.Add(new NPCConfig("NPC_01", "Торговец Джон", true));
            npcConfigs.Add(new NPCConfig("NPC_02", "Страж Ворон", true));
        }

        public NPCConfig GetNPCConfig(string npcID)
        {
            Debug.Log($"QuestsDatabase npcConfigs.count = {npcConfigs.Count}");
            foreach (NPCConfig npcConfig in npcConfigs)
            {
                Debug.Log($"npcConfig {npcConfig.npcID} == npcID {npcID}");
                if (npcConfig.npcID == npcID)
                    return npcConfig;
            }
            return default(NPCConfig);
        }
    }

    // --- КОНФИГУРАЦИЯ NPC (Статика) ---
    // Хранится в общем списке, загружается при старте (из JSON/XML/Excel)
    [Serializable]
    public struct NPCConfig
    {
        public string displayName;      // Имя
        public string npcID;            // Уникальный ключ
        public bool isQuestGiver;       // Выдает ли квесты

        public NPCConfig(string id, string name, bool questGiver)
        {
            displayName = name;
            npcID = id;
            isQuestGiver = questGiver;
        }
    }
}