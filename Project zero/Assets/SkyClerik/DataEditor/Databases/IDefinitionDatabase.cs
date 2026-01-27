using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.DataEditor
{
    public interface IDefinitionDatabase<T> where T : BaseDefinition
    {
        IReadOnlyList<T> Items { get; }
    }
}
