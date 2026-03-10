using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik
{
    [InitializeOnLoad]
    public static class EditorStartupRunner
    {
        private static bool _initialized;
        private static bool _isInitializationPending;
        private static double _initializationStartTime;
        private const float DelayDuration = 2f;
        public static EditorStartupDefinition EditorStartup;

        static EditorStartupRunner()
        {
            ScheduleInitialize();
        }

        /// <summary>
        /// Планирует отложенную инициализацию, чтобы не цепляться за редактор
        /// в момент его активной загрузки.
        /// </summary>
        private static void ScheduleInitialize()
        {
            if (_isInitializationPending || _initialized)
                return;

            _isInitializationPending = true;
            _initializationStartTime = EditorApplication.timeSinceStartup;

            // Подписываемся на EditorApplication.update для проверки времени
            EditorApplication.update += DelayedInitializeUpdate;
        }

        /// <summary>
        /// Проверяет, прошла ли задержка, и запускает фактическую инициализацию.
        /// </summary>
        private static void DelayedInitializeUpdate()
        {
            if (EditorApplication.timeSinceStartup - _initializationStartTime < DelayDuration)
                return;

            // Отписываемся от события, чтобы не вызываться каждый кадр
            EditorApplication.update -= DelayedInitializeUpdate;
            _isInitializationPending = false;

            Initialize();
        }

        /// <summary>
        /// Фактическая инициализация. Здесь можно подписываться на события,
        /// создавать окна и т.п. Вызывается один раз.
        /// </summary>
        private static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            // Пример: можно подписаться на события редактора, если нужно.
            // EditorApplication.hierarchyChanged += OnHierarchyChanged;
            // EditorApplication.projectChanged += OnProjectChanged;

            Run();
        }

        /// <summary>
        /// Единая точка входа для твоей логики после инициализации редактора.
        /// Отсюда можно вызывать любые нужные методы/инициализацию инструментов.
        /// </summary>
        public static void Run()
        {
            if (TryLoadDataForCurrentUser(out EditorStartupRunner.EditorStartup))
            {

                // Пример: открыть или обновить своё окно на UI Toolkit
                if (EditorStartupRunner.EditorStartup.ShowStartingWindow)
                    OpenOrCreateWindow();
            }
        }

        /// <summary>
        /// Открывает/создаёт кастомное окно редактора на UI Toolkit.
        /// Показывает пример работы с VisualElement.
        /// </summary>
        private static void OpenOrCreateWindow()
        {
            var window = EditorWindow.GetWindow<EditorStartupWindow>();
            window.titleContent = new GUIContent("Startup Tool");
            window.Show();
        }

        // ----

        /// <summary>
        /// Вспомогательный метод для получения текущего пользователя редактора
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentEditorUser()
        {
            string userName = CloudProjectSettings.userName;
            // Если Unity ID user name пустой (пользователь не вошел или не настроен), используем имя пользователя ОС
            if (string.IsNullOrEmpty(userName))
                userName = System.Environment.UserName;

            return userName;
        }

        /// <summary>
        /// Попробуем найти глобальные настройки для других расширений. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryGetData<T>(Type type, out T data) where T : ScriptableObject
        {
            data = null;
            var t = type.ToString().Split('.').Last();
            string[] guids = AssetDatabase.FindAssets($"t:{t}");

            var foundDatas = new List<T>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    foundDatas.Add(asset);
                }
            }

            data = foundDatas.FirstOrDefault();

            if (data != null)
            {
                Debug.Log($"Загружен актив {t}");
                return true;
            }
            else
            {
                Debug.LogWarning($"Актив {t} не найден.");
                return false;
            }
        }

        private static bool TryLoadDataForCurrentUser(out EditorStartupDefinition data)
        {
            data = null;
            string currentUser = GetCurrentEditorUser();
            string[] guids = AssetDatabase.FindAssets("t:EditorStartupDefinition");
            var foundDatas = new List<EditorStartupDefinition>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var asset = AssetDatabase.LoadAssetAtPath<EditorStartupDefinition>(path);
                if (asset != null)
                {
                    foundDatas.Add(asset);
                }
            }

            data = foundDatas.FirstOrDefault(hd => hd.Developers.Contains(currentUser));

            if (data != null)
            {
                Debug.Log($"Загружен актив EditorStartupDefinition для разработчика '{currentUser}'");
                return true;
            }
            else
            {
                //Debug.LogWarning($"HierarchyData для текущего пользователя '{currentUser}' не найдено. Попытка найти актив для 'Default' пользователя.");
                //data = foundDatas.FirstOrDefault(hd => hd.GetSkyClerikMail == "default");

                //if (data != null)
                //{
                //    Debug.Log($"Загружен актив EditorStartupDefinition для пользователя 'default': {AssetDatabase.GetAssetPath(data)}");
                //    return true;
                //}
                //else
                //{
                //    Debug.LogWarning($"EditorStartupDefinition для пользователя 'default' не найдено. Всего найдено активов EditorStartupDefinition: {foundDatas.Count}.");
                //    return false;
                //}
                return false;
            }
        }
    }

    /// <summary>
    /// Пример окна на UI Toolkit, чтобы показать базовую интеграцию.
    /// </summary>
    public class EditorStartupWindow : EditorWindow
    {
        /// <summary>
        /// Метод вызывается Unity при создании/открытии окна.
        /// Здесь создаём UI через UI Toolkit.
        /// </summary>
        public void CreateGUI()
        {
            // Корневой элемент UI Toolkit
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            // Заголовок
            var titleLabel = new Label("Инициализация редактора выполнена")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13,
                    marginBottom = 4
                }
            };

            // Описание
            var descriptionLabel = new Label(
                "Этот инструмент запущен автоматически после старта редактора.\n" +
                "Здесь можно вызывать любые нужные методы и отображать статус.")
            {
                style =
                {
                    marginBottom = 8
                }
            };

            // Кнопка для ручного перезапуска логики Run()
            var rerunButton = new Button(() =>
            {
                // Повторно вызываем основной метод
                EditorStartupRunner.Run();
            })
            {
                text = "Повторно выполнить Run()",
            };

            root.Add(titleLabel);
            root.Add(descriptionLabel);
            root.Add(rerunButton);
        }
    }
}
