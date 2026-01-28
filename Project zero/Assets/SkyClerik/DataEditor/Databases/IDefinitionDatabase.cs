using System.Collections.Generic;

namespace UnityEngine.DataEditor
{
    public interface IDefinitionDatabase<T> where T : BaseDefinition
    {
        IReadOnlyList<T> Items { get; }
    }
}
