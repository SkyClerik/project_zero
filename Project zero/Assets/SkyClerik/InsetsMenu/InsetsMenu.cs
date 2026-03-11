using Assets.SimpleLocalization.Scripts;
using SkyClerik.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik
{
    [RequireComponent(typeof(UIDocument))]
    public class InsetsMenu : MonoBehaviour
    {
        private VisualElement _root;
        private InsetDocument _insetDocument;
        [SerializeField]
        private KeyCode _key = KeyCode.Tab;
        private bool _active;

        private const string _localizationPrefix = "UI.";

        private InventoryAPI _inventoryAPI;
        private Journal _journal;
        private DevelopInset _developInset;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void Start()
        {
            var uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
            uiDocument.enabled = true;
            _root = uiDocument.rootVisualElement;
            _insetDocument = new InsetDocument(_root);

            _inventoryAPI = ServiceProvider.Get<InventoryAPI>();
            _journal = ServiceProvider.Get<Journal>();
            _developInset = new DevelopInset(_insetDocument);

            _insetDocument.bInventory.clicked += ClickInventory;
            _insetDocument.bJournal.clicked += ClickJournal;
            _insetDocument.bSettings.text = "DEVELOP PAGE";
            _insetDocument.bSettings.clicked += ClickDevelopPage;
            _insetDocument.bClose.clicked += Hide;

            LocalizationChanged();
            LocalizationManager.OnLocalizationChanged += LocalizationChanged;

            Hide();
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
            LocalizationManager.OnLocalizationChanged -= LocalizationChanged;
        }

        private void ClickInventory()
        {
            ClosePages();
            _inventoryAPI.OpenInventoryAndEquip();
        }

        private void ClickJournal()
        {
            ClosePages();
            _journal.Show();
        }

        private void ClickDevelopPage()
        {
            ClosePages();
            _developInset.Show();
        }

        public void Show()
        {
            _active = true;
            _root.SetVisibility(true);
        }

        private void LocalizationChanged()
        {
            UpdatePageStaticText();
        }

        public void UpdatePageStaticText()
        {
            _insetDocument.bInventory.text = LocalizationManager.Localize($"{_localizationPrefix}{_insetDocument.bInventory.name}");
            _insetDocument.bJournal.text = LocalizationManager.Localize($"{_localizationPrefix}{_insetDocument.bJournal.name}");
            //_insetDocument.bSettings.text = LocalizationManager.Localize($"{_localizationPrefix}{_insetDocument.bSettings.name}");
            _insetDocument.bClose.text = LocalizationManager.Localize($"{_localizationPrefix}{_insetDocument.bClose.name}");

            _insetDocument.playerLevel.text = LocalizationManager.Localize($"{_localizationPrefix}{_insetDocument.playerLevel.name}");
            _insetDocument.playerMoney.text = LocalizationManager.Localize($"{_localizationPrefix}{_insetDocument.playerMoney.name}");
            _insetDocument.dataTimeMonth.text = LocalizationManager.Localize($"{_localizationPrefix}{_insetDocument.dataTimeMonth.name}");
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

        public void Hide()
        {
            ClosePages();
            _active = false;
            _root.SetVisibility(false);
            Debug.Log("Кнопка закрытия вкладок нажата!");
        }

        private void ClosePages()
        {
            _inventoryAPI.CloseAll();
            _journal.Hide();
            _developInset.Hide();
        }

        private class InsetDocument
        {
            // --- Левые кнопки ---
            public Button bInventory;
            public Button bJournal;
            public Button bSettings;
            public Button bClose;

            // --- Верхняя панель ---
            public VisualElement topPanel;
            public VisualElement leftArea;
            public Label playerLevel;
            public Label playerLevelValue;
            public Label playerMoney;
            public Label playerMoneyValue;
            public VisualElement rightArea;
            public VisualElement battery;
            public VisualElement battery_icon;
            public Label dataTimeMonth;
            public Label dataValue;
            public Label timeValue;

            public VisualElement space;

            public InsetDocument(VisualElement root)
            {
                bInventory = root.Q<Button>("b_inventory");
                bJournal = root.Q<Button>("b_journal");
                bSettings = root.Q<Button>("b_settings");
                bClose = root.Q<Button>("b_close");

                topPanel = root.Q<VisualElement>("top_panel");
                leftArea = root.Q<VisualElement>("left_area");
                playerLevel = root.Q<Label>("player_level");
                playerLevelValue = root.Q<Label>("player_level_value");
                playerMoney = root.Q<Label>("player_money");
                playerMoneyValue = root.Q<Label>("player_money_value");
                rightArea = root.Q<VisualElement>("right_area");
                battery = root.Q<VisualElement>("battery");
                battery_icon = root.Q<VisualElement>("battery_icon");
                dataTimeMonth = root.Q<Label>("data_time_martch");
                dataValue = root.Q<Label>("data_value");
                timeValue = root.Q<Label>("time_value");

                space = root.Q<VisualElement>("space");
            }
        }

        private class DevelopInset
        {
            private InsetDocument _insetDocument;

            private List<string> _languages = new() { "Russian", "English", "Spanish", "Germany" };
            private VisualElement _languageArea;
            public DevelopInset(InsetDocument insetDocument)
            {
                _insetDocument = insetDocument;

                CreateLanguageElements();
            }

            private void CreateLanguageElements()
            {
                _languageArea = new VisualElement();

                var languageSelect = new PopupField<string>(_languages, 1);
                languageSelect.name = "languageSelect";
                languageSelect.RegisterCallback<ChangeEvent<string>>((evt) =>
                {
                    languageSelect.value = evt.newValue;
                    LocalizationManager.Language = evt.newValue;
                });

                var buttonsArea = new VisualElement();
                buttonsArea.style.flexDirection = FlexDirection.Row;
                buttonsArea.style.justifyContent = Justify.SpaceBetween;
                _languageArea.Add(buttonsArea);

                foreach (var language in _languages)
                {
                    Button button = new Button();
                    button.text = language;
                    button.SetWidthAndHeight(200, 50);
                    button.clicked += () =>
                    {
                        LocalizationManager.Language = button.text;
                        _languageArea.Q<PopupField<string>>("languageSelect").value = button.text;
                    };
                    buttonsArea.Add(button);
                }

                _languageArea.Add(languageSelect);
            }

            public void Show()
            {
                _insetDocument.space.Add(_languageArea);
            }

            public void Hide()
            {
                _insetDocument.space.Clear();
            }
        }
    }
}
