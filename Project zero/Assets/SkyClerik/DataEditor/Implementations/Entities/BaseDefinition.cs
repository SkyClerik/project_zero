using Newtonsoft.Json;
using System;

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
        [Tooltip("Уникальный идентификатор. Может быть числом или строкой.")]
        protected string _id;

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
        protected Sprite _icon;

        /// <summary>
        /// Уникальный идентификатор определения.
        /// </summary>
        public string ID { get => _id; set => _id = value; } // Изменено на public set

        /// <summary>
        /// Генерирует новый уникальный ID для этого определения.
        /// Вызывается из контекстного меню в Инспекторе.
        /// </summary>
        [ContextMenu("Regenerate ID")]
        public void RegenerateID()
        {
            ID = Guid.NewGuid().ToString(); // Используем сеттер
            Debug.Log($"[BaseDefinition] ID для '{DefinitionName}' пересоздан: {ID}");
        }

        /// <summary>
        /// Имя определения, отображаемое в игре.
        /// </summary>
        public string DefinitionName { get => _definitionName; set => _definitionName = value; } // Изменено на public set

        /// <summary>
        /// Подробное описание определения.
        /// </summary>
        public string Description { get => _description; set => _description = value; } // Изменено на public set

        /// <summary>
        /// Иконка определения.
        /// </summary>
        public Sprite Icon { get => _icon; set => _icon = value; } // Изменено на public set

        /// <summary>
        /// Вызывается при загрузке объекта.
        /// Гарантирует, что у объекта есть ID и имя.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(_id))
                _id = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(_definitionName))
                _definitionName = name;
        }

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
