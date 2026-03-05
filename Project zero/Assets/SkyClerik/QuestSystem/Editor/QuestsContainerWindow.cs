using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SkyClerik.Editor
{
    public class QuestsContainerWindow : EditorWindow
    {
        private Quests _container;
        private SerializedObject _so;
        private SerializedProperty _npcsProp;
        private Vector2 _scrollPos;

        [MenuItem("SkyClerik/Quests Container")]
        public static void Open()
        {
            var wnd = GetWindow<QuestsContainerWindow>("Quests Container");
            wnd.Show();
        }

        private void OnEnable()
        {
            if (_container != null)
                InitSerialized();
        }

        private void InitSerialized()
        {
            if (_container == null)
                return;

            _so = new SerializedObject(_container);
            _npcsProp = _so.FindProperty("_npcs"); // поле списка в QuestsContainer
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            _container = (Quests)EditorGUILayout.ObjectField(
                "Quests Container", _container, typeof(Quests), true);

            if (_container == null)
            {
                EditorGUILayout.HelpBox("Выберите QuestsContainer в сцене или префабе.", MessageType.Info);
                return;
            }

            if (_so == null || _so.targetObject != _container || _npcsProp == null)
                InitSerialized();

            if (_so == null || _npcsProp == null)
            {
                EditorGUILayout.HelpBox("Не удалось найти поле _npcs.", MessageType.Error);
                return;
            }

            _so.Update();

            EditorGUILayout.LabelField("NPC List", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _npcsProp.arraySize; i++)
            {
                SerializedProperty npcProp = _npcsProp.GetArrayElementAtIndex(i);
                if (npcProp == null)
                    continue;

                SerializedProperty npcIdProp = npcProp.FindPropertyRelative("_elementName");

                string label = npcIdProp != null ? npcIdProp.stringValue : $"NPC [{i}]";

                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField(label);

                if (GUILayout.Button("Открыть", GUILayout.Width(80)))
                {
                    var npcObj = _container.Npcs[i]; // IReadOnlyList<NpcConfigBase>
                    NpcEditorWindow.OpenNpcWindow(_container, npcObj);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            _so.ApplyModifiedProperties();
        }
    }

    public class NpcEditorWindow : EditorWindow
    {
        private Quests _container;
        private NpcConfigBase _npcInstance;   // ссылка на конкретный NPC
        private SerializedObject _soContainer;
        private SerializedProperty _npcsProp;
        private SerializedProperty _npcProp;  // SerializedProperty этого NPC
        private SerializedProperty _questsProp;
        private Vector2 _scrollPos;
        private int _maxActiveQuests = -1;
        private Type _questEnumType;

        public static void OpenNpcWindow(Quests container, NpcConfigBase npcInstance)
        {
            var window = CreateInstance<NpcEditorWindow>();
            window._container = container;
            window._npcInstance = npcInstance;
            window.titleContent = new GUIContent($"NPC: {npcInstance.GetType().Name}");
            window.Show();
        }

        private void OnEnable()
        {
            if (_container == null || _npcInstance == null)
                return;

            _soContainer = new SerializedObject(_container);
            _npcsProp = _soContainer.FindProperty("_npcs");
            if (_npcsProp != null)
            {
                // находим индекс этого NPC в списке
                for (int i = 0; i < _npcsProp.arraySize; i++)
                {
                    var prop = _npcsProp.GetArrayElementAtIndex(i);
                    if (prop != null && SerializedObject.ReferenceEquals(_npcInstance, GetManagedReferenceObject<NpcConfigBase>(prop)))
                    {
                        _npcProp = prop;
                        _questsProp = _npcProp.FindPropertyRelative("_quests");
                        break;
                    }
                }
            }

            var field = typeof(NpcConfigBase).GetField("_maxActiveQuests", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
                _maxActiveQuests = (int)field.GetRawConstantValue();

            var npcType = _npcInstance.GetType();
            var attr = npcType.GetCustomAttribute<NpcQuestEnumAttribute>();
            if (attr != null)
                _questEnumType = attr.EnumType;
        }

        // хелпер для получения объекта из managed reference
        private static T GetManagedReferenceObject<T>(SerializedProperty prop) where T : class
        {
            return prop.managedReferenceValue as T;
        }

        private void OnGUI()
        {
            if (_container == null || _npcInstance == null)
            {
                EditorGUILayout.HelpBox("NPC не найден.", MessageType.Error);
                return;
            }

            if (_soContainer == null)
                OnEnable();

            // всегда пытаемся восстановиться, если что-то разъехалось
            if (_soContainer == null || _npcsProp == null || _npcProp == null || _questsProp == null)
            {
                if (!RebuildBindings())
                {
                    EditorGUILayout.HelpBox("Не удалось инициализировать SerializedObject для NPC.", MessageType.Error);
                    return;
                }
            }

            _soContainer.Update();

            var npcIdProp = _npcProp.FindPropertyRelative("_npcId");

            if (npcIdProp != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(npcIdProp, new GUIContent("Npc ID"));
                }

                EditorGUILayout.LabelField($"NPC: {npcIdProp.enumDisplayNames[npcIdProp.enumValueIndex]}", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"NPC: {_npcInstance.GetType().Name}", EditorStyles.boldLabel);
            }

            EditorGUILayout.PropertyField(_npcProp.FindPropertyRelative("_isAcquainted"));
            EditorGUILayout.PropertyField(_npcProp.FindPropertyRelative("_curTrustLevel"));
            EditorGUILayout.PropertyField(_npcProp.FindPropertyRelative("_maxTrustLevel"));
            EditorGUILayout.PropertyField(_npcProp.FindPropertyRelative("_curActiveQuests"));
            EditorGUILayout.PropertyField(_npcProp.FindPropertyRelative("_isJournalVisible"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quests", EditorStyles.boldLabel);

            if (_questsProp == null)
            {
                EditorGUILayout.HelpBox("Список квестов не найден.", MessageType.Error);
                _soContainer.ApplyModifiedProperties();
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUI.indentLevel++;

            // идём С КОНЦА и сразу удаляем (это позволит избежать вылета движка)
            for (int i = _questsProp.arraySize - 1; i >= 0; i--)
            {
                var questProp = _questsProp.GetArrayElementAtIndex(i);
                if (questProp == null)
                    continue;

                bool remove;
                DrawQuestElement(questProp, i, out remove);

                if (remove)
                {
                    _questsProp.DeleteArrayElementAtIndex(i);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndScrollView();

            int maxQuests = _maxActiveQuests > 0 ? _maxActiveQuests : int.MaxValue;

            EditorGUI.BeginDisabledGroup(_questsProp.arraySize >= maxQuests);
            if (GUILayout.Button($"Добавить квест ({_questsProp.arraySize}/{maxQuests})"))
                _questsProp.arraySize++;
            EditorGUI.EndDisabledGroup();

            _soContainer.ApplyModifiedProperties();
        }

        private void DrawQuestElement(SerializedProperty questProp, int index, out bool remove)
        {
            remove = false;
            if (questProp == null)
                return;

            var idStringProp = questProp.FindPropertyRelative("_id");
            string title = idStringProp != null && !string.IsNullOrEmpty(idStringProp.stringValue) ? idStringProp.stringValue : $"Quest {index}";

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            questProp.isExpanded = EditorGUILayout.Foldout(questProp.isExpanded, title, true);
            remove = GUILayout.Button("Удалить", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            if (questProp.isExpanded)
            {
                EditorGUI.indentLevel++;

                // выбор локального enum вместо сырого байта
                DrawQuestKeyEnum(questProp);

                EditorGUILayout.PropertyField(questProp.FindPropertyRelative("_questInfoState"));
                EditorGUILayout.PropertyField(questProp.FindPropertyRelative("_needTrustLevel"));

                var textKeyProp = questProp.FindPropertyRelative("_textKeyDescription");
                if (textKeyProp != null)
                    EditorGUILayout.PropertyField(textKeyProp);

                EditorGUILayout.PropertyField(questProp.FindPropertyRelative("_addedMoney"));
                EditorGUILayout.PropertyField(questProp.FindPropertyRelative("_rewardItems"), true);
                EditorGUILayout.PropertyField(questProp.FindPropertyRelative("_targetNpcs"), true);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawQuestKeyEnum(SerializedProperty questProp)
        {
            if (_questEnumType == null)
            {
                // fallback: просто байт
                var questKeyProp = questProp.FindPropertyRelative("_questKey");
                if (questKeyProp != null)
                {
                    var rawValueProp = questKeyProp.FindPropertyRelative("_value");
                    if (rawValueProp != null)
                        EditorGUILayout.PropertyField(rawValueProp, new GUIContent("Quest Key"));
                }
                return;
            }

            var questKey = questProp.FindPropertyRelative("_questKey");
            if (questKey == null)
                return;

            var valueProp = questKey.FindPropertyRelative("_value");
            if (valueProp == null)
                return;

            // текущее значение байта → enum
            byte raw = (byte)valueProp.intValue;
            object currentEnum = Enum.ToObject(_questEnumType, raw);

            EditorGUI.BeginChangeCheck();

            // рисуем EnumPopup
            currentEnum = EditorGUILayout.EnumPopup("Quest ID", (Enum)currentEnum);

            if (EditorGUI.EndChangeCheck())
            {
                // enum → байт обратно
                byte newRaw = Convert.ToByte(currentEnum);
                valueProp.intValue = newRaw;

                // опционально обновить строковый _id, если он есть
                var idStringProp = questProp.FindPropertyRelative("_id");
                if (idStringProp != null)
                    idStringProp.stringValue = currentEnum.ToString();

                // обновить ElementName в QuestInfo
                var elementNameProp = questProp.FindPropertyRelative("_elementName");
                if (elementNameProp != null)
                    elementNameProp.stringValue = currentEnum.ToString();
            }
        }

        private bool RebuildBindings()
        {
            if (_container == null || _npcInstance == null)
                return false;

            _soContainer = new SerializedObject(_container);
            _npcsProp = _soContainer.FindProperty("_npcs");
            _npcProp = null;
            _questsProp = null;

            if (_npcsProp == null)
                return false;

            for (int i = 0; i < _npcsProp.arraySize; i++)
            {
                var prop = _npcsProp.GetArrayElementAtIndex(i);
                if (prop == null)
                    continue;

                if (SerializedObject.ReferenceEquals(
                        _npcInstance,
                        GetManagedReferenceObject<NpcConfigBase>(prop)))
                {
                    _npcProp = prop;
                    _questsProp = _npcProp.FindPropertyRelative("_quests");
                    break;
                }
            }

            return _npcProp != null && _questsProp != null;
        }

    }
}