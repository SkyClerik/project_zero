using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace SkyClerik
{
    /// <summary>
    /// Создает эффект "глитча" в виде цветных полупрозрачных полос поверх UI.
    /// Эффект может быть ограничен границами указанных элементов.
    /// </summary>
    public class ColorGlitchController : MonoBehaviour
    {
        [Header("Основные настройки")]
        [Tooltip("Как часто (в мс) в среднем будет появляться новая полоса глитча.")]
        public float glitchFrequencyMs = 200f;

        [Tooltip("Максимальная высота глитч-полосы в пикселях.")]
        public int maxGlitchHeight = 150;
        
        [Header("Настройки цветов")]
        [Tooltip("Если включено, используются цвета из списка 'Custom Glitch Colors'.")]
        public bool useCustomColors = false;

        [Tooltip("Список пользовательских цветов для глитчей.")]
        public List<Color> customGlitchColors = new List<Color>();

        [Tooltip("Прозрачность глитч-полос (от 0 до 1).")]
        [Range(0f, 1f)]
        public float glitchOpacity = 0.3f;

        [Header("Целевые элементы")]
        [Tooltip("Список имен VisualElement, в границах которых будет происходить эффект. Если пусто, эффект не будет работать.")]
        public List<string> targetElementNames = new List<string>();

        private UIDocument _uiDocument;
        private VisualElement _root;
        private List<VisualElement> _targetElements;
        private bool _isDestroyed = false;

        private readonly Color[] _defaultGlitchColors = {
            new Color(1, 0, 0), // Red
            new Color(0, 1, 0), // Green
            new Color(0, 0, 1)  // Blue
        };

        void Start()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("ColorGlitchController: Компонент UIDocument не найден!");
                return;
            }
            _root = _uiDocument.rootVisualElement;

            InitializeTargetElements();

            if (_targetElements == null || _targetElements.Count == 0)
            {
                Debug.LogWarning("ColorGlitchController: Список целевых элементов пуст или элементы не найдены. Эффект не будет запущен.");
                return;
            }

            ScheduleNextGlitch();
        }

        private void InitializeTargetElements()
        {
            _targetElements = new List<VisualElement>();
            if (targetElementNames == null || targetElementNames.Count == 0)
            {
                return;
            }

            foreach (var name in targetElementNames)
            {
                if (string.IsNullOrEmpty(name)) continue;
                
                var element = _root.Q<VisualElement>(name);
                if (element != null)
                {
                    // Это критически важно, чтобы глитчи не вылезали за границы родителя.
                    element.style.overflow = Overflow.Hidden;
                    _targetElements.Add(element);
                }
                else
                {
                    Debug.LogWarning($"ColorGlitchController: Элемент с именем '{name}' не найден.");
                }
            }
        }

        private void ScheduleNextGlitch()
        {
            long randomInterval = (long)(glitchFrequencyMs * Random.Range(0.5f, 1.5f));
            _root.schedule.Execute(TriggerGlitch).ExecuteLater(randomInterval);
        }

        private void TriggerGlitch()
        {
            if (_isDestroyed || _targetElements.Count == 0) return;

            // Выбираем случайный контейнер из списка целей
            VisualElement targetContainer = _targetElements[Random.Range(0, _targetElements.Count)];

            var glitchBlock = new VisualElement();
            
            float containerHeight = targetContainer.resolvedStyle.height;
            if (containerHeight <= 0) // Не создавать глитч в невидимом элементе
            {
                ScheduleNextGlitch();
                return;
            }
            
            float glitchHeight = Random.Range(10, maxGlitchHeight);
            float randomY = Random.Range(0, containerHeight - glitchHeight);

            // Определяем, какой список цветов использовать
            Color baseColor;
            if (useCustomColors && customGlitchColors != null && customGlitchColors.Count > 0)
            {
                baseColor = customGlitchColors[Random.Range(0, customGlitchColors.Count)];
            }
            else
            {
                baseColor = _defaultGlitchColors[Random.Range(0, _defaultGlitchColors.Length)];
            }
            baseColor.a = glitchOpacity; // Применяем настраиваемую прозрачность

            glitchBlock.style.position = Position.Absolute;
            glitchBlock.style.left = 0;
            glitchBlock.style.top = randomY;
            glitchBlock.style.width = new Length(100, LengthUnit.Percent);
            glitchBlock.style.height = glitchHeight;
            glitchBlock.style.backgroundColor = baseColor;
            glitchBlock.pickingMode = PickingMode.Ignore;

            targetContainer.Add(glitchBlock);

            glitchBlock.schedule.Execute(() => glitchBlock.RemoveFromHierarchy()).ExecuteLater(Random.Range(50, 150));

            ScheduleNextGlitch();
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }
    }
}
