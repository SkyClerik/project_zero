using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Text;

namespace SkyClerik.Editor
{
    public class DocumentDataExtractor : EditorWindow
    {
        private VisualTreeAsset _uxmlAsset;
        private PanelSettings _panelSettings;
        
        private VisualElement _calculationHost;
        private IVisualElementScheduledItem _pollingScheduler;

        [MenuItem("SkyClerik/Tools/DocumentDataExtractor")]
        public static void ShowWindow()
        {
            DocumentDataExtractor window = GetWindow<DocumentDataExtractor>();
            window.titleContent = new GUIContent("Document Data Extractor");
            window.minSize = new Vector2(350, 150);
        }

        public void CreateGUI()
        {
            var uxmlField = new ObjectField("UXML Document")
            {
                objectType = typeof(VisualTreeAsset),
                allowSceneObjects = false
            };
            uxmlField.RegisterValueChangedCallback(evt => _uxmlAsset = evt.newValue as VisualTreeAsset);

            var panelSettingsField = new ObjectField("Panel Settings")
            {
                objectType = typeof(PanelSettings),
                allowSceneObjects = false
            };
            panelSettingsField.RegisterValueChangedCallback(evt => _panelSettings = evt.newValue as PanelSettings);

            var calculateButton = new Button(StartLayoutCalculation)
            {
                text = "Расчитать"
            };

            rootVisualElement.Add(uxmlField);
            rootVisualElement.Add(panelSettingsField);
            rootVisualElement.Add(calculateButton);
        }

        private void StartLayoutCalculation()
        {
            if (_uxmlAsset == null)
            {
                Debug.LogError("DocumentDataExtractor: Пожалуйста, выберите UXML документ (VisualTreeAsset).");
                return;
            }
            if (_panelSettings == null)
            {
                Debug.LogError("DocumentDataExtractor: Пожалуйста, выберите файл настроек панели (PanelSettings).");
                return;
            }

            // If a previous calculation is somehow running, stop it.
            _pollingScheduler?.Pause();
            
            // Create a temporary, off-screen host for the UXML tree
            _calculationHost = new VisualElement();
            _uxmlAsset.CloneTree(_calculationHost);

            _calculationHost.style.width = _panelSettings.referenceResolution.x;
            _calculationHost.style.height = _panelSettings.referenceResolution.y;
            _calculationHost.style.position = Position.Absolute;
            _calculationHost.style.left = -10000; // Move it far off-screen

            rootVisualElement.Add(_calculationHost);

            // Start polling every 10ms to check if the layout engine has run.
            _pollingScheduler = rootVisualElement.schedule.Execute(CheckLayoutReady).Every(10);
        }

        private void CheckLayoutReady()
        {
            // Once the host element has a calculated layout width, we know the engine has started its work.
            if (_calculationHost == null || _calculationHost.layout.width <= 0)
            {
                return;
            }
            
            // Stop polling.
            _pollingScheduler?.Pause();
            
            // Per user suggestion, add a small extra delay to allow deeply nested elements
            // like ScrollView components to finalize their layout.
            rootVisualElement.schedule.Execute(ProcessAndLogLayout).ExecuteLater(150); // Increased delay
        }

        private void ProcessAndLogLayout()
        {
            if (_calculationHost == null) return;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"--- Расчет для разрешения: {_panelSettings.referenceResolution.x}x{_panelSettings.referenceResolution.y} ---");
            stringBuilder.AppendLine("| Имя элемента | X (px) | Y (px) | Ширина (px) | Высота (px) |");
            stringBuilder.AppendLine("|--------------|--------|--------|-------------|-------------|");

            // Get the root's absolute position to use as the origin (0,0) for all calculations.
            Vector2 rootOffset = _calculationHost.worldBound.position;
            ProcessElement(_calculationHost, rootOffset, 0, stringBuilder);

            Debug.Log(stringBuilder.ToString());

            // Clean up by removing the temporary host
            if (rootVisualElement.Contains(_calculationHost))
            {
                rootVisualElement.Remove(_calculationHost);
            }
            _calculationHost = null;
        }

        private void ProcessElement(VisualElement element, Vector2 rootOffset, int depth, StringBuilder sb)
        {
            // Read the final, computed world-space rect.
            Rect worldBound = element.worldBound;
            
            // Normalize the element's position by subtracting the root's offset to get coordinates relative to the root.
            float relativeX = worldBound.x - rootOffset.x;
            float relativeY = worldBound.y - rootOffset.y;

            if (!string.IsNullOrEmpty(element.name) && (worldBound.width > 0 || worldBound.height > 0))
            {
                // Indent based on depth in the hierarchy to create a tree view.
                string indent = new string(' ', depth * 2);
                sb.AppendLine($"| {indent}{element.name} | {relativeX:F0} | {relativeY:F0} | {worldBound.width:F0} | {worldBound.height:F0} |");
            }

            foreach (var child in element.hierarchy.Children())
            {
                // Pass the same rootOffset down to all children.
                ProcessElement(child, rootOffset, depth + 1, sb);
            }
        }
        
        private void OnDestroy()
        {
            // Ensure the scheduler is stopped when the window is closed.
            _pollingScheduler?.Pause();
        }
    }
}
