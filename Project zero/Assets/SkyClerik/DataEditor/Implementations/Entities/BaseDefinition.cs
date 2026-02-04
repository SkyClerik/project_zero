using Newtonsoft.Json;
using UnityEngine.Toolbox;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// АБСТРАКТНЫЙ базовый класс для всех определений в игре (навыков, предметов, юнитов и т.д.).
    /// Предоставляет общий набор данных, таких как ID, имя, описание и иконка,
    /// чтобы все производные классы имели единую структуру.
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    public abstract class BaseDefinition : ScriptableObject
    {
        [Header("Базовая информация")]

        [JsonProperty]
        [SerializeField]
        [Tooltip("Уникальный идентификатор. Индекс полученный от общей базы предметов")]
        [ReadOnly]
        protected int _id;

        [JsonProperty]
        [SerializeField]
        [Tooltip("Имя, отображаемое в игре. Используем другое имя, чтобы избежать конфликта с 'name' из ScriptableObject.")]
        protected string _definitionName;

        [JsonProperty]
        [SerializeField, TextArea(3, 10)]
        [Tooltip("Подробное описание, которое может отображаться в UI.")]
        protected string _description;

        [SerializeField]
        [Tooltip("Иконка для отображения в инвентаре, меню навыков и т.д.")]
        [JsonIgnore]
        [DrawWithIconField]
        protected Sprite _icon;

        public int ID { get => _id; set => _id = value; }

        /// <summary>
        /// Имя определения, отображаемое в игре.
        /// </summary>
        public string DefinitionName { get => _definitionName; set => _definitionName = value; }

        /// <summary>
        /// Подробное описание определения.
        /// </summary>
        public string Description { get => _description; set => _description = value; }

        /// <summary>
        /// Иконка определения.
        /// </summary>
        public Sprite Icon { get => _icon; set => _icon = value; }

        /// <summary>
        /// Возвращает строковое представление объекта, используя его игровое имя.
        /// </summary>
        /// <returns>Имя определения.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(DefinitionName) ? name : DefinitionName;
        }
    }
}
