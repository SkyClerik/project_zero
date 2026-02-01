using UnityEngine;

namespace SkyClerik.GlobalGameStates
{
    [System.Serializable]
    public class GlobalGameState
    {
        [Header("Состояние игры")]
        [Tooltip("Текущее состояние игры (например, в главном меню, в игре, на паузе).")]
        [SerializeField]
        private GameState _currentGameState = GameState.MainMenu;
        public GameState CurrentGameState => _currentGameState;

        [Tooltip("Установлено в 'true', если это новая игра. 'false' - если загруженная.")]
        [SerializeField]
        private bool _isNewGame = true;
        public bool IsNewGame => _isNewGame;

        [Header("Глобальные флаги")]
        [Tooltip("Установлено в 'true', если игрок уже посмотрел вступительный ролик/обучение.")]
        [SerializeField]
        private bool _hasSeenIntro = false;
        public bool HasSeenIntro => _hasSeenIntro;

        [Tooltip("Текущий счет или очки игрока.")]
        [SerializeField]
        private int _playerScore = 0;
        public int PlayerScore => _playerScore;

        [Header("Сохранение")]
        [Tooltip("Индекс текущего слота сохранения (например, 0, 1, 2).")]
        [SerializeField]
        private int _currentSaveSlotIndex = 0;
        public int CurrentSaveSlotIndex => _currentSaveSlotIndex;

        /// <summary>
        /// Инициализирует состояние для новой игры.
        /// </summary>
        public void SetNewGame()
        {
            _currentGameState = GameState.NewGame;
            _isNewGame = true;
            _hasSeenIntro = false; // Сбрасываем флаг вступления при новой игре
            _playerScore = 0; // Сбрасываем счёт
            Debug.Log("Global Game State set to New Game.");
        }

        /// <summary>
        /// Устанавливает состояние для загрузки существующей игры.
        /// </summary>
        public void SetLoadGame()
        {
            _currentGameState = GameState.LoadGame;
            _isNewGame = false;
            Debug.Log("Global Game State set to Load Game.");
        }

        /// <summary>
        /// Устанавливает состояние, когда идет активная игра.
        /// </summary>
        public void SetInGame()
        {
            _currentGameState = GameState.InGame;
            Debug.Log("Global Game State set to In Game.");
        }
        
        /// <summary>
        /// Устанавливает игровое состояние на "Пауза".
        /// </summary>
        public void SetPaused()
        {
            _currentGameState = GameState.Paused;
            Debug.Log("Global Game State set to Paused.");
        }

        /// <summary>
        /// Устанавливает игровое состояние на "Игра окончена".
        /// </summary>
        public void SetGameOver()
        {
            _currentGameState = GameState.GameOver;
            Debug.Log("Global Game State set to Game Over.");
        }

        // Методы для обновления других флагов
        public void SetHasSeenIntro(bool value)
        {
            _hasSeenIntro = value;
            Debug.Log($"Has Seen Intro: {_hasSeenIntro}");
        }

        public void AddPlayerScore(int amount)
        {
            _playerScore += amount;
            Debug.Log($"Player Score updated: {_playerScore}");
        }

        /// <summary>
        /// Устанавливает текущий слот сохранения.
        /// </summary>
        /// <param name="index">Индекс слота.</param>
        public void SetCurrentSaveSlot(int index)
        {
            _currentSaveSlotIndex = index;
            Debug.Log($"Current save slot set to: {_currentSaveSlotIndex}");
        }

        /// <summary>
        /// Сбрасывает все глобальное состояние игры до значений по умолчанию (главное меню).
        /// </summary>
        public void ResetStateToMainMenu()
        {
            _currentGameState = GameState.MainMenu;
            _isNewGame = true;
            _hasSeenIntro = false;
            _playerScore = 0;
            _currentSaveSlotIndex = 0;
            Debug.Log("Global Game State reset to Main Menu defaults.");
        }
    }
}
