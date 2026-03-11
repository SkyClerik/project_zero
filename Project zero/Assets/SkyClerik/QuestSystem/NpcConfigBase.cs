using SkyClerik.Inventory;
using SkyClerik.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public class QuestAcceptFailedInfo
    {
        /// <summary>
        /// Сколько не хватает до нужного значения
        /// </summary>
        public int TrustLackToMax;
        /// <summary>
        /// Текущее кол-во активных заданий
        /// </summary>
        public int CurActiveQuests;
        /// <summary>
        /// Максимум активных заданий
        /// </summary>
        public int MaxActiveQuests;

    }

    [Serializable]
    public abstract class NpcConfigBase
    {
        [ReadOnly]
        [SerializeField]
        private string _elementName = "NpcConfigBase";
        public string ElementName { get => _elementName;  set => _elementName = value; }

        [ReadOnly]
        [SerializeField]
        private string _actorName = "Test Name Key";
        public string ActorName => _actorName;

        [Tooltip("Я знаком")]
        [SerializeField]
        protected bool _isAcquainted;
        public bool IsAcquainted { get => _isAcquainted; set => _isAcquainted = value; }

        [Tooltip("Текущий уровень доверия")]
        [SerializeField]
        protected int _curTrustLevel;
        public int CurTrustLevel => _curTrustLevel;

        [Tooltip("Максимальный уровень доверия для этого конкретного NPC")]
        [SerializeField]
        protected int _maxTrustLevel = 10;
        public int MaxTrustLevel => _maxTrustLevel;

        [Tooltip("Текущее кол-во активных заданий")]
        [SerializeField]
        protected int _curActiveQuests;
        public int CurActiveQuests => _curActiveQuests;

        [Tooltip("Отображается ли в журнале")]
        [SerializeField]
        protected bool _isJournalVisible = true;
        public bool IsJournalVisible => _isJournalVisible;

        public abstract NpcID NpcID { get; }

        public const int MaxActiveQuests = 5;

        [SerializeField]
        protected List<QuestInfo> _quests = new List<QuestInfo>();
        public IReadOnlyList<QuestInfo> Quests => _quests;

        // Попробовать принять задание
        /// <summary>
        /// Попробовать принять задание. При неудаче вернет информацию в QuestAcceptFailedInfo
        /// </summary>
        /// <param name="key"></param>
        /// <param name="failedInfo"></param>
        /// <returns></returns>
        protected bool TryAcceptQuestInternal(QuestKey key, out QuestAcceptFailedInfo failedInfo)
        {
            failedInfo = new QuestAcceptFailedInfo();

            if (QuestExist(key, out QuestInfo questInfo))
            {
                if (_curTrustLevel < questInfo.needTrustLevel)
                {
                    failedInfo.TrustLackToMax = IntExt.LackToMax(_curTrustLevel, questInfo.needTrustLevel);
                    failedInfo.CurActiveQuests = _curActiveQuests;
                    failedInfo.MaxActiveQuests = MaxActiveQuests;
                    Debug.Log($"[Квест] KEY: {questInfo.QuestKey.Value} : '{questInfo.ElementName}' не принят от {NpcID} по нехватке доверия");
                    return false;
                }

                if (_curActiveQuests >= MaxActiveQuests)
                {
                    failedInfo.CurActiveQuests = _curActiveQuests;
                    failedInfo.MaxActiveQuests = MaxActiveQuests;
                    Debug.Log($"[Квест] KEY: {questInfo.QuestKey.Value} : '{questInfo.ElementName}' не принят от {NpcID} по лимиту заданий");
                    return false;
                }

                questInfo.QuestInfoState = QuestInfoState.IsAccepted;
                _curActiveQuests++;
                failedInfo.CurActiveQuests = _curActiveQuests;
                failedInfo.MaxActiveQuests = MaxActiveQuests;
                ServiceProvider.Get<QuestAPI>()?.RiseQuestAccept(questInfo, this);
                Debug.Log($"[Квест] KEY: {questInfo.QuestKey.Value} : '{questInfo.ElementName}' принят от {NpcID}.");
                return true;
            }
            Debug.LogError($"[Квест] KEY: {key.Value} вообще не найден! Проверь значение ключа, оно автоматически передается от конкретной реализации enum внутри класса персонажа но сломать все же наверное можно!!!");
            return false;
        }

        /// <summary>
        /// Проверить влезает ли список предметов в инвентарь. 
        /// </summary>
        /// <param name="questKey"></param>
        /// <returns></returns>
        protected bool IsWillRewardsFitInternal(QuestKey questKey)
        {
            if (QuestExist(questKey, out QuestInfo questInfo))
            {
                var storege = ServiceProvider.Get<GlobalItemStorage>();
                if (storege == null)
                {
                    Debug.Log($"Потерялось хранилище предметов");
                    return false;
                }

                var inventoryAPI = ServiceProvider.Get<InventoryAPI>();
                if (inventoryAPI == null)
                {
                    Debug.Log($"Потерялась связь с инвентарем игрока");
                    return false;
                }

                List<ItemBaseDefinition> itemsToTest = new List<ItemBaseDefinition>();
                foreach (var rewardItem in questInfo.RewardItems)
                    itemsToTest.Add(storege.GlobalItemsStorageDefinition.GetOriginalItem(rewardItem.ItemId));

                return inventoryAPI.CanFitItems(itemsToTest);
            }
            return false;
        }

        /// <summary>
        /// Получить список квестов которые можно взять на выполнение
        /// </summary>
        /// <param name="idleQuests"></param>
        /// <returns></returns>
        public bool TryGetIdleQuests(out List<QuestInfo> idleQuests)
        {
            idleQuests = new List<QuestInfo>();

            foreach (var quest in _quests)
            {
                if (quest.QuestInfoState == QuestInfoState.IsIdle)
                    idleQuests.Add(quest);
            }

            if (idleQuests.Count == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Получить список квестов сейчас в процессе выполнения
        /// </summary>
        /// <param name="acceptQuests"></param>
        /// <returns></returns>
        public bool TryGetAcceptedQuests(out List<QuestInfo> acceptQuests)
        {
            acceptQuests = new List<QuestInfo>();

            foreach (var quest in _quests)
            {
                if (quest.QuestInfoState == QuestInfoState.IsAccepted)
                    acceptQuests.Add(quest);
            }

            if (acceptQuests.Count == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Получить список квестов с конкретным указанием желаемого состояния
        /// </summary>
        /// <param name="acceptQuests"></param>
        /// <returns></returns>
        public bool TryGetQuestsInState(out List<QuestInfo> acceptQuests, QuestInfoState questInfoState)
        {
            acceptQuests = new List<QuestInfo>();

            foreach (var quest in _quests)
            {
                if (quest.QuestInfoState == questInfoState)
                    acceptQuests.Add(quest);
            }

            if (acceptQuests.Count == 0)
                return false;
            else
                return true;
        }

        // Завершить задание
        protected void CompleteQuestInternal(QuestKey key)
        {
            foreach (var quest in _quests)
            {
                if (quest.QuestKey.Value == key.Value)
                {
                    // Внутри цикла потому что если не найден вообще квест то и API искать не нужно
                    QuestAPI questAPI = ServiceProvider.Get<QuestAPI>();
                    var globalGameProperty = ServiceProvider.Get<GlobalBox>()?.GlobalGameProperty;

                    foreach (TargetNpc targetNpc in quest.TargetNpcs)
                    {
                        NpcConfigBase target = questAPI.Npc(targetNpc.TargetNpcId);

                        if (target == null)
                        {
                            Debug.LogError($"[Квест] Для квеста: '{quest.ElementName}' не найдена цель {targetNpc.TargetNpcId}");
                            return;
                        }
                        else
                            target.SetCurTrustLevel(targetNpc, quest);
                    }
                    // меняю QuestInfoState обязательно после SetCurTrustLevel, там проверка на активный (но наверное это излишне)
                    quest.QuestInfoState = QuestInfoState.IsCompleted;
                    globalGameProperty.PlayerMoney.Add(quest.AddedMoney);
                    questAPI.RiseQuestComplate(quest, this);

                    return;
                }
            }
        }

        // Базовый поиск по ключу
        private bool QuestExist(QuestKey key, out QuestInfo questInfo)
        {
            foreach (var quest in _quests)
            {
                //Debug.Log($"quest.QuestKey.Value: {quest.QuestKey.Value} == key.Value: {key.Value}");
                if (quest.QuestKey.Value == key.Value)
                {
                    questInfo = quest;
                    return true;
                }
            }
            questInfo = null;
            return false;
        }

        // Присвоение указанному npc доверия от завершенного задания
        public void SetCurTrustLevel(TargetNpc questResult, QuestInfo questInfo)
        {
            int newTrust = _curTrustLevel + questResult.AddedTrust;
            _curTrustLevel = Math.Max(0, Math.Min(int.MaxValue, newTrust));
            Debug.Log($"target {NpcID} addedTrust: {questResult.AddedTrust}");

            // Если принят на момент проверок
            if (questInfo.QuestInfoState == QuestInfoState.IsAccepted)
            {
                // Если доверие персонажа ноль то задание провалено
                if (_curTrustLevel == 0)
                {
                    Debug.Log($"[Система] Квест : '{questInfo.ElementName}' провален потому что понизилось доверие у NPC {NpcID}.");
                    questInfo.QuestInfoState = QuestInfoState.IsFailed;
                }
            }
        }
    }
}