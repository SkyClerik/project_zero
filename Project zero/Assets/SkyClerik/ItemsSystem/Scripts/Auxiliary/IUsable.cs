namespace SkyClerik.Inventory
{
    /// <summary>
    /// Интерфейс для предметов, которые могут быть использованы игроком (например, по двойному клику).
    /// </summary>
    public interface IUsable
    {
        /// <summary>
        /// Выполняет логику использования предмета.
        /// </summary>
        void Use();
    }
}
