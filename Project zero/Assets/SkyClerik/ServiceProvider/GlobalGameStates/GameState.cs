namespace SkyClerik.Utils
{
    /// <summary>
    /// Перечисление, определяющее различные глобальные состояния игры.
    /// </summary>
    public enum GameState : byte
    {
        /// <summary>
        /// Игра находится в главном меню.
        /// </summary>
        MainMenu = 0,
        /// <summary>
        /// Идет процесс создания новой игры.
        /// </summary>
        NewGame = 1,
        /// <summary>
        /// Идет процесс загрузки игры.
        /// </summary>
        LoadGame = 2,
        /// <summary>
        /// Игра активно запущена.
        /// </summary>
        InGame = 3,
        /// <summary>
        /// Игра поставлена на паузу.
        /// </summary>
        Paused = 4,
        /// <summary>
        /// Игра завершена (состояние Game Over).
        /// </summary>
        GameOver = 5,
    }
}
