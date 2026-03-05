using UnityEngine;

namespace SkyClerik
{
    [System.Serializable]
    public struct TextKeyContainer
    {
        [Tooltip("Ключ от текста с описанием задания")]
        [SerializeField]
        private string _key;

        [Tooltip("Текст для разработки")]
        [SerializeField]
        private string _debugText;

        public string GetValue
        {
            get
            {
                string src = string.IsNullOrEmpty(_key) ? _debugText : _key;
                if (!string.IsNullOrEmpty(src) && src.Length > 260)
                {
                    Debug.LogWarning($"[TextKeyContainer] Текст длиннее ({src.Length} > 260).");
                }
                return src;
            }
        }
    }
}