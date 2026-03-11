using Assets.SimpleLocalization.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik
{
    public class JournalQuestButton : Button
    {
        public int IndexHash;
    }

    [RequireComponent(typeof(UIDocument))]
    public class Journal : MonoBehaviour
    {
        private Quests _questsContainer;
        private JournalDocument _journalDocument;
        private VisualElement _root;

        [SerializeField]
        private VisualTreeAsset _questDescriptionAsset;

        private List<NpcConfigBase> _npcs = new List<NpcConfigBase>();
        private const string _localizationJournalPrefix = "Journal.";
        private const string _localizationQuestPrefix = "Quest.";
        private int _currentNpcView;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            _questsContainer = ServiceProvider.Get<Quests>();
            var uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
            uiDocument.enabled = true;
            _journalDocument = new JournalDocument();
            _root = uiDocument.rootVisualElement;
            _journalDocument.Initialize(_root);

            LocalizationChanged();
            LocalizationManager.OnLocalizationChanged += LocalizationChanged;

            Hide();
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
            LocalizationManager.OnLocalizationChanged -= LocalizationChanged;
        }

        public void Show()
        {
            //Debug.Log("Кнопка открытия журнала нажата!");
            _currentNpcView = 0;

            UpdateUpInsets();
            UpdatePage();

            _root.SetVisibility(true);
        }

        private void UpdateUpInsets()
        {
            _journalDocument.upInsets.Clear();
            _npcs = _questsContainer.GetCurrentQuestNpc();
            for (int i = 0; i < _npcs.Count; i++)
            {
                JournalQuestButton button = new JournalQuestButton();
                button.text = string.Empty;
                button.IndexHash = i;
                button.SetWidthAndHeight(120, 120);
                button.clicked += () =>
                {
                    _currentNpcView = button.IndexHash;
                    UpdatePage();
                };
                _journalDocument.upInsets.Add(button);
            }
        }

        public void UpdatePage()
        {
            NpcConfigBase npc = _questsContainer.Npcs[_currentNpcView];
            //characterImage
            _journalDocument.nameZoneValue.text = $"{npc.ActorName}";
            _journalDocument.scrollViewContent.Clear();

            if (npc.TryGetQuestsInState(out List<QuestInfo> quests, QuestInfoState.IsAccepted))
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    var description = new QuestDescription(_questDescriptionAsset);
                    var questTitle = LocalizationManager.Localize($"{_localizationQuestPrefix}{npc.ElementName}.id.{quests[i].ElementName}");
                    quests[i].QuestDescription = LocalizationManager.Localize($"{_localizationQuestPrefix}{npc.ElementName}.desc.{quests[i].ElementName}");
                    Debug.Log($"ElementName {_localizationQuestPrefix}{quests[i].ElementName} - res: {questTitle}");
                    Debug.Log($"QuestKey {_localizationQuestPrefix}{quests[i].QuestKey} - res: {quests[i].QuestDescription}");
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

        private void LocalizationChanged()
        {
            UpdatePageStaticText();
        }

        public void UpdatePageStaticText()
        {
            _journalDocument.nameZoneTitle.text = LocalizationManager.Localize($"{_localizationJournalPrefix}{_journalDocument.nameZoneTitle.name}");
        }

        public void Hide()
        {
            _root.SetVisibility(false);
            //Debug.Log("Кнопка закрытия журнала нажата!");
        }

        private class JournalDocument
        {
            // --- Главная страница ---
            public VisualElement upInsets;
            public VisualElement page;
            public VisualElement downInserts;

            // --- Содержимое страницы ---
            public VisualElement characterArea;
            public VisualElement characterImage;
            public Label nameZoneTitle;
            public Label nameZoneValue;

            public VisualElement questsArea;
            public VisualElement scrollViewContent;
            public VisualElement QuestDescription; // Это <Instance> шаблона

            public void Initialize(VisualElement root)
            {
                upInsets = root.Q<VisualElement>("up_insets");
                page = root.Q<VisualElement>("page");
                characterArea = page.Q<VisualElement>("character_area");
                var nameZone = characterArea.Q<VisualElement>("name_zone");
                nameZoneTitle = nameZone.Q<Label>("name_zone_title");
                nameZoneValue = nameZone.Q<Label>("value");
                questsArea = page.Q<VisualElement>("quests_area");
                scrollViewContent = root.Q<VisualElement>("scroll_view_content");
                QuestDescription = root.Q<VisualElement>("QuestDescription");
                downInserts = root.Q<VisualElement>("down_inserts");
                characterImage = root.Query<VisualElement>("character_image");
            }
        }
    }
}
