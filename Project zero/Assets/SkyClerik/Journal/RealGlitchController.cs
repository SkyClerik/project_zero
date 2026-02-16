using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace SkyClerik
{
    /// <summary>
    /// Создает эффект "настоящего" глитча, рендеря UI на текстуру и сдвигая ее части.
    /// </summary>
    public class RealGlitchController : MonoBehaviour
    {
        [Tooltip("Файл настроек панели, который использует этот UI. Нужен для получения разрешения.")]
        public PanelSettings panelSettings;

        [Tooltip("Как часто (в мс) может происходить глитч.")]
        public float glitchFrequencyMs = 500f;
        
        [Tooltip("Насколько сильным может быть глитч по горизонтали (в пикселях).")]
        public float glitchMagnitude = 5f;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _glitchOverlay;
        private RenderTexture _renderTexture;
        private IVisualElementScheduledItem _glitchScheduler;

        void Start()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("RealGlitchController: Компонент UIDocument не найден!");
                return;
            }
            if (panelSettings == null)
            {
                Debug.LogError("RealGlitchController: Пожалуйста, назначьте PanelSettings в инспекторе!");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            SetupRenderTexture();
            
            // Создаем оверлей для глитчей
            _glitchOverlay = new VisualElement()
            {
                name = "GlitchOverlay",
                pickingMode = PickingMode.Ignore 
            };
            _glitchOverlay.style.position = Position.Absolute;
            _glitchOverlay.style.width = new Length(100, LengthUnit.Percent);
            _glitchOverlay.style.height = new Length(100, LengthUnit.Percent);
            _root.Add(_glitchOverlay);

            // Запускаем таймер глитчей
            _glitchScheduler = _root.schedule.Execute(TriggerGlitch).Every((long)glitchFrequencyMs);
        }

        private void SetupRenderTexture()
        {
            Vector2Int resolution = panelSettings.referenceResolution;
            
            // Создаем RenderTexture с разрешением из PanelSettings
            _renderTexture = new RenderTexture(resolution.x, resolution.y, 16, RenderTextureFormat.ARGB32);
            _renderTexture.Create();

            // Клонируем PanelSettings, чтобы не изменять исходный ассет
            var runtimePanelSettings = Instantiate(panelSettings);
            runtimePanelSettings.targetTexture = _renderTexture;
            _uiDocument.panelSettings = runtimePanelSettings;
            
            // Отображаем "захваченный" UI, устанавливая его как фон для корневого элемента
            _root.style.backgroundImage = Background.FromRenderTexture(_renderTexture);
        }

        private void TriggerGlitch()
        {
            var glitchBlock = new VisualElement();
            
            float screenHeight = panelSettings.referenceResolution.y;
            float glitchHeight = 100; // "сотню пикселей"
            float randomY = Random.Range(0, screenHeight - glitchHeight);
            float randomXShift = Random.Range(-glitchMagnitude, glitchMagnitude);

            glitchBlock.style.position = Position.Absolute;
            glitchBlock.style.left = 0;
            glitchBlock.style.top = randomY;
            glitchBlock.style.width = new Length(100, LengthUnit.Percent);
            glitchBlock.style.height = glitchHeight;
            
            // Используем нашу текстуру как фон
            glitchBlock.style.backgroundImage = Background.FromRenderTexture(_renderTexture);
            
            // Сдвигаем фон, чтобы создать иллюзию сдвига пикселей
            glitchBlock.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, randomXShift);
            glitchBlock.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top, -randomY);

            _glitchOverlay.Add(glitchBlock);

            // Убираем глитч через 5 кадров (около 83мс при 60fps)
            glitchBlock.schedule.Execute(() => glitchBlock.RemoveFromHierarchy()).ExecuteLater(83);
        }

        private void OnDestroy()
        {
            // Остановка таймера
            _glitchScheduler?.Pause();
            // Очистка RenderTexture
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                _renderTexture = null;
            }
        }
    }
}
