using UnityEditor;
using UnityEngine;

namespace SkyClerik.Editor
{
    [CustomEditor(typeof(SkyClerik.Quests))]
    public class QuestsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // стандартный инспектор
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (GUILayout.Button("Открыть Quests Container"))
            {
                QuestsContainerWindow.Open();
            }
        }
    }
}
