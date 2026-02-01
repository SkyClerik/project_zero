using System;
using UnityEngine.Toolbox;
using Newtonsoft.Json;
using SkyClerik.Inventory;

namespace UnityEngine.DataEditor
{
    [Serializable]
    public abstract class BaseModifier
    {
        [ReadOnly]
        [SerializeField]
        [Tooltip("Уникальный идентификатор этого экземпляра модификатора. Генерируется автоматически при первом создании.")]
        private string _id = Guid.NewGuid().ToString();

        [SerializeField]
        [Tooltip("Ключ для локализации отображаемого имени модификатора.")]
        private string _nameKey;

        [SerializeField]
        [Tooltip("Ключ для локализации подробного описания модификатора.")]
        private string _descriptionKey;

        [SerializeField]
        [Tooltip("Иконка, представляющая модификатор в UI.")]
        [JsonConverter(typeof(SpriteJsonConverter))]
        private Sprite _icon;

        public string Id => _id;
        public string NameKey => _nameKey;
        public string DescriptionKey => _descriptionKey;
        public Sprite Icon => _icon;

        public abstract void Apply(IUnit unit, object source);
        public abstract void Remove(IUnit unit, object source);
        public abstract void OnTick(IUnit unit, object source, float deltaTime = 0f);
    }
}