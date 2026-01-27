using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// Базовое определение для юнита (персонажа, врага), содержащее много данных описывающих юнита.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitBaseDefinition", menuName = "Definition/UnitBaseDefinition")]
    public class UnitBaseDefinition : BaseDefinition
    {
        [SerializeField]
        [Tooltip("Изображение юнита, используемое в бою.")]
        private Sprite _battleImage;
        /// <summary>
        /// Изображение юнита, используемое в бою.
        /// </summary>
        public Sprite BattleImage => _battleImage;

        [SerializeField]
        [Tooltip("Полное изображение юнита (например, для отображения в инвентаре или меню).")]
        private Sprite _fullImage;
        /// <summary>
        /// Полное изображение юнита (например, для отображения в инвентаре или меню).
        /// </summary>
        public Sprite FullImage => _fullImage;

        [SerializeField]
        [Tooltip("Префаб юнита для инстанцирования в игровом мире.")]
        private GameObject _prefab;
        /// <summary>
        /// Префаб юнита для инстанцирования в игровом мире.
        /// </summary>
        public GameObject Prefab => _prefab;

        [Header("Характеристики")]
        [SerializeField]
        [Tooltip("Базовые характеристики юнита.")]
        private StatsContainer _baseStats;
        /// <summary>
        /// Базовые характеристики юнита.
        /// </summary>
        public StatsContainer BaseStats => _baseStats;

        [Header("Класс юнита")]
        [SerializeField]
        [Tooltip("Класс, определяющий базовые характеристики и их рост, а также изучаемые навыки.")]
        private ClassDefinition _classDefinition;
        /// <summary>
        /// Класс, определяющий базовые характеристики и их рост, а также изучаемые навыки.
        /// </summary>
        public ClassDefinition ClassDefinition => _classDefinition;

        [Header("Врожденные особенности юнита")]
        [SerializeField, DisplayNameOverride("Уникальные Свойства Юнита")]
        [SerializeReference, SubclassSelector]
        [Tooltip("Список особенностей (Traits), которыми обладает юнит от рождения.")]
        private List<BaseModifier> _modifier = new List<BaseModifier>();
        /// <summary>
        /// Список свойств (Traits), которыми обладает юнит от рождения.
        /// </summary>
        public IReadOnlyList<BaseModifier> Modifier => _modifier;

        [Header("Навыки")]
        [SerializeField]
        [Tooltip("Список навыков (Skills), которыми обладает юнит от рождения.")]
        private List<SkillBaseDefinition> _skills = new List<SkillBaseDefinition>();
        /// <summary>
        /// Список навыков (Skills), которыми обладает юнит от рождения.
        /// </summary>
        public IReadOnlyList<SkillBaseDefinition> Skills => _skills;
    }
}