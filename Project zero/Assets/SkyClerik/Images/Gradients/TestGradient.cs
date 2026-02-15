using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik
{
    public class TestGradient : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _document;

        private class TElement
        {
            private VisualElement _vElement;
            private Sprite _sprite128;
            private Sprite _sprite256;
            private Sprite _sprite512;
            private Sprite _sprite1024;

            public TElement(VisualElement vElement, Sprite _128, Sprite _256, Sprite _512, Sprite _1024)
            {
                _vElement = vElement;
                _sprite128 = _128;
                _sprite256 = _256;
                _sprite512 = _512;
                _sprite1024 = _1024;
            }

            public VisualElement VElement { get => _vElement; set => _vElement = value; }
            public Sprite Sprite128 { get => _sprite128; set => _sprite128 = value; }
            public Sprite Sprite256 { get => _sprite256; set => _sprite256 = value; }
            public Sprite Sprite512 { get => _sprite512; set => _sprite512 = value; }
            public Sprite Sprite1024 { get => _sprite1024; set => _sprite1024 = value; }
        }

        [SerializeField]
        private List<Sprite> _sprite128 = new List<Sprite>();

        [SerializeField]
        private List<Sprite> _sprite256 = new List<Sprite>();

        [SerializeField]
        private List<Sprite> _sprite512 = new List<Sprite>();

        [SerializeField]
        private List<Sprite> _sprite1024 = new List<Sprite>();

        private List<TElement> _elements = new List<TElement>();

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: false);
        }

        private void Start()
        {
            var visualElements = _document.rootVisualElement.Query("gradient").ToList();

            for (int i = 0; i < visualElements.Count; i++)
            {
                var e = new TElement(visualElements[i], _sprite128[i], _sprite256[i], _sprite512[i], _sprite1024[i]);
                _elements.Add(e);
            }
        }

        [ContextMenu("128")]
        public void Set128()
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                _elements[i].VElement.style.backgroundImage = new StyleBackground(_elements[i].Sprite128);
            }
        }

        [ContextMenu("256")]
        public void Set256()
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                _elements[i].VElement.style.backgroundImage = new StyleBackground(_elements[i].Sprite256);
            }
        }

        [ContextMenu("512")]
        public void Set512()
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                _elements[i].VElement.style.backgroundImage = new StyleBackground(_elements[i].Sprite512);
            }
        }

        [ContextMenu("1024")]
        public void Set1024()
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                _elements[i].VElement.style.backgroundImage = new StyleBackground(_elements[i].Sprite1024);
            }
        }
    }
}
