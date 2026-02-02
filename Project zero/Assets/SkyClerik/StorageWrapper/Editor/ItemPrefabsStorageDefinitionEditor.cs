#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SkyClerik.Inventory.Editor
{
    [CustomEditor(typeof(ItemPrefabsStorageDefinition))]
    public class ItemPrefabsStorageDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty _prefabMappingsProperty;
        private GUIStyle _dirtyButtonStyle;
        private GUIStyle _cleanButtonStyle;

        private void OnEnable()
        {
            _prefabMappingsProperty = serializedObject.FindProperty("_prefabMappings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var storage = (ItemPrefabsStorageDefinition)target;
            InitializeStyles();

            EditorGUILayout.PropertyField(_prefabMappingsProperty, true);

            bool isDirty = CheckIfDirty(storage);
            GUIStyle currentStyle = isDirty ? _dirtyButtonStyle : _cleanButtonStyle;
            string buttonText = isDirty ? "ОБНОВИТЬ ID (ТРЕБУЕТСЯ!)" : "IDs Актуальны (Обновить)";

            EditorGUILayout.Space(10);

            if (GUILayout.Button(buttonText, currentStyle))
            {
                Undo.RecordObject(storage, "Assign Prefab Indexes as IDs");
                storage.AssignIndexesToIDs();
                EditorUtility.SetDirty(storage);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool CheckIfDirty(ItemPrefabsStorageDefinition storage)
        {
            // Используем SerializedObject для доступа к данным, чтобы избежать прямого доступа через рефлексию
            // и корректно работать с UI редактора.
            for (int i = 0; i < _prefabMappingsProperty.arraySize; i++)
            {
                var mappingProperty = _prefabMappingsProperty.GetArrayElementAtIndex(i);
                var idProperty = mappingProperty.FindPropertyRelative("_itemID");

                if (idProperty.intValue != i)
                {
                    return true; // Если ID не совпадает с индексом
                }
            }

            return false; // Все ID на своих местах
        }

        private void InitializeStyles()
        {
            if (_dirtyButtonStyle == null)
            {
                _dirtyButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f, 1f)) },
                    hover = { background = MakeTex(2, 2, new Color(0.9f, 0.3f, 0.3f, 1f)) },
                    active = { background = MakeTex(2, 2, new Color(0.7f, 0.1f, 0.1f, 1f)) },
                    fontStyle = FontStyle.Bold,
                    fontSize = 12,
                    fixedHeight = 30
                };
            }

            if (_cleanButtonStyle == null)
            {
                _cleanButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.2f, 0.6f, 0.2f, 1f)) },
                    hover = { background = MakeTex(2, 2, new Color(0.3f, 0.7f, 0.3f, 1f)) },
                    active = { background = MakeTex(2, 2, new Color(0.1f, 0.5f, 0.1f, 1f)) },
                    fontStyle = FontStyle.Normal,
                    fontSize = 12,
                    fixedHeight = 30
                };
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
#endif
