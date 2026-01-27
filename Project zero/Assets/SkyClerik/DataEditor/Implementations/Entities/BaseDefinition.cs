using System;

namespace UnityEngine.DataEditor
{
    /// <summary>
    /// АБСТРАКТНЫЙ базовый класс для всех определений в игре (навыков, предметов, юнитов и т.д.).
    /// Предоставляет общий набор данных, таких как ID, имя, описание и иконка,
    /// чтобы все производные классы имели единую структуру.
    /// </summary>
    public abstract class BaseDefinition : ScriptableObject
    {
        [Header("Базовая информация")]

        [SerializeField]
        [Tooltip("Уникальный идентификатор. Может быть числом или строкой.")]
        private string _id;

        [SerializeField]
        [Tooltip("Имя, отображаемое в игре. Используем другое имя, чтобы избежать конфликта с 'name' из ScriptableObject.")]
        private string _definitionName;

        [SerializeField, TextArea(3, 10)]
        [Tooltip("Подробное описание, которое может отображаться в UI.")]
        private string _description;

        [SerializeField]
        [Tooltip("Иконка для отображения в инвентаре, меню навыков и т.д.")]
        [DrawWithIconField]
        private Sprite _icon;

        /// <summary>
        /// Уникальный идентификатор определения.
        /// </summary>
        public string ID => _id;

        /// <summary>
        /// Имя определения, отображаемое в игре.
        /// </summary>
        public string DefinitionName => _definitionName;

        /// <summary>
        /// Подробное описание определения.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Иконка определения.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// Вызывается при загрузке объекта.
        /// Гарантирует, что у объекта есть ID и имя.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString();
                // TODO: Рассмотреть, следует ли раскомментировать эту строку, чтобы
                // автоматически присваивать DefinitionName имя ассета при создании.
                // _definitionName = name; // ScriptableObject.name
            }
            if (string.IsNullOrEmpty(_definitionName))
            {
                _definitionName = name;
            }
        }

        /// <summary>
        /// Устанавливает новое имя для определения.
        /// </summary>
        /// <param name="name">Новое имя.</param>
        public void SetDefinitionName(string name)
        {
            // TODO: Рассмотреть удаление этого публичного сеттера, если DefinitionName
            // должен быть неизменяемым после создания.
            // Если он необходим, убедиться, что его использование контролируется.
            _definitionName = name;
        }

        /// <summary>
        /// Возвращает строковое представление объекта, используя его игровое имя.
        /// </summary>
        /// <returns>Имя определения.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(_definitionName) ? name : _definitionName;
        }
    }
}
