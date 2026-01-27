namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "DamageSkillDefinition", menuName = "Definition/Skill/DamageSkillDefinition")]
    public class DamageSkillDefinition : SkillBaseDefinition
    {
        [Header("Параметры Навыка")]

        [field: SerializeField, Tooltip("Количество урона, наносимого этим навыком.")]
        private float damageAmount = 10f;
        [SerializeField]
        [Tooltip("Стоимость применения в мане или другом ресурсе.")]
        private int _cost = 0;
        [SerializeField]
        [Tooltip("Время перезарядки в ходах или секундах.")]
        private float _cooldown = 0f;
        [Header("Тип навыка")]
        [SerializeField]
        [Tooltip("Тип навыка (например, 'Магия', 'Техника'). Ссылка на GameTypeDefinition.")]
        [DrawWithDefinitionSelector(typeof(TypeDefinition))]
        private TypeDefinition _skillType;

        public TypeDefinition SkillType => _skillType;
        public int Cost => _cost;
        public float Cooldown => _cooldown;
        public float DamageAmount => damageAmount;

        public override void Apply(IUnit unit, object source)
        {
            Debug.Log($"Навык '\''{DefinitionName}'\'' нанес {DamageAmount} урона юниту '\''{unit}'\'' от источника '\''{source}'\''");
        }

        public override void Remove(IUnit unit, object source)
        {

        }

        public override void OnTick(IUnit unit, object source, float deltaTime = 0f)
        {

        }
    }
}