using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UnityEngine.DataEditor
{
    public enum StatType
    {
        [InspectorName("None (Нет)")]
        None = 0,

        // Основные характеристики
        [InspectorName("Strength (Сила)")]
        Strength = 1,
        [InspectorName("Defense (Защита)")]
        Defense = 2,
        [InspectorName("Agility (Ловкость)")]
        Agility = 3,
        [InspectorName("Intuition (Интуиция)")]
        Intuition = 4,
        [InspectorName("Vitality (Живучесть)")]
        Vitality = 5,

        // Ресурсные характеристики
        [InspectorName("Health (Здоровье)")]
        Health = 10,
        [InspectorName("MaxHealth (Макс. Здоровье)")]
        MaxHealth = 11,
        [InspectorName("Energy (Энергия)")]
        Energy = 12,
        [InspectorName("MaxEnergy (Макс. Энергия)")]
        MaxEnergy = 13,

        // RAID-специфичные/Продвинутые характеристики
        [InspectorName("Speed (Скорость)")]
        Speed = 20,
        [InspectorName("CritRate (Шанс крит. удара)")]
        CritRate = 21,
        [InspectorName("CritDamage (Крит. урон)")]
        CritDamage = 22,
        [InspectorName("Accuracy (Точность)")]
        Accuracy = 23,
        [InspectorName("Resistance (Сопротивление)")]
        Resistance = 24,

        // Дополнительные характеристики (из GameParameterType)
        [InspectorName("Magic Attack (Маг. Атака)")]
        MagicAttack = 30,
        [InspectorName("Magic Defense (Маг. Защита)")]
        MagicDefense = 31,
        [InspectorName("Luck (Удача)")]
        Luck = 32,
    }

    public struct StatDisplayInfo
    {
        public int CurrentHp;
        public int MaxHp;
        public int CurrentEnergy;
        public int MaxEnergy;
    }

    public enum StatModType
    {
        [InspectorName("Flat (Плоский)")]
        Flat = 0,
        [InspectorName("PercentAdd (Процентный, аддитивный)")]
        PercentAdd = 1,
        [InspectorName("PercentMult (Процентный, мультипликативный)")]
        PercentMult = 2
    }

    public class StatModifier
    {
        public readonly float Value;
        public readonly StatModType Type;
        public readonly int Order;
        public readonly object Source;

        public StatModifier(float value, StatModType type, int order, object source)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }
        public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }
    }

    [Serializable]
    public class Stat
    {
        public float BaseValue;

        [NonSerialized] private float _value;
        [NonSerialized] private bool _isDirty = true;

        protected readonly List<StatModifier> _statModifiers = new List<StatModifier>();
        public readonly ReadOnlyCollection<StatModifier> StatModifiers;

        public Stat()
        {
            StatModifiers = new ReadOnlyCollection<StatModifier>(_statModifiers);
        }

        // Copy constructor
        public Stat(Stat original) : this() // Call default constructor to initialize ReadOnlyCollection
        {
            BaseValue = original.BaseValue;
            // Modifiers are typically not copied this way, they are applied dynamically
            // _statModifiers.AddRange(original._statModifiers); // Only if modifiers should be copied
        }

        public virtual float Value
        {
            get
            {
                if (_isDirty)
                {
                    _value = CalculateFinalValue();
                    _isDirty = false;
                }
                return _value;
            }
        }

        public virtual void AddModifier(StatModifier mod)
        {
            _isDirty = true;
            _statModifiers.Add(mod);
        }

        public virtual bool RemoveModifier(StatModifier mod)
        {
            if (_statModifiers.Remove(mod))
            {
                _isDirty = true;
                return true;
            }
            return false;
        }

        public virtual bool RemoveAllModifiersFromSource(object source)
        {
            bool removed = false;
            for (int i = _statModifiers.Count - 1; i >= 0; i--)
            {
                if (_statModifiers[i].Source == source)
                {
                    _statModifiers.RemoveAt(i);
                    _isDirty = true;
                    removed = true;
                }
            }
            return removed;
        }

        protected virtual float CalculateFinalValue()
        {
            float finalValue = BaseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < _statModifiers.Count; i++)
            {
                StatModifier mod = _statModifiers[i];
                switch (mod.Type)
                {
                    case StatModType.Flat:
                        finalValue += mod.Value;
                        break;
                    case StatModType.PercentAdd:
                        sumPercentAdd += mod.Value;
                        break;
                    case StatModType.PercentMult:
                        finalValue *= 1 + sumPercentAdd;
                        break;
                }
            }

            if (sumPercentAdd != 0)
            {
                finalValue *= 1 + sumPercentAdd;
            }

            return (float)Math.Round(finalValue, 4);
        }
    }

    [Serializable]
    public class ResourceStat : Stat
    {
        [SerializeField] private float _currentValue;

        public ResourceStat() : base() { } // Default constructor

        // Copy constructor from ResourceStat
        public ResourceStat(ResourceStat original) : base(original)
        {
            _currentValue = original._currentValue;
        }

        // Copy constructor from base Stat (if you want to create a ResourceStat from a regular Stat)
        public ResourceStat(Stat original) : base(original)
        {
            // Initial current value to base value
            _currentValue = original.BaseValue;
        }

        [NonSerialized]
        public Action<float, float> OnValueChanged; // Current, Max

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                float clampedValue = Mathf.Clamp(value, 0, Value);
                if (Math.Abs(_currentValue - clampedValue) > float.Epsilon)
                {
                    _currentValue = clampedValue;
                    OnValueChanged?.Invoke(_currentValue, Value);
                }
            }
        }

        public override float Value // For ResourceStat, Value is MaxValue
        {
            get { return base.Value; }
        }

        public void SetToMax()
        {
            CurrentValue = Value;
        }
    }

    [Serializable]
    public class StatsContainer
    {
        [field: SerializeField] public ResourceStat Health { get; private set; } = new ResourceStat();
        [field: SerializeField] public ResourceStat Energy { get; private set; } = new ResourceStat();
        [field: SerializeField] public Stat Strength { get; private set; } = new Stat();
        [field: SerializeField] public Stat Defense { get; private set; } = new Stat();
        [field: SerializeField] public Stat Agility { get; private set; } = new Stat();
        [field: SerializeField] public Stat Intuition { get; private set; } = new Stat();
        [field: SerializeField] public Stat Vitality { get; private set; } = new Stat();

        [field: SerializeField] public Stat Speed { get; private set; } = new Stat();
        [field: SerializeField] public Stat CritRate { get; private set; } = new Stat();
        [field: SerializeField] public Stat CritDamage { get; private set; } = new Stat();
        [field: SerializeField] public Stat Accuracy { get; private set; } = new Stat();
        [field: SerializeField] public Stat Resistance { get; private set; } = new Stat();

        // New stats from GameParameterType
        [field: SerializeField] public Stat MagicAttack { get; private set; } = new Stat();
        [field: SerializeField] public Stat MagicDefense { get; private set; } = new Stat();
        [field: SerializeField] public Stat Luck { get; private set; } = new Stat();

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnEnergyChanged;

        public StatsContainer()
        {
            // Предоставляем разумные ненулевые значения по умолчанию. Их можно переопределить в Инспекторе.
            Health.BaseValue = 100;
            Energy.BaseValue = 50;
            Strength.BaseValue = 10;
            Defense.BaseValue = 5;
            Agility.BaseValue = 5;
            Intuition.BaseValue = 5;
            Vitality.BaseValue = 8;
            Speed.BaseValue = 100;
            CritRate.BaseValue = 15;
            CritDamage.BaseValue = 50;
            Accuracy.BaseValue = 0;
            Resistance.BaseValue = 0;

            // New stats default values
            MagicAttack.BaseValue = 10;
            MagicDefense.BaseValue = 5;
            Luck.BaseValue = 0;

            RegisterEventHandlers();
        }

        // Copy constructor
        public StatsContainer(StatsContainer original) : this() // Call default constructor for initial values
        {
            CopyFrom(original);
        }

        public void CopyFrom(StatsContainer original)
        {
            if (original == null) return;

            // Deep copy each stat
            Health = new ResourceStat(original.Health);
            Energy = new ResourceStat(original.Energy);
            Strength = new Stat(original.Strength);
            Defense = new Stat(original.Defense);
            Agility = new Stat(original.Agility);
            Intuition = new Stat(original.Intuition);
            Vitality = new Stat(original.Vitality);
            Speed = new Stat(original.Speed);
            CritRate = new Stat(original.CritRate);
            CritDamage = new Stat(original.CritDamage);
            Accuracy = new Stat(original.Accuracy);
            Resistance = new Stat(original.Resistance);

            // New stats copy
            MagicAttack = new Stat(original.MagicAttack);
            MagicDefense = new Stat(original.MagicDefense);
            Luck = new Stat(original.Luck);

            RegisterEventHandlers();
        }


        public void Initialize()
        {
            RegisterEventHandlers();

            Health.BaseValue = Vitality.Value * 10;
            Health.SetToMax();
            Energy.SetToMax();
        }

        private void RegisterEventHandlers()
        {
            // Clear existing listeners to prevent duplicates, especially after deserialization
            if (Health != null) Health.OnValueChanged = null;
            if (Energy != null) Energy.OnValueChanged = null;

            Health.OnValueChanged += (current, max) => OnHealthChanged?.Invoke(current, max);
            Energy.OnValueChanged += (current, max) => OnEnergyChanged?.Invoke(current, max);
        }

        public void ApplyDamage(float amount, ElementDefinition elementType = null)
        {
            float defenseBlock = Defense.Value * 0.5f;
            // TODO: Здесь может быть логика применения сопротивлений к elementType
            // Например:
            // float effectiveResistance = GetElementalResistance(elementType);
            // amount *= (1 - effectiveResistance);

            float finalDamage = Mathf.Max(0, amount - defenseBlock);

            Debug.Log($"Расчет урона: Финальный урон ({finalDamage}) = Входящий ({amount}) - Заблокировано ({defenseBlock}) Защитой {Defense.Value}. Элемент: {elementType}.");

            Health.CurrentValue -= finalDamage;
            Debug.Log($"Текущее HP: {Health.CurrentValue}");
        }

        public void Heal(float amount)
        {
            Health.CurrentValue += amount;
            Debug.Log($"Восстановлено {amount} здоровья. Текущее HP: {Health.CurrentValue}");
        }

        public void SpendEnergy(int cost)
        {
            Energy.CurrentValue -= cost;
        }

        public Stat GetStat(StatType statType)
        {
            switch (statType)
            {
                case StatType.Strength: return Strength;
                case StatType.Defense: return Defense;
                case StatType.Agility: return Agility;
                case StatType.Intuition: return Intuition;
                case StatType.Vitality: return Vitality;
                case StatType.Health: case StatType.MaxHealth: return Health;
                case StatType.Energy: case StatType.MaxEnergy: return Energy;
                case StatType.Speed: return Speed;
                case StatType.CritRate: return CritRate;
                case StatType.CritDamage: return CritDamage;
                case StatType.Accuracy: return Accuracy;
                case StatType.Resistance: return Resistance;
                // New stats
                case StatType.MagicAttack: return MagicAttack;
                case StatType.MagicDefense: return MagicDefense;
                case StatType.Luck: return Luck;
                default: return null;
            }
        }

        public float GetStatValue(StatType statType)
        {
            return GetStat(statType)?.Value ?? 0f;
        }

        public StatDisplayInfo GetDisplayInfo()
        {
            return new StatDisplayInfo
            {
                CurrentHp = Mathf.CeilToInt(Health.CurrentValue),
                MaxHp = Mathf.CeilToInt(Health.Value),
                CurrentEnergy = Mathf.CeilToInt(Energy.CurrentValue),
                MaxEnergy = Mathf.CeilToInt(Energy.Value)
            };
        }
    }
}
