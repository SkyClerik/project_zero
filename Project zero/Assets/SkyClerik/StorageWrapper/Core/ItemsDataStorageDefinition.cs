using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    /// <summary>
    /// ScriptableObject для хранения глобального списка всех базовых определений предметов в игре.
    /// Используется для получения клонированных экземпляров предметов по их ID или WrapperIndex.
    /// </summary>
    [CreateAssetMenu(fileName = "Global Items Storage Definition", menuName = "SkyClerik/Inventory/Global Items Storage Definition")]
    public class ItemsDataStorageDefinition : ScriptableObject
    {
        [SerializeField]
        private List<ItemBaseDefinition> _baseDefinitions = new List<ItemBaseDefinition>();

        /// <summary>
        /// Возвращает клонированный <see cref="ItemBaseDefinition"/> по его itemID = (индексу в списке).
        /// </summary>
        /// <param name="itemID">Индекс (itemID) искомого предмета.</param>
        /// <returns>Клонированный <see cref="ItemBaseDefinition"/> или null, если предмет не найден или индекс вне диапазона.</returns>
        public ItemBaseDefinition GetClonedItem(int itemID)
        {
            if (itemID >= 0 && itemID < _baseDefinitions.Count)
            {
                ItemBaseDefinition originalItem = _baseDefinitions[itemID];
                if (originalItem != null)
                {
                    return originalItem.Clone();
                }
                Debug.LogWarning($"[ItemsDataStorageDefinition] Предмет по ID '{itemID}' найден, но его определение равно null.");
                return null;
            }

            Debug.LogWarning($"[ItemsDataStorageDefinition] Искомый предмет с ID '{itemID}' не найден (индекс вне диапазона).");
            return null;
        }

        /// <summary>
        /// Возвращает <see cref="ItemBaseDefinition"/> из внутреннего списка <c>_baseDefinitions</c> по указанному itemID = (индексу в списке).
        /// Этот метод предоставляет прямой доступ к оригинальному объекту <see cref="ItemBaseDefinition"/> из списка,
        /// что может быть полезно для чтения данных или для операций, не требующих клонирования.
        /// Возвращает null, если индекс находится вне диапазона.
        /// </summary>
        /// <param name="itemID">Индекс <see cref="ItemBaseDefinition"/> в списке <c>_baseDefinitions</c>.</param>
        /// <returns>Оригинальный <see cref="ItemBaseDefinition"/> по указанному индексу или null, если индекс невалиден.</returns>
        public ItemBaseDefinition GetOriginalItem(int itemID)
        {
            if (itemID >= 0 && itemID < _baseDefinitions.Count)
            {
                return _baseDefinitions[itemID];
            }
            Debug.LogWarning($"[ItemsDataStorageDefinition] Индекс {itemID} находится вне диапазона для _baseDefinitions.");
            return null;
        }
    }
}