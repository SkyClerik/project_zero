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

        [MenuItem("SkyClerik/Tools/DocumentDataExtractor")]
        public static void ShowWindow()
        {
            DocumentDataExtractor window = GetWindow<DocumentDataExtractor>();
            window.titleContent = new GUIContent("Document Data Extractor");
            window.minSize = new Vector2(350, 150);
        }

        public void CreateGUI()
        {
            // Create fields for user input
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

            var calculateButton = new Button(CalculateLayout)
            {
                text = "Расчитать"
            };

            // Add elements to the window's root
            rootVisualElement.Add(uxmlField);
            rootVisualElement.Add(panelSettingsField);
            rootVisualElement.Add(calculateButton);
        }

        private void CalculateLayout()
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

            // Create an in-memory representation of the UXML tree
            var rootElement = _uxmlAsset.CloneTree();
            Vector2 screenSize = _panelSettings.referenceResolution;
            
            // Set the root element's size to the reference screen size to start calculations
            rootElement.style.width = screenSize.x;
            rootElement.style.height = screenSize.y;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("--- Расчет размеров элементов UI ---");
            stringBuilder.AppendLine("| Имя элемента | Ширина (px) | Высота (px) |");
            stringBuilder.AppendLine("|--------------|-------------|-------------|");

            ProcessElement(rootElement, screenSize, 0, stringBuilder);

            Debug.Log(stringBuilder.ToString());
        }

        private void ProcessElement(VisualElement element, Vector2 parentSize, int depth, StringBuilder sb)
        {
            Vector2 calculatedSize = Vector2.zero;

            // Get the Length struct for width
            Length widthLength = element.style.width.value;

            // Calculate width
            if (element.style.width.keyword == StyleKeyword.Auto)
            {
                // In a real scenario, this would depend on content. We'll treat as parent size for this tool.
                calculatedSize.x = parentSize.x;
            }
            else if (widthLength.unit == LengthUnit.Pixel)
            {
                calculatedSize.x = widthLength.value;
            }
            else if (widthLength.unit == LengthUnit.Percent)
            {
                calculatedSize.x = parentSize.x * (widthLength.value / 100f);
            }

            // Get the Length struct for height
            Length heightLength = element.style.height.value;

            // Calculate height
            if (element.style.height.keyword == StyleKeyword.Auto)
            {
                calculatedSize.y = parentSize.y;
            }
            else if (heightLength.unit == LengthUnit.Pixel)
            {
                calculatedSize.y = heightLength.value;
            }
            else if (heightLength.unit == LengthUnit.Percent)
            {
                calculatedSize.y = parentSize.y * (heightLength.value / 100f);
            }
            
            // Append to string builder if the element has a name
            if (!string.IsNullOrEmpty(element.name))
            {
                string indent = new string(' ', depth * 2);
                sb.AppendLine($"| {indent}{element.name} | {calculatedSize.x:F2} | {calculatedSize.y:F2} |");
            }

            // Recurse through children
            foreach (var child in element.hierarchy.Children())
            {
                // Pass the newly calculated size of the current element as the parent size for its children
                ProcessElement(child, calculatedSize, depth + 1, sb);
            }
        }
    }
}
