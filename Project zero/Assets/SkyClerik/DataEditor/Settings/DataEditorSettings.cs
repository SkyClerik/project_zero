namespace UnityEngine.DataEditor
{
    [CreateAssetMenu(fileName = "DataEditorSettings", menuName = "SkyClerik/Game Data/Data Editor Settings")]
    public class DataEditorSettings : ScriptableObject
    {
        [Header("Пути к базам данных")]
        [Tooltip("Папка, где хранятся ассеты баз данных (например, SkillDatabase.asset)")]
        [SerializeField] private string _databaseAssetPath = "Assets/DataEditor/Databases";
        public string DatabaseAssetPath => _databaseAssetPath;

        [Header("Пути к определениям")]
        [Tooltip("Корневая папка, где хранятся ассеты определений (навыки, юниты и т.д.)")]
        [SerializeField] private string _dataEntityAssetPath = "Assets/DataEditor/DataEntity";
        public string DataEntityAssetPath => _dataEntityAssetPath;
    }
}
