using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;
using SkyClerik.Utils;

namespace SkyClerik
{
    /// <summary>
    /// Компонент, управляющий UI главной страницы меню для режима разработки.
    /// Предоставляет кнопки для начала новой игры, загрузки и выхода,
    /// а также инициирует цепочку выполнения действий после выбора опции.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuDevelopPage : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Сюда перетаскиваем первое звено которое начнет цепочку")]
        private MonoBehaviour _firstStep;
        private IChain _chainHead;

        [SerializeField]
        [ReadOnly]
        private UIDocument _uiDocument;

        private void OnValidate()
        {
            _uiDocument = GetComponentInChildren<UIDocument>(includeInactive: false);
        }

        private void Awake()
        {
            _uiDocument.enabled = true;
            //_uiDocument.rootVisualElement.SetDisplay(false);

            if (_firstStep != null && _firstStep is IChain)
                _chainHead = _firstStep as IChain;
            else
                Debug.LogError("Первое звено цепи не назначено или не реализует IChain!", this);
        }

        /// <summary>
        /// Запускает выполнение цепочки действий, начиная с первого звена.
        /// </summary>
        public void StartChain()
        {
            _chainHead?.ExecuteStep();
        }

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            root.Clear();

            var menuContainer = new VisualElement();
            menuContainer.SetWidthPercentage(100);
            menuContainer.SetHeightPercentage(100);
            menuContainer.style.justifyContent = Justify.Center;
            menuContainer.style.alignItems = Align.Center;
            root.Add(menuContainer);

            var newGameButton = ToolkitExt.CreateButton("Новая игра", OnNewGameClick);
            newGameButton.style.minHeight = 80;
            StyleButton(newGameButton);
            menuContainer.Add(newGameButton);

            var loadButton = ToolkitExt.CreateButton("Загрузить", OnLoadGameClick);
            loadButton.style.minHeight = 80;
            StyleButton(loadButton);
            menuContainer.Add(loadButton);

            var exitButton = ToolkitExt.CreateButton("Выход", OnExitClick);
            exitButton.style.minHeight = 80;
            StyleButton(exitButton);
            menuContainer.Add(exitButton);
        }

        private void StyleButton(Button button)
        {
            button.SetWidthAndHeight(200, 40);
            button.style.marginTop = 10;
            button.style.marginBottom = 10;
            button.style.fontSize = 18;
            button.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            button.style.color = new StyleColor(Color.white);

            button.SetBorderWidth(1);
            button.SetBorderColor(Color.gray);
        }

        private void OnNewGameClick()
        {
            var globalBox = ServiceProvider.Get<GlobalBox>();
            if (globalBox != null)
            {
                Hide();
                globalBox.GlobalGameProperty.SetNewGame();
                StartChain();
            }
            else
            {
                Debug.LogError("GameStateManager не найден!");
            }
        }

        private void OnLoadGameClick()
        {
            var globalBox = ServiceProvider.Get<GlobalBox>();
            if (globalBox != null)
            {
                Hide();
                globalBox.GlobalGameProperty.SetLoadGame();
                StartChain();
            }
            else
            {
                Debug.LogError("GameStateManager не найден!");
            }
        }

        private void OnExitClick()
        {
            Hide();
            Application.Quit();
        }

        private void Hide()
        {
            _uiDocument.rootVisualElement.SetDisplay(false);
        }
    }
}