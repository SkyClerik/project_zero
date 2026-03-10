using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik
{
    [CreateAssetMenu(fileName = "PrefabPainterDefinition", menuName = "SkyClerik/Editor/PrefabPainterDefinition", order = 0)]
    public class PrefabPainterDefinition : ScriptableObject
    {
        [SerializeField]
        private float _iconButtonSize = 48;
        public float IconButtonSize => _iconButtonSize;

        [SerializeField]
        private float _previewButtonSize = 96;
        public float PreviewButtonSize => _previewButtonSize;

        [SerializeField]
        private string _titleGuiContent = "Prefab Painter v. DEV";
        public string TitleGuiContent => _titleGuiContent;


        [SerializeField]
        private Color _blackA = new Color(0, 0, 0, 0.1f);
        public StyleColor BlackA => new StyleColor(_blackA);


        [Header("ICONS")]

        [SerializeField]
        private CastomGuiContent _dir;
        public CastomGuiContent Dir => _dir;

    }

    [System.Serializable]
    public class CastomGuiContent 
    {
        public string name;
        public string tooltip;
        public Sprite sprite;
    }
}
