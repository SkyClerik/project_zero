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
        /// Игра активно запущена.
        /// </summary>
        InGame = 3,
        /// <summary>
        /// Игра поставлена на паузу.
        /// </summary>
        Paused = 4,
    }
}
