using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.DataEditor
{
    public class TypeSelectionWindow : EditorWindow
    {
        private Action<Type> _onTypeSelected;
        private Type _baseType;

        public static void ShowWindow(Type baseType, Action<Type> onTypeSelected)
        {
            var window = GetWindow<TypeSelectionWindow>("Выберите Тип");
            window.Initialize(baseType, onTypeSelected);
        }

        public void Initialize(Type baseType, Action<Type> onTypeSelected)
        {
            _baseType = baseType;
            _onTypeSelected = onTypeSelected;
            minSize = new Vector2(300, 400);
            BuildUI();
        }

        private void CreateGUI()
        {
            // The UI is now built by calling Initialize directly.
            // This method is still required by EditorWindow.
        }

        private void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();

            if (_baseType == null)
            {
                root.Add(new Label("Ошибка: базовый тип не был установлен."));
                return;
            }

            var types = TypeCache.GetTypesDerivedFrom(_baseType)
                .Where(t => !t.IsAbstract)
                .ToList();

            var listView = new ListView(types, 20,
                () => new Label(),
                (element, index) =>
                {
                    var label = element as Label;
                    label.text = types[index].Name;
                    label.style.paddingLeft = 5;
                })
            {
                selectionType = SelectionType.Single
            };

            var selectButton = new Button(() =>
            {
                if (listView.selectedIndex >= 0 && listView.selectedIndex < types.Count)
                {
                    _onTypeSelected?.Invoke(types[listView.selectedIndex]);
                    Close();
                }
            })
            {
                text = "Выбрать"
            };

            root.Add(new Label($"Доступные типы для '{_baseType.Name}':")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, paddingBottom = 5 }
            });
            root.Add(listView);
            root.Add(selectButton);
        }
    }
}
