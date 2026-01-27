using System;

namespace UnityEngine.DataEditor
{
    [Serializable]
    public class AddSkillModifier : BaseModifier
    {
        [SerializeField]
        [Tooltip("Навык, который будет добавлен юниту.")]
        private SkillBaseDefinition _skillToAdd;

        public SkillBaseDefinition SkillToAdd => _skillToAdd;

        public AddSkillModifier() 
        {
        }

        public AddSkillModifier(SkillBaseDefinition skill)
        {
            _skillToAdd = skill;
        }

        public override void Apply(IUnit unit, object source)
        {
            if (unit == null || _skillToAdd == null)
            {
                Debug.LogWarning($"AddSkillTraitData '{NameKey}': Невозможно применить модификатор. Юнит или навык для добавления недействительны.");
                return;
            }

            // Проверяем, есть ли уже этот навык у юнита
            if (!unit.Skills.Contains(_skillToAdd))
            {
                unit.Skills.Add(_skillToAdd);
                Debug.Log($"Навык '{_skillToAdd.Description}' добавлен юниту: {unit.ToString()}"); // Предполагаем, что SkillDefinition имеет DefinitionName
            }
            else
            {
                Debug.Log($"Навык '{_skillToAdd.Description}' уже есть у юнита. Юнит: {unit.ToString()}");
            }
        }

        public override void Remove(IUnit unit, object source)
        {
            if (unit == null || _skillToAdd == null)
            {
                Debug.LogWarning($"AddSkillTraitData '{NameKey}': Невозможно удалить модификатор. Юнит или навык для добавления недействительны.");
                return;
            }

            if (unit.Skills.Contains(_skillToAdd))
            {
                unit.Skills.Remove(_skillToAdd);
                Debug.Log($"Навык '{_skillToAdd.Description}' удален у юнита. Юнит: {unit.ToString()}.");
            }
            else
            {
                Debug.Log($"Навык '{_skillToAdd.Description}' отсутствует у юнита. Юнит: {unit.ToString()}.");
            }
        }

        public override void OnTick(IUnit unit, object source, float deltaTime = 0f)
        {
            // Навыки, добавляемые трейтом, обычно не имеют логики тиков.
        }
    }
}