namespace UnityEngine.DataEditor
{
    public abstract class SkillBaseDefinition : BaseDefinition
    {
        public abstract void Apply(IUnit unit, object source);
        public abstract void Remove(IUnit unit, object source);
        public abstract void OnTick(IUnit unit, object source, float deltaTime = 0f);

    }
}
