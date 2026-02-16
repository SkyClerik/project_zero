using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace SkyClerik
{
	public class Journal : MonoBehaviour
	{
        private JournalDocument _journalDocument;

        private void Start()
        {
            var uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
            if (uiDocument == null)
            {
                Debug.LogError("Journal: UIDocument component not found on this GameObject!", this);
                return;
            }
            uiDocument.enabled = true;
            _journalDocument = new JournalDocument();
            _journalDocument.Initialize(uiDocument.rootVisualElement);

            // Теперь вы можете получить доступ к вашим элементам через _journalDocument, например:
            // _journalDocument.b_close.clicked += () => Debug.Log("Кнопка закрытия нажата!");
        }

        /// <summary>
        /// Содержит ссылки на именованные элементы из Journal.uxml.
        /// Этот класс был сгенерирован автоматически.
        /// </summary>
        private class JournalDocument
        {
            // Поля упорядочены так, чтобы отражать иерархию в UXML-файле.
            
            // Корневые контейнеры
            public VisualElement window_area;
            public VisualElement job_area;

            // --- Левые кнопки ---
            public VisualElement main_insets_area;
            public Button b_inventory;
            public Button b_journal;
            public Button b_settings;
            public Button b_close;

            // --- Верхняя панель ---
            public VisualElement top_panel;
            public VisualElement left_area;
            public Label player_level;
            public Label player_money;
            public VisualElement right_area;
            public VisualElement battery;
            public VisualElement icon;
            public Label data_time;
            
            // --- Главная страница ---
            public VisualElement up_inserts;
            public VisualElement page;
            public VisualElement down_inserts;

            // --- Содержимое страницы ---
            public VisualElement character_area;
            public VisualElement quests_area;
            public VisualElement scroll_view_content;
            public VisualElement QuestDescription; // Это <Instance> шаблона

            // ПРИМЕЧАНИЕ: Следующие поля являются Списками (List), потому что несколько элементов в UXML
            // используют одно и то же имя. Запрос по такому имени вернет коллекцию элементов.
            
            /// <summary>
            /// Список всех VisualElement с именем "progress_parent".
            /// </summary>
            public List<VisualElement> progress_parent;

            /// <summary>
            /// Список всех VisualElement с именем "text_wrap".
            /// </summary>
            public List<VisualElement> text_wrap;

            /// <summary>
            /// Список всех Label с именем "title".
            /// </summary>
            public List<Label> title;

            /// <summary>
            /// Список всех Label с именем "value".
            /// </summary>
            public List<Label> value;


            public void Initialize(VisualElement root)
            {
                // Запрашиваем уникальные элементы
                window_area = root.Q<VisualElement>("window_area");
                job_area = root.Q<VisualElement>("job_area");
                main_insets_area = root.Q<VisualElement>("main_insets_area");
                b_inventory = root.Q<Button>("b_inventory");
                b_journal = root.Q<Button>("b_journal");
                b_settings = root.Q<Button>("b_settings");
                b_close = root.Q<Button>("b_close");
                top_panel = root.Q<VisualElement>("top_panel");
                left_area = root.Q<VisualElement>("left_area");
                player_level = root.Q<Label>("player_level");
                player_money = root.Q<Label>("player_money");
                right_area = root.Q<VisualElement>("right_area");
                battery = root.Q<VisualElement>("battery");
                icon = root.Q<VisualElement>("icon");
                data_time = root.Q<Label>("data_time");
                up_inserts = root.Q<VisualElement>("up_inserts");
                page = root.Q<VisualElement>("page");
                character_area = root.Q<VisualElement>("character_area");
                quests_area = root.Q<VisualElement>("quests_area");
                scroll_view_content = root.Q<VisualElement>("scroll_view_content");
                QuestDescription = root.Q<VisualElement>("QuestDescription");
                down_inserts = root.Q<VisualElement>("down_inserts");

                // Запрашиваем элементы с дублирующимися именами, собирая их в списки
                progress_parent = root.Query<VisualElement>("progress_parent").ToList();
                text_wrap = root.Query<VisualElement>("text_wrap").ToList();
                title = root.Query<Label>("title").ToList();
                value = root.Query<Label>("value").ToList();
            }
        }
    }
}
