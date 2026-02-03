using UnityEngine;

namespace SkyClerik.Utils
{
    [System.Serializable]
    public class GlobalGameProperty
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

        [Tooltip("Установлено в 'true', если игрок не хочет видеть автоматические подсказки.")]
        [SerializeField]
        private bool _hasInfomatron = false;
        public bool HasInfomatron => _hasInfomatron;

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
            _hasSeenIntro = false;
            _playerScore = 0;
            Debug.Log("Глобальное состояние игры установлено: Новая Игра.");
        }

        /// <summary>
        /// Устанавливает состояние для загрузки существующей игры.
        /// </summary>
        public void SetLoadGame()
        {
            _currentGameState = GameState.LoadGame;
            _isNewGame = false;
            Debug.Log("Глобальное состояние игры установлено: Загрузка Игры.");
        }

        /// <summary>
        /// Устанавливает состояние, когда идет активная игра.
        /// </summary>
        public void SetInGame()
        {
            _currentGameState = GameState.InGame;
            Debug.Log("Глобальное состояние игры установлено: В Игре.");
        }

        /// <summary>
        /// Устанавливает игровое состояние на "Пауза".
        /// </summary>
        public void SetPaused()
        {
            _currentGameState = GameState.Paused;
            Debug.Log("Глобальное состояние игры установлено: Пауза.");
        }

        /// <summary>
        /// Устанавливает игровое состояние на "Игра окончена".
        /// </summary>
        public void SetGameOver()
        {
            _currentGameState = GameState.GameOver;
            Debug.Log("Глобальное состояние игры установлено: Игра Окончена.");
        }

        // Методы для обновления других флагов
        public void SetHasSeenIntro(bool value)
        {
            _hasSeenIntro = value;
            Debug.Log($"Просмотрено Вступление: {_hasSeenIntro}");
        }

        public void AddPlayerScore(int amount)
        {
            _playerScore += amount;
            Debug.Log($"Счет игрока обновлен: {_playerScore}");
        }

        /// <summary>
        /// Устанавливает текущий слот сохранения.
        /// </summary>
        /// <param name="index">Индекс слота.</param>
        public void SetCurrentSaveSlot(int index)
        {
            _currentSaveSlotIndex = index;
            Debug.Log($"Текущий слот сохранения установлен: {_currentSaveSlotIndex}");
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
            Debug.Log("Глобальное состояние игры сброшено до значений Главного Меню.");
        }
    }
}
