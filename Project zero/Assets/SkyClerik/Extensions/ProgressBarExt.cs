using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik
{
    public class ProgressBarExt : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ProgressBarExt, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_value = new UxmlFloatAttributeDescription { name = "value", defaultValue = 25f };
            UxmlStringAttributeDescription m_title = new UxmlStringAttributeDescription { name = "title", defaultValue = "" };
            UxmlFloatAttributeDescription m_minValue = new UxmlFloatAttributeDescription { name = "min-value", defaultValue = 1f };
            UxmlFloatAttributeDescription m_maxValue = new UxmlFloatAttributeDescription { name = "max-value", defaultValue = 100f };
            UxmlColorAttributeDescription m_progressColor = new UxmlColorAttributeDescription { name = "progress-color", defaultValue = new Color(0.1f, 0.4f, 0.8f, 1f) };
            UxmlColorAttributeDescription m_backgroundColor = new UxmlColorAttributeDescription { name = "background-color", defaultValue = new Color(0.2f, 0.2f, 0.2f, 0.8f) };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as ProgressBarExt;

                ate.MinValue = m_minValue.GetValueFromBag(bag, cc);
                ate.MaxValue = m_maxValue.GetValueFromBag(bag, cc);
                ate.Value = m_value.GetValueFromBag(bag, cc);
                ate.Title = m_title.GetValueFromBag(bag, cc);
                ate.ProgressColor = m_progressColor.GetValueFromBag(bag, cc);
                ate.BackgroundColor = m_backgroundColor.GetValueFromBag(bag, cc);
            }
        }

        private readonly VisualElement _progressFill;
        private readonly VisualElement _background;
        private readonly Label _titleLabel;

        private float _value;
        private float _currentMinValue;
        private float _currentMaxValue;

        public event Action<float> OnValueChanged;

        public float Value
        {
            get => _value;
            set
            {
                float clampedValue = Mathf.Clamp(value, _currentMinValue, _currentMaxValue);
                if (Mathf.Approximately(_value, clampedValue)) return;

                _value = clampedValue;
                UpdateVisuals();
                OnValueChanged?.Invoke(_value);
            }
        }

        public float Percentage
        {
            get
            {
                if (Mathf.Approximately(_currentMaxValue, _currentMinValue)) return 0;
                return ((_value - _currentMinValue) / (_currentMaxValue - _currentMinValue)) * 100f;
            }
            set
            {
                float clampedPercentage = Mathf.Clamp(value, 0f, 100f);
                float calculatedValue = _currentMinValue + (clampedPercentage / 100f) * (_currentMaxValue - _currentMinValue);
                this.Value = calculatedValue;
            }
        }
        
        public string Title
        {
            get => _titleLabel.text;
            set => _titleLabel.text = value;
        }

        public float MinValue
        {
            get => _currentMinValue;
            set
            {
                _currentMinValue = Mathf.Min(value, _currentMaxValue);
                Value = _value; 
            }
        }

        public float MaxValue
        {
            get => _currentMaxValue;
            set
            {
                _currentMaxValue = Mathf.Max(value, _currentMinValue);
                Value = _value;
            }
        }

        public Color ProgressColor
        {
            get => _progressFill.style.backgroundColor.value;
            set => _progressFill.style.backgroundColor = value;
        }

        public Color BackgroundColor
        {
            get => _background.style.backgroundColor.value;
            set => _background.style.backgroundColor = value;
        }

        public ProgressBarExt()
        {
            pickingMode = PickingMode.Ignore;

            _currentMinValue = 25f;
            _currentMaxValue = 100f;

            var bar = new VisualElement { name = "custom-progress-bar" };
            hierarchy.Add(bar);

            _background = new VisualElement { name = "custom-progress-bar__background" };
            bar.Add(_background);

            _progressFill = new VisualElement { name = "custom-progress-bar__progress" };
            _background.hierarchy.Add(_progressFill);

            _titleLabel = new Label { name = "custom-progress-bar__title" };
            hierarchy.Add(_titleLabel);
            
            // Set default colors for when created via code
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            ProgressColor = new Color(0.1f, 0.4f, 0.8f, 1f);

            Value = _currentMinValue; 
        }

        private void UpdateVisuals()
        {
            if (Mathf.Approximately(_currentMaxValue, _currentMinValue))
            {
                _progressFill.style.width = new Length(0f, LengthUnit.Percent);
                return;
            }
            float normalizedValue = (_value - _currentMinValue) / (_currentMaxValue - _currentMinValue);
            _progressFill.style.width = new Length(normalizedValue * 100f, LengthUnit.Percent);
        }
    }
}
