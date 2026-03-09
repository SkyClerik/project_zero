using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    public enum QuestInfoState : byte
    {
        [InspectorName("IsIdle (Ожидает)")]
        IsIdle = 0,
        [InspectorName("IsAccepted (Принят)")]
        IsAccepted = 10,
        [InspectorName("IsCompleted (Завершен)")]
        IsCompleted = 20,
        [InspectorName("IsFailed (Провален)")]
        IsFailed = 30,
        [InspectorName("IsNoValid (Не действителен)")]
        IsNoValid = 40,
    }

    [Serializable]
    public struct QuestKey
    {
        [SerializeField]
        [ReadOnly]
        private byte _value;

        public byte Value => _value;

        public QuestKey(byte value)
        {
            _value = value;
        }
        public static QuestKey FromEnum<TEnum>(TEnum e) where TEnum : Enum => new QuestKey(Convert.ToByte(e));
    }

    [System.Serializable]
    public class TargetNpc
    {
        [SerializeField]
        [ReadOnly]
        private string _id;
        [Tooltip("Уникальный ключ цели")]
        [SerializeField]
        private NpcID _targetNpcId;
        public NpcID TargetNpcId => _targetNpcId;
        [Tooltip("Добавляемое доверие")]
        [SerializeField]
        private int _addedTrust;
        public int AddedTrust => _addedTrust;

        public void ValidateID()
        {
            _id = TargetNpcId.ToString();
        }
    }

    [System.Serializable]
    public class ItemReward
    {
        [SerializeField]
        [ReadOnly]
        [Tooltip("Уникальный ключ")]
        private int _itemId;
        [Tooltip("Только для добавления предметов (это уменшит возможность опечаток)")]
        [SerializeField]
        private ItemBaseDefinition _item;

        public int ItemId => _itemId;
        public ItemBaseDefinition Item => _item;

        public void ValidateID()
        {
            if (_item == null)
                return;

            _itemId = _item.ID;
        }
    }

    [Serializable]
    public class QuestInfo
    {
        [ReadOnly]
        [SerializeField]
        private string _elementName = "NpcConfigBase";
        public string ElementName => _elementName;

        [Header("SETTINGS")]

        [Tooltip("Уникальный ключ (byte, но с обёрткой)")]
        [SerializeField]
        private QuestKey _questKey;
        public QuestKey QuestKey => _questKey;
        [Tooltip("Необходимый уровень доверия что бы задание стало доступно")]
        [SerializeField]
        private int _needTrustLevel;
        public int needTrustLevel => _needTrustLevel;
        [Tooltip("Текущее состояние задания.")]
        [SerializeField]
        private QuestInfoState _questInfoState;
        public QuestInfoState QuestInfoState { get => _questInfoState; set => _questInfoState = value; }

        [Header("TEXT")]

        [Tooltip("Текст задания")]
        [SerializeField]
        private TextKeyContainer _textKeyDescription = new TextKeyContainer();
        public string QuestText => _textKeyDescription.GetValue;

        [Header("RESULT")]

        [Tooltip("Добавляемое бабло")]
        [SerializeField]
        private int _addedMoney;
        public int AddedMoney => _addedMoney;

        [Tooltip("Предметы за выполнение задания)")]
        [SerializeField]
        private List<ItemReward> _rewardItems = new List<ItemReward>();
        public List<ItemReward> RewardItems => _rewardItems;

        [Tooltip("Изменение доверия в последствии для конкретных NPC")]
        [SerializeField]
        private List<TargetNpc> _targetNpcs = new List<TargetNpc>();
        public List<TargetNpc> TargetNpcs => _targetNpcs;

        public void Validate()
        {
            ValidateResultQuests();
            ValidateItemRewards();

            void ValidateResultQuests()
            {
                foreach (TargetNpc result in _targetNpcs)
                    result.ValidateID();
            }

            void ValidateItemRewards()
            {
                foreach (var item in _rewardItems)
                    item.ValidateID();
            }
        }
    }
}
