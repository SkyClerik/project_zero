using UnityEditor;
using UnityEngine;
using UnityEngine.DataEditor;

namespace SkyClerik.Inventory.Editor
{
    [CustomEditor(typeof(GlobalItemsStorageDefinition))]
    public class GlobalItemsStorageDefinitionEditor : UnityEditor.Editor
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
            GlobalItemsStorageDefinition storage = (GlobalItemsStorageDefinition)target;
            _lastKnownHash = CalculateCurrentHash(storage);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Обновляем SerializedObject перед использованием свойств

            GlobalItemsStorageDefinition storage = (GlobalItemsStorageDefinition)target;

            InitializeStyles(); // Инициализируем стили кнопок

            // Рисуем поле _baseDefinitions
            EditorGUILayout.PropertyField(_baseDefinitionsProperty, true); // true, чтобы рисовать дочерние элементы списка

            // Вместо использования CalculateCurrentHash для CheckIfDirty, будем более явно проверять изменения
            bool isDirty = CheckIfDirtyState(storage); 

            GUIStyle currentButtonStyle = isDirty ? _dirtyButtonStyle : _cleanButtonStyle;

            // Добавляем немного отступа
            EditorGUILayout.Space();

            if (GUILayout.Button(isDirty ? "ОБНОВИТЬ WRAPPER INDEXES (СРОЧНО!)" : "Обновить Wrapper Indexes", currentButtonStyle))
            {
                UpdateWrapperIndexes(storage);
                // После обновления индексов, пересчитываем хэш, чтобы кнопка стала "чистой"
                _lastKnownHash = CalculateCurrentHash(storage); // Обновляем _lastKnownHash после успешного обновления
                // Сразу помечаем объект как чистый после нажатия
                isDirty = false; 
            }

            serializedObject.ApplyModifiedProperties(); // Применяем изменения к SerializedObject
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
                _dirtyButtonStyle.fixedHeight = 30; // Увеличим высоту кнопки
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
                _cleanButtonStyle.fixedHeight = 30; // Увеличим высоту кнопки
            }
        }

        /// <summary>
        /// Создает текстуру заданного цвета и размера.
        /// </summary>
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
        
        // Новый метод для более точной проверки "грязного" состояния
        private bool CheckIfDirtyState(GlobalItemsStorageDefinition storage)
        {
            // Проверяем, изменился ли список _baseDefinitions (количество элементов, порядок, или ссылки на объекты)
            // Это будет отслеживаться через CalculateCurrentHash
            if (CalculateCurrentHash(storage) != _lastKnownHash)
            {
                return true;
            }

            // Проверяем, корректны ли WrapperIndex относительно их позиции в списке
            for (int i = 0; i < _baseDefinitionsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = _baseDefinitionsProperty.GetArrayElementAtIndex(i);
                ItemBaseDefinition item = itemProperty.objectReferenceValue as ItemBaseDefinition;

                if (item != null)
                {
                    // Для доступа к WrapperIndex через SerializedProperty нужно его найти.
                    // Однако, WrapperIndex - это не свойство GlobalItemsStorageDefinition,
                    // а свойство ItemBaseDefinition. Мы не можем напрямую найти SerializedProperty
                    // для WrapperIndex здесь, так как item является отдельным ScriptableObject.
                    // Доступ к item.WrapperIndex возможен напрямую, если ItemBaseDefinition не приватный.
                    // Если item.WrapperIndex является приватным, тогда нужно будет создавать CustomEditor для ItemBaseDefinition
                    // или использовать рефлексию, но пока будем считать его публичным или [SerializeField].

                    // Если ItemBaseDefinition.WrapperIndex не является [SerializeField] или public,
                    // этот прямой доступ вызовет ошибку, как в GlobalItemsStorageDefinition
                    // где _baseDefinitions был приватным.
                    // Я предполагаю, что WrapperIndex является публичным или [SerializeField].
                    if (item.WrapperIndex != i)
                    {
                        return true; // Индекс не соответствует позиции
                    }
                }
            }
            return false;
        }


        private int CalculateCurrentHash(GlobalItemsStorageDefinition storage)
        {
            unchecked // Разрешает переполнение для int, что хорошо для хэширования
            {
                int hash = 17; // Начальное число для хэша

                // Добавляем количество элементов в хэш
                hash = hash * 23 + _baseDefinitionsProperty.arraySize.GetHashCode();

                // Добавляем хэш каждого элемента.
                for (int i = 0; i < _baseDefinitionsProperty.arraySize; i++)
                {
                    SerializedProperty itemProperty = _baseDefinitionsProperty.GetArrayElementAtIndex(i);
                    Object itemObject = itemProperty.objectReferenceValue;

                    if (itemObject != null)
                    {
                        // Используем GUID ассета для стабильного хэширования
                        string assetPath = AssetDatabase.GetAssetPath(itemObject);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            hash = hash * 23 + assetPath.GetHashCode();
                        }
                        else
                        {
                            // Если это не ассет (например, вложенный объект или временный),
                            // используем его собственный GetHashCode().
                            hash = hash * 23 + itemObject.GetHashCode();
                        }
                    }
                    else
                    {
                        hash = hash * 23 + 0; // Для null-элементов
                    }
                }
                
                return hash;
            }
        }

        private void UpdateWrapperIndexes(GlobalItemsStorageDefinition storage)
        {
            Undo.RecordObject(storage, "Update Wrapper Indexes"); // Для возможности отмены действия

            bool changed = false;
            for (int i = 0; i < _baseDefinitionsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = _baseDefinitionsProperty.GetArrayElementAtIndex(i);
                ItemBaseDefinition item = itemProperty.objectReferenceValue as ItemBaseDefinition;

                if (item != null)
                {
                    // Чтобы изменять WrapperIndex, нам нужно убедиться, что он доступен.
                    // Он должен быть публичным или [SerializeField] в ItemBaseDefinition.
                    // Если он приватный и не [SerializeField], то его нельзя будет изменить напрямую.
                    // Если ItemBaseDefinition является ScriptableObject, то нужно помечать его как dirty.
                    if (item.WrapperIndex != i) // Обновляем только если индекс изменился
                    {
                        item.WrapperIndex = i;
                        EditorUtility.SetDirty(item); // Помечаем ассет как измененный
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(storage); // Помечаем сам ScriptableObject как измененный
                AssetDatabase.SaveAssets(); // Сохраняем изменения
                Debug.Log("Wrapper Indexes обновлены!");
            }
            else
            {
                Debug.Log("Wrapper Indexes уже были актуальны.");
            }
        }
    }
}