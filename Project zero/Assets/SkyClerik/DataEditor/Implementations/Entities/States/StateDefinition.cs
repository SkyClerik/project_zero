using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "StateDefinition", menuName = "SkyClerik/Definition/State")]
    public class StateDefinition : StateBaseDefinition
    {
        [Header("Параметры Состояния")]
        [SerializeField]
        [Tooltip("Длительность состояния в ходах (0 = бесконечно).")]
        private int _durationTurns = 0;

        public int DurationTurns => _durationTurns;

        [Header("Влияние Состояния")]
        [Tooltip("Список особенностей (Traits), которые дает это состояние.")]
        [field: SerializeField, DisplayNameOverride("Уникальные Свойства Состояния")]
        [field: SerializeReference]
        private List<BaseModifier> _modifier = new List<BaseModifier>();
        public IReadOnlyList<BaseModifier> Modifier => _modifier;

        [Tooltip("Список навыков (Skills), которые дает это состояние.")]
        [SerializeField]
        private List<SkillBaseDefinition> _stateSkills = new List<SkillBaseDefinition>();
        public IReadOnlyList<SkillBaseDefinition> StateSkills => _stateSkills;

        // TODO: Добавить другие параметры состояния, например:
        // - Возможность складывания (stacking)
        // - Визуальные/звуковые эффекты
        // - Тип состояния (бафф/дебафф)

        public override void Apply(IUnit unit, object source)
        {
            // Логика применения состояния.
            // Например, наложение модификаторов характеристик, активация визуальных эффектов.
            // Активация трейтов и навыков, даваемых состоянием.
            Debug.Log($"Применено состояние '{DefinitionName}' от источника '{source}' к юниту '{unit}'.");

            // Apply unique traits from state
            if (_modifier != null)
            {
                foreach (BaseModifier modifier in _modifier)
                {
                    modifier.Apply(unit, this);
                }
            }

            if (_stateSkills != null)
            {
                foreach (SkillBaseDefinition skill in _stateSkills)
                {
                    skill.Apply(unit, this);
                }
            }
        }

        public override void Remove(IUnit unit, object source)
        {
            // Логика снятия состояния.
            // Например, удаление модификаторов характеристик, деактивация визуальных эффектов.
            // Деактивация трейтов и навыков, даваемых состоянием.
            Debug.Log($"Снято состояние '{name}' с юнита '{unit}'.");

            if (_modifier != null)
            {
                foreach (BaseModifier modifier in _modifier)
                {
                    modifier.Remove(unit, this);
                }
            }

            if (_stateSkills != null)
            {
                foreach (SkillBaseDefinition skill in _stateSkills)
                {
                    skill.Remove(unit, this);
                }
            }
        }

        public override void OnTick(IUnit unit, object source, float deltaTime = 0f)
        {
            // Логика, срабатывающая каждый тик состояния.
            // Например, уменьшение длительности, периодический урон/лечение.
        }
    }
}