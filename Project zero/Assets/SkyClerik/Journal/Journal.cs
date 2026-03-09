using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;
using System.Collections.Generic;

namespace SkyClerik
{
    [RequireComponent(typeof(UIDocument))]
    public class Journal : MonoBehaviour
    {
        private Quests _questsContainer;
        //private QuestAPI _questAPI;
        private JournalDocument _journalDocument;
        private VisualElement _root;

        [SerializeField]
        private VisualTreeAsset _questDescriptionAsset;

        private int _currentNpcView;

        [SerializeField]
        private KeyCode _key = KeyCode.J;
        private bool _active;

        private void Start()
        {
            //_questAPI = ServiceProvider.Get<QuestAPI>();
            _questsContainer = ServiceProvider.Get<Quests>();
            var uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
            uiDocument.enabled = true;
            _journalDocument = new JournalDocument();
            _root = uiDocument.rootVisualElement;
            _journalDocument.Initialize(_root);

            _journalDocument.bClose.clicked += Hide;

            Hide();
        }

        public void Show()
        {
            _active = true;
            _currentNpcView = 0;
            _root.SetVisibility(true);
            Debug.Log("Кнопка открытия журнала нажата!");
            UpdatePage();

        }

        public void UpdatePage()
        {
            var npc = _questsContainer.Npcs[_currentNpcView];
            //characterImage
            _journalDocument.textNameZoneValue.text = $"{npc.ActorName}";
            _journalDocument.scrollViewContent.Clear();

            if (npc.TryGetQuestsInState(out List<QuestInfo> quests, QuestInfoState.IsAccepted))
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    var description = new QuestDescription(_questDescriptionAsset);
                    description.Init(quests[i]);
                    _journalDocument.scrollViewContent.Add(description);
                }
            }

            //TODO не забудь что тут заглушка добавляет закрытые квесты
            if (npc.TryGetQuestsInState(out List<QuestInfo> quests1, QuestInfoState.IsCompleted))
            {
                for (int i = 0; i < quests1.Count; i++)
                {
                    var description = new QuestDescription(_questDescriptionAsset);
                    description.Init(quests1[i]);
                    _journalDocument.scrollViewContent.Add(description);
                }
            }
        }

        public void UpdatePageStaticText()
        {
            var npc = _questsContainer.Npcs[_currentNpcView];
            _journalDocument.textNameZoneTitle.text = "Имя";
        }

        public void Hide()
        {
            _active = false;
            _root.SetVisibility(false);
            Debug.Log("Кнопка закрытия журнала нажата!");
        }

        private void Update()
        {
            if (Input.GetKeyUp(_key))
            {
                if (!_active)
                    Show();
                else
                    Hide();
            }
        }

        /// <summary>
        /// Содержит ссылки на именованные элементы из Journal.uxml.
        /// Этот класс был сгенерирован автоматически.
        /// </summary>
        private class JournalDocument
        {
            // Корневые контейнеры
            public VisualElement windowArea;
            public VisualElement jobArea;

            // --- Левые кнопки ---
            public VisualElement mainInsetsArea;
            public Button bInventory;
            public Button bJournal;
            public Button bSettings;
            public Button bClose;

            // --- Верхняя панель ---
            public VisualElement topPanel;
            public VisualElement leftArea;
            public Label playerLevel;
            public Label playerMoney;
            public VisualElement rightArea;
            public VisualElement battery;
            public VisualElement icon;
            public Label dataTime;

            // --- Главная страница ---
            public VisualElement upInserts;
            public VisualElement page;
            public VisualElement downInserts;

            // --- Содержимое страницы ---
            public VisualElement characterArea;
            public VisualElement characterImage;
            public Label textNameZoneTitle;
            public Label textNameZoneValue;

            public VisualElement questsArea;
            public VisualElement scrollViewContent;
            public VisualElement QuestDescription; // Это <Instance> шаблона

            public void Initialize(VisualElement root)
            {
                // Запрашиваем уникальные элементы
                windowArea = root.Q<VisualElement>("window_area");
                jobArea = root.Q<VisualElement>("job_area");
                mainInsetsArea = root.Q<VisualElement>("main_insets_area");
                bInventory = root.Q<Button>("b_inventory");
                bJournal = root.Q<Button>("b_journal");
                bSettings = root.Q<Button>("b_settings");
                bClose = root.Q<Button>("b_close");
                topPanel = root.Q<VisualElement>("top_panel");
                leftArea = root.Q<VisualElement>("left_area");
                playerLevel = root.Q<Label>("player_level");
                playerMoney = root.Q<Label>("player_money");
                rightArea = root.Q<VisualElement>("right_area");
                battery = root.Q<VisualElement>("battery");
                icon = root.Q<VisualElement>("icon");
                dataTime = root.Q<Label>("data_time");
                upInserts = root.Q<VisualElement>("up_inserts");
                page = root.Q<VisualElement>("page");
                characterArea = page.Q<VisualElement>("character_area");
                var nameZone = characterArea.Q<VisualElement>("name_zone");
                textNameZoneTitle = nameZone.Q<Label>("title");
                textNameZoneValue = nameZone.Q<Label>("value");
                questsArea = page.Q<VisualElement>("quests_area");
                scrollViewContent = root.Q<VisualElement>("scroll_view_content");
                QuestDescription = root.Q<VisualElement>("QuestDescription");
                downInserts = root.Q<VisualElement>("down_inserts");
                characterImage = root.Query<VisualElement>("character_image");

                // Запрашиваем элементы с дублирующимися именами, собирая их в списки
            }
        }
    }
}
