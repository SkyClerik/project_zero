using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace SkyClerik
{
    /// <summary>
    /// Класс-профиль, описывающий один "рецепт" мерцания.
    /// </summary>
    [System.Serializable]
    public class FlickerProfile
    {
        [Tooltip("Просто имя для удобства организации в инспекторе.")]
        public string profileName = "Новый профиль мерцания";
        
        [Tooltip("Имена элементов (VisualElement), на которые будет действовать этот профиль.")]
        public List<string> targetElementNames = new List<string>();
        
        [Header("Настройки времени")]
        [Tooltip("Как долго (в мс) будет длиться одно мерцание для этого профиля.")]
        public long flickerDurationMs = 100;

        [Header("Настройки эффектов")]
        [Tooltip("Будет ли эффект влиять на фон?")]
        public bool affectBackground = true;
        [Tooltip("Список цветов для мерцания фона.")]
        public List<Color> backgroundColors = new List<Color>();
        
        [Tooltip("Будет ли эффект влиять на рамку?")]
        public bool affectBorder = true;
        [Tooltip("Список цветов для мерцания рамки.")]
        public List<Color> borderColors = new List<Color>();

        [Tooltip("Будет ли эффект влиять на текст? (Работает для Label, Button и т.д.)")]
        public bool affectText = true;
        [Tooltip("Список цветов для мерцания текста.")]
        public List<Color> textColors = new List<Color>();
    }

    /// <summary>
    /// Управляет эффектом "нестабильной голограммы" на основе набора профилей.
    /// </summary>
    public class HologramInstabilityController : MonoBehaviour
    {
        [Tooltip("Как часто (в мс) в среднем будет происходить одно случайное мерцание среди всех элементов.")]
        public long globalFlickerFrequencyMs = 200;

        [Tooltip("Список профилей мерцания. Можно создать несколько и настроить каждый индивидуально.")]
        public List<FlickerProfile> flickerProfiles = new List<FlickerProfile>();

        private UIDocument _uiDocument;
        private IVisualElementScheduledItem _flickerScheduler;
        private bool _isDestroyed;
        
        // Хранит все элементы, которые могут мерцать.
        private List<VisualElement> _allTargetElements = new List<VisualElement>();
        // Хранит "привязку" элемента к его профилю и его оригинальным стилям.
        private Dictionary<VisualElement, (FlickerProfile profile, Color originalBg, Color originalBorder, Color originalText)> _elementData = 
            new Dictionary<VisualElement, (FlickerProfile, Color, Color, Color)>();

        void Start()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("HologramInstabilityController: Компонент UIDocument не найден!");
                return;
            }
            var root = _uiDocument.rootVisualElement;

            // Обрабатываем каждый профиль
            foreach (var profile in flickerProfiles)
            {
                if (profile == null) continue;
                
                foreach (var elementName in profile.targetElementNames)
                {
                    if (string.IsNullOrEmpty(elementName)) continue;
                    
                    var element = root.Q<VisualElement>(elementName);
                    if (element != null)
                    {
                        if (_elementData.ContainsKey(element)) continue; // Элемент уже настроен другим профилем

                        _allTargetElements.Add(element);

                        // Сохраняем оригинальные стили
                        Color originalText = (element is TextElement te) ? te.resolvedStyle.color : Color.clear;
                        _elementData[element] = (profile, element.resolvedStyle.backgroundColor, element.resolvedStyle.borderBottomColor, originalText);
                    }
                    else
                    {
                        Debug.LogWarning($"HologramInstabilityController: Элемент с именем '{elementName}' не найден.");
                    }
                }
            }

            if (_allTargetElements.Count > 0)
            {
                _flickerScheduler = root.schedule.Execute(TriggerRandomFlicker).Every(globalFlickerFrequencyMs);
            }
        }

        private void TriggerRandomFlicker()
        {
            if (_isDestroyed || _allTargetElements.Count == 0) return;

            // Выбираем случайный элемент из всех доступных для мерцания
            VisualElement target = _allTargetElements[Random.Range(0, _allTargetElements.Count)];
            
            // Получаем его данные (профиль и оригинальные стили)
            var data = _elementData[target];
            FlickerProfile profile = data.profile;

            // Применяем мерцание на основе настроек профиля
            if (profile.affectBackground && profile.backgroundColors.Any())
                target.style.backgroundColor = profile.backgroundColors[Random.Range(0, profile.backgroundColors.Count)];
            
            if (profile.affectBorder && profile.borderColors.Any())
            {
                Color borderColor = profile.borderColors[Random.Range(0, profile.borderColors.Count)];
                target.style.borderTopColor = borderColor;
                target.style.borderBottomColor = borderColor;
                target.style.borderLeftColor = borderColor;
                target.style.borderRightColor = borderColor;
            }

            if (profile.affectText && target is TextElement textElement && profile.textColors.Any())
                textElement.style.color = profile.textColors[Random.Range(0, profile.textColors.Count)];

            // Планируем возврат к оригинальным стилям
            target.schedule.Execute(() => RevertFlicker(target, data)).ExecuteLater(profile.flickerDurationMs);
        }

        private void RevertFlicker(VisualElement target, (FlickerProfile profile, Color originalBg, Color originalBorder, Color originalText) data)
        {
            // Проверяем, не был ли объект уничтожен за время длительности мерцания
            if (_isDestroyed || target == null) return;
            
            // Возвращаем стили
            if (data.profile.affectBackground)
                target.style.backgroundColor = data.originalBg;

            if (data.profile.affectBorder)
            {
                target.style.borderTopColor = data.originalBorder;
                target.style.borderBottomColor = data.originalBorder;
                target.style.borderLeftColor = data.originalBorder;
                target.style.borderRightColor = data.originalBorder;
            }

            if (data.profile.affectText && target is TextElement textElement)
                textElement.style.color = data.originalText;
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
            _flickerScheduler?.Pause();
        }
    }
}
