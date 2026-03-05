using System;

namespace SkyClerik
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NpcQuestEnumAttribute : Attribute
    {
        public Type EnumType { get; }

        public NpcQuestEnumAttribute(Type enumType)
        {
            EnumType = enumType;
        }
    }
}
