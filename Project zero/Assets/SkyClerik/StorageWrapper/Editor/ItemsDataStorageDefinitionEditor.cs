#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory.Editor
{
    /// <summary>
    /// Пользовательский редактор для <see cref="ItemsDataStorageDefinition"/>.
    /// Добавляет функциональность для автоматического обновления <c>WrapperIndex</c>
    /// у всех <see cref="ItemBaseDefinition"/> в списке на основе их позиции.
    /// </summary>
    [CustomEditor(typeof(ItemsDataStorageDefinition))]
    public class ItemsDataStorageDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty _baseDefinitionsProperty;
        private GUIStyle _dirtyButtonStyle;
        private GUIStyle _cleanButtonStyle;
        private int _lastKnownHash;

        private void OnEnable()
        {
            _baseDefinitionsProperty = serializedObject.FindProperty("_baseDefinitions");
            // Инициализируем хэш при активации инспектора, чтобы отслеживать изменения данных.
            // Это предотвращает ложное срабатывание "грязного" состояния при первом отображении инспектора.
            ItemsDataStorageDefinition storage = (ItemsDataStorageDefinition)target;
            _lastKnownHash = CalculateCurrentHash(storage);
        }

        /// <summary>
        /// Переопределяет метод отрисовки инспектора для <see cref="ItemsDataStorageDefinition"/>.
        /// Отображает список базовых определений предметов и кнопку для обновления их <c>WrapperIndex</c>.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ItemsDataStorageDefinition storage = (ItemsDataStorageDefinition)target;
            InitializeStyles();
            EditorGUILayout.PropertyField(_baseDefinitionsProperty, true);
            bool isDirty = CheckIfDirtyState(storage); 
            GUIStyle currentButtonStyle = isDirty ? _dirtyButtonStyle : _cleanButtonStyle;
            EditorGUILayout.Space();

            if (GUILayout.Button(isDirty ? "ОБНОВИТЬ WRAPPER INDEXES (СРОЧНО!)" : "Обновить Wrapper Indexes", currentButtonStyle))
            {
                UpdateWrapperIndexes(storage);
                _lastKnownHash = CalculateCurrentHash(storage);
                isDirty = false; 
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeStyles()
        {
            if (_dirtyButtonStyle == null)
            {
                _dirtyButtonStyle = new GUIStyle(GUI.skin.button);
                _dirtyButtonStyle.normal.textColor = Color.white;
                _dirtyButtonStyle.hover.textColor = Color.white;
                _dirtyButtonStyle.active.textColor = Color.white;
                _dirtyButtonStyle.normal.background = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f, 1f)); // Красный
                _dirtyButtonStyle.hover.background = MakeTex(2, 2, new Color(0.9f, 0.3f, 0.3f, 1f));
                _dirtyButtonStyle.active.background = MakeTex(2, 2, new Color(0.7f, 0.1f, 0.1f, 1f));
                _dirtyButtonStyle.fontStyle = FontStyle.Bold;
                _dirtyButtonStyle.fontSize = 12;
                _dirtyButtonStyle.fixedHeight = 30;
            }

            if (_cleanButtonStyle == null)
            {
                _cleanButtonStyle = new GUIStyle(GUI.skin.button);
                _cleanButtonStyle.normal.textColor = Color.white;
                _cleanButtonStyle.hover.textColor = Color.white;
                _cleanButtonStyle.active.textColor = Color.white;
                _cleanButtonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.6f, 0.2f, 1f)); // Зеленый
                _cleanButtonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.7f, 0.3f, 1f));
                _cleanButtonStyle.active.background = MakeTex(2, 2, new Color(0.1f, 0.5f, 0.1f, 1f));
                _cleanButtonStyle.fontStyle = FontStyle.Normal;
                _cleanButtonStyle.fontSize = 12;
                _cleanButtonStyle.fixedHeight = 30;
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
        
        private bool CheckIfDirtyState(ItemsDataStorageDefinition storage)
        {
            if (CalculateCurrentHash(storage) != _lastKnownHash)
                return true;

            for (int i = 0; i < _baseDefinitionsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = _baseDefinitionsProperty.GetArrayElementAtIndex(i);
                ItemBaseDefinition item = itemProperty.objectReferenceValue as ItemBaseDefinition;

                if (item != null)
                {
                    if (item.ID != i)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        private int CalculateCurrentHash(ItemsDataStorageDefinition storage)
        {
            unchecked // Разрешает переполнение для int, что хорошо для хэширования
            {
                int hash = 17;
                hash = hash * 23 + _baseDefinitionsProperty.arraySize.GetHashCode();

                for (int i = 0; i < _baseDefinitionsProperty.arraySize; i++)
                {
                    SerializedProperty itemProperty = _baseDefinitionsProperty.GetArrayElementAtIndex(i);
                    Object itemObject = itemProperty.objectReferenceValue;

                    if (itemObject != null)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(itemObject);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            hash = hash * 23 + assetPath.GetHashCode();
                        }
                        else
                        {
                            hash = hash * 23 + itemObject.GetHashCode();
                        }
                    }
                    else
                    {
                        hash = hash * 23 + 0;
                    }
                }
                
                return hash;
            }
        }

        private void UpdateWrapperIndexes(ItemsDataStorageDefinition storage)
        {
            Undo.RecordObject(storage, "Update Wrapper Indexes");

            bool changed = false;
            for (int i = 0; i < _baseDefinitionsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = _baseDefinitionsProperty.GetArrayElementAtIndex(i);
                ItemBaseDefinition item = itemProperty.objectReferenceValue as ItemBaseDefinition;

                if (item != null)
                {
                    if (item.ID != i) 
                    {
                        item.ID = i;
                        EditorUtility.SetDirty(item);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(storage);
                AssetDatabase.SaveAssets();
                Debug.Log("Wrapper Indexes обновлены!");
            }
            else
            {
                Debug.Log("Wrapper Indexes уже были актуальны.");
            }
        }
    }
}
#endif