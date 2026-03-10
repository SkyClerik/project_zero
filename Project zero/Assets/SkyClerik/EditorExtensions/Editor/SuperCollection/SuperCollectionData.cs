using UnityEngine;

namespace SC
{
    [CreateAssetMenu(fileName = "SuperCollectionData", menuName = "SuperCollected/SuperCollectionData")]
    public class SuperCollectionData : ScriptableObject
    {
        [Header("Icons")]

        [SerializeField]
        public string _applicationDescription = "Тут будет описание приложения.";
        [SerializeField]
        public Texture2D _iconCollection;
        [SerializeField]
        public Texture2D _iconRoot;
        [SerializeField]
        public Texture2D _iconMerge;
        [SerializeField]
        public Texture2D _iconClearParent;
        [SerializeField]
        public Texture2D _iconRotate;
        [SerializeField]
        private Texture2D _iconGreed;
        [SerializeField]
        private float _rotateAnge = 90f;

        [SerializeField]
        private ERoundStep _roundStep = ERoundStep.Quarter;
        public enum ERoundStep
        {
            [InspectorName("One (1.0)")]
            One = 1,
            [InspectorName("Half (0.5)")]
            Half = 2,
            [InspectorName("Quarter (0.25)")]
            Quarter = 4,
            [InspectorName("Eighth (0.125)")]
            Eighth = 8,
            [InspectorName("Sixteenth (0.0625)")]
            Sixteenth = 16
        }

        public string applicationDescription => _applicationDescription;
        public Texture2D iconCollection => _iconCollection;
        public Texture2D iconRoot => _iconRoot;
        public Texture2D iconMerge => _iconMerge;
        public Texture2D iconClearParent => _iconClearParent;
        public Texture2D iconRotate => _iconRotate;
        public Texture2D iconGreed => _iconGreed;
        public float rotateAnge => _rotateAnge;
        public ERoundStep RoundStep => _roundStep;


        [Header("Grid")]

        [SerializeField]
        [Tooltip("Размер шага сетки по XZ")]
        private Vector2Int _gridOffset = new Vector2Int(2, 2);
        [SerializeField]
        [Tooltip("Сколько объектов в строке")]
        private int _gridStack = 5;

        public Vector2 GridOffset => _gridOffset;
        public int GridStack => _gridStack;

    }
}