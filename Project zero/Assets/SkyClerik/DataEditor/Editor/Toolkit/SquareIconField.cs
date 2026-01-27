#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;

namespace UnityEditor.Toolbox
{
    public class SquareIconField : VisualElement
    {
        private readonly ObjectField _objectField;
        private readonly Image _previewImage;
        private EventCallback<ChangeEvent<UnityEngine.Object>> _changeEventCallback;

        public SquareIconField(float iconSize = 128f)
        {
            _objectField = new ObjectField("")
            {
                objectType = typeof(Sprite),
                allowSceneObjects = false
            };
            _objectField.labelElement.style.minWidth = 50;

            _previewImage = new Image
            {
                style = {
                    width = iconSize,
                    height = iconSize,
                    minWidth = iconSize,
                    minHeight = iconSize,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    unityBackgroundImageTintColor = Color.white,
                    backgroundSize = new BackgroundSize(BackgroundSizeType.Contain),
                    flexDirection = FlexDirection.ColumnReverse,
                    marginLeft = 5
                }
            };
            _previewImage.SetBorderWidth(1);
            _previewImage.SetBorderColor(Color.gray);

            style.flexDirection = FlexDirection.Row;
            Add(_previewImage);
            _previewImage.Add(_objectField);

            _changeEventCallback = evt => UpdatePreview(evt.newValue as Sprite);
            _objectField.RegisterValueChangedCallback(_changeEventCallback);

            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (_changeEventCallback != null && _objectField != null)
            {
                _objectField.UnregisterValueChangedCallback(_changeEventCallback);
            }
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        public void BindProperty(SerializedProperty property)
        {
            _objectField.BindProperty(property);
            UpdatePreview(property.objectReferenceValue as Sprite);
        }

        private void UpdatePreview(Sprite sprite)
        {
            if (sprite != null)
            {
                _previewImage.style.backgroundImage = new StyleBackground(sprite);
                _previewImage.style.unityBackgroundImageTintColor = Color.white;
            }
            else
            {
                _previewImage.style.backgroundImage = null;
                _previewImage.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            }
        }
    }
}
#endif
