using UnityEngine.DataEditor;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// Аргументы события, когда предмет перемещается между контейнерами.
    /// </summary>
    public struct ItemMovedEventArgs
    {
        /// <summary>
        /// Перемещенный предмет.
        /// </summary>
        public ItemBaseDefinition Item { get; }
        
        /// <summary>
        /// Контейнер-источник (откуда перемещен предмет).
        /// </summary>
        public ItemContainer SourceContainer { get; }
        
        /// <summary>
        /// Контейнер-получатель (куда перемещен предмет).
        /// </summary>
        public ItemContainer DestinationContainer { get; }

        public ItemMovedEventArgs(ItemBaseDefinition item, ItemContainer source, ItemContainer destination)
        {
            Item = item;
            SourceContainer = source;
            DestinationContainer = destination;
        }
    }
}
