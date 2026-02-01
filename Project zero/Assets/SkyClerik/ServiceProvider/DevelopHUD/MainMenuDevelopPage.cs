using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Toolbox;
using SkyClerik.GlobalGameStates;
using SkyClerik.Utils;

namespace SkyClerik
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuDevelopPage : MonoBehaviour
    {
        [SerializeField]
        private ChainTrigger _newGameChain;
        private UIDocument _uiDocument;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
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
            StyleButton(newGameButton);
            menuContainer.Add(newGameButton);

            var loadButton = ToolkitExt.CreateButton("Загрузить", OnLoadGameClick);
            StyleButton(loadButton);
            menuContainer.Add(loadButton);

            var exitButton = ToolkitExt.CreateButton("Выход", OnExitClick);
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
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager != null)
            {
                gameStateManager.GlobalGameState.SetNewGame();
                _newGameChain?.StartChain();
            }
            else
            {
                Debug.LogError("GameStateManager не найден!");
            }
        }

        private void OnLoadGameClick()
        {
            var gameStateManager = ServiceProvider.Get<GameStateManager>();
            if (gameStateManager != null)
            {
                gameStateManager.GlobalGameState.SetLoadGame();
                _newGameChain?.StartChain();
            }
            else
            {
                Debug.LogError("GameStateManager не найден!");
            }
        }

        private void OnExitClick()
        {
            Application.Quit();
        }
    }
}