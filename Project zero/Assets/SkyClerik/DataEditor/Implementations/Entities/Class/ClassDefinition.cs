using System;
using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// Определение класса персонажа (Воин, Маг и т.д.).
    /// Определяет базовые характеристики, их рост по уровням, изучаемые навыки и доступное снаряжение.
    /// </summary>
    [CreateAssetMenu(fileName = "ClassDefinition", menuName = "SkyClerik/Definition/ClassDefinition")]
    public class ClassDefinition : BaseDefinition
    {
        [Header("Кривые роста характеристик")]
        [Tooltip("Список кривых роста для различных характеристик.")]
        [SerializeField]
        private List<StatGrowth> _statGrowths = new List<StatGrowth>();
        public IReadOnlyList<StatGrowth> StatGrowths => _statGrowths;

        [Header("Изучаемые навыки")]
        [Tooltip("Список навыков, изучаемых классом на определенных уровнях.")]
        [SerializeField]
        private List<LearnableSkill> _learnableSkills = new List<LearnableSkill>();
        public IReadOnlyList<LearnableSkill> LearnableSkills => _learnableSkills;

        //[Header("Доступные типы снаряжения")]
        //[Tooltip("Типы оружия, которые может использовать этот класс.")]
        //[SerializeField]
        //private List<WeaponTypeDefinition> _equippableWeaponTypes = new List<WeaponTypeDefinition>();
        //public IReadOnlyList<WeaponTypeDefinition> EquippableWeaponTypes => _equippableWeaponTypes;

        //[Tooltip("Типы брони, которые может использовать этот класс.")]
        //[SerializeField]
        //private List<ArmorTypeDefinition> _equippableArmorTypes = new List<ArmorTypeDefinition>();
        //public IReadOnlyList<ArmorTypeDefinition> EquippableArmorTypes => _equippableArmorTypes;

        [Header("Врожденные особенности класса")]
        [Tooltip("Список уникальных свойств (Unique Traits), которые дает этот класс.")]
        [field: SerializeField, DisplayNameOverride("Особенность Класса")]
        [field: SerializeReference]
        private List<BaseModifier> _modifier = new List<BaseModifier>();
        public IReadOnlyList<BaseModifier> Modifier => _modifier;

        [Header("Навыки класса")]
        [Tooltip("Список навыков, которые дает этот класс.")]
        [SerializeField, SerializeReference]
        private List<SkillBaseDefinition> _skills = new List<SkillBaseDefinition>();
        public IReadOnlyList<SkillBaseDefinition> Skills => _skills;


        [Header("Кривая опыта")]
        [Tooltip("Кривая, определяющая количество опыта, необходимое для перехода на следующий уровень.")]
        [SerializeField]
        private AnimationCurve _experienceCurve = new AnimationCurve();
        /// <summary>
        /// Кривая, определяющая количество опыта, необходимое для перехода на следующий уровень.
        /// </summary>
        public AnimationCurve ExperienceCurve => _experienceCurve;
        // TODO: Проверить, соответствует ли индексация AnimationCurve (0-based) системе уровней в игре (1-based).
        // Если уровни начинаются с 1, а кривая с 0, может потребоваться корректировка.

        // TODO: Добавить механизм для определения уникальных особенностей (Traits) класса.
        // List<TraitDefinition> (уникальные особенности класса)

        /// <summary>
        /// Возвращает значение характеристики для данного уровня, используя кривые роста.
        /// </summary>
        public float GetStatValueAtLevel(StatType statType, int level)
        {
            var growth = _statGrowths.Find(sg => sg.StatType == statType);
            if (growth != null && growth.GrowthCurve != null)
            {
                // Примечание: Уровень в игре обычно начинается с 1, тогда как AnimationCurve индексируется с 0.
                // Может потребоваться корректировка `level - 1` для точного соответствия.
                // TODO: Проверить и реализовать корректное соответствие индексации уровня и кривой.
                return growth.GrowthCurve.Evaluate(level);
            }
            return 0f; // Если кривая роста для данной характеристики не найдена
        }
        // TODO: Рассмотреть возможность перемещения этой логики в отдельный класс-сервис или менеджер,
        // так как класс определения должен быть максимально "чистым" от логики, содержа только данные.
    }

    /// <summary>
    /// Сериализуемая структура для определения кривой роста одной характеристики.
    /// </summary>
    [Serializable]
    public class StatGrowth
    {
        [SerializeField]
        [Tooltip("Тип характеристики, для которой определяется кривая роста.")]
        private StatType _statType;
        /// <summary>
        /// Тип характеристики, для которой определяется кривая роста.
        /// </summary>
        public StatType StatType => _statType;

        [SerializeField]
        [Tooltip("Кривая, определяющая значение характеристики на каждом уровне (X-ось: уровень, Y-ось: значение).")]
        private AnimationCurve _growthCurve;
        /// <summary>
        /// Кривая, определяющая значение характеристики на каждом уровне (X-ось: уровень, Y-ось: значение).
        /// </summary>
        public AnimationCurve GrowthCurve => _growthCurve;
        // TODO: Проверить, соответствует ли индексация AnimationCurve (0-based) системе уровней в игре (1-based).
        // Если уровни начинаются с 1, а кривая с 0, может потребоваться корректировка.
    }

    /// <summary>
    /// Сериализуемая структура для определения навыка, изучаемого на определенном уровне.
    /// </summary>
    [Serializable]
    public class LearnableSkill
    {
        [SerializeField]
        [Tooltip("Уровень, на котором изучается навык.")]
        private int _level;
        /// <summary>
        /// Уровень, на котором изучается навык.
        /// </summary>
        public int Level => _level;

        [SerializeField]
        [Tooltip("Навык, который изучается.")]
        private SkillBaseDefinition _skill;
        /// <summary>
        /// Навык, который изучается.
        /// </summary>
        public SkillBaseDefinition Skill => _skill;
    }
}

