using System;

namespace UnityEngine.DataEditor
{ 
    [Serializable]
    public class DamageModifier : BaseModifier
    {
        [SerializeField]
        [Tooltip("Базовое количество урона, которое наносит этот модификатор.")]
        private float _damageAmount;

        [SerializeField]
        [Tooltip("Тип элемента, к которому относится урон (например, Огонь, Физический).")]
        private ElementDefinition _elementDefinition;

        public float DamageAmount => _damageAmount;
        public ElementDefinition ElementDefinition => _elementDefinition;

        public DamageModifier() 
        {
        }

        public DamageModifier(float damage, ElementDefinition elementDefinition)
        {
            _damageAmount = damage;
            _elementDefinition = elementDefinition;
            // NameKey, DescriptionKey, Icon унаследованы от EntityModifier и могут быть установлены здесь
        }

        public override void Apply(IUnit unit, object source)
        {
            if (unit == null || unit.Stats == null)
            {
                Debug.LogWarning($"DamageModifierData '{NameKey}': Невозможно применить модификатор. Юнит или StatsContainer недействительны.");
                return;
            }

            Debug.Log($"Модификатор '{NameKey}' наносит {_damageAmount} {_elementDefinition} урона юниту {unit.ToString()}.");
            unit.Stats.ApplyDamage(_damageAmount, _elementDefinition);
        }

        public override void Remove(IUnit unit, object source)
        {
            // Урон обычно не "удаляется", но если есть эффект "восстановление урона", то здесь будет он
            Debug.Log($"Модификатор '{NameKey}' удален (урон не откатывается) с юнита {unit.ToString()}.");
        }

        public override void OnTick(IUnit unit, object source, float deltaTime = 0f)
        {
            // Если урон должен наноситься периодически (DoT), то здесь будет логика
        }
    }
}