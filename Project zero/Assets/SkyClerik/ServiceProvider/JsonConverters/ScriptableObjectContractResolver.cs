using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Пользовательский ContractResolver для Newtonsoft.Json, который обеспечивает
    /// правильное создание экземпляров ScriptableObject в Unity с использованием ScriptableObject.CreateInstance().
    /// </summary>
    public class ScriptableObjectContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);

            // Если тип является ScriptableObject и не является абстрактным,
            // устанавливаем DefaultCreator для использования ScriptableObject.CreateInstance
            // ScriptableObject.CreateInstance не может создавать экземпляры абстрактных классов.
            if (typeof(ScriptableObject).IsAssignableFrom(objectType) && !objectType.IsAbstract)
            {
                contract.DefaultCreator = () => ScriptableObject.CreateInstance(objectType);
            }

            return contract;
        }
    }
}
