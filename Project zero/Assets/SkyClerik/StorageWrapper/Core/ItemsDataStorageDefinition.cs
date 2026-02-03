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

        // Awake больше не нужен, так как словарь _itemsById удален.
        // public void Awake() {}

        /// <summary>
        /// Возвращает клонированный <see cref="ItemBaseDefinition"/> по его индексу (id) в списке.
        /// </summary>
        /// <param name="id">Индекс (id) искомого предмета.</param>
        /// <returns>Клонированный <see cref="ItemBaseDefinition"/> или null, если предмет не найден или индекс вне диапазона.</returns>
        public ItemBaseDefinition GetItem(int id)
        {
            if (id >= 0 && id < _baseDefinitions.Count)
            {
                ItemBaseDefinition originalItem = _baseDefinitions[id];
                if (originalItem != null)
                {
                    return originalItem.Clone();
                }
                Debug.LogWarning($"[ItemsDataStorageDefinition] Предмет по ID '{id}' найден, но его определение равно null.");
                return null;
            }

            Debug.LogWarning($"[ItemsDataStorageDefinition] Искомый предмет с ID '{id}' не найден (индекс вне диапазона).");
            return null;
        }

        /// <summary>
        /// Возвращает клонированный <see cref="ItemBaseDefinition"/> по ссылке на оригинальный <see cref="ItemBaseDefinition"/>.
        /// Этот метод выполняет линейный поиск по ссылке объекта в базовом списке <c>_baseDefinitions</c>.
        /// Если предмет был ранее клонирован и его <c>WrapperIndex</c> известен, рекомендуется использовать <see cref="GetItemByWrapperIndex(ItemBaseDefinition)"/> для более быстрого поиска.
        /// </summary>
        /// <param name="item">Оригинальный <see cref="ItemBaseDefinition"/> для поиска.</param>
        /// <returns>Клонированный <see cref="ItemBaseDefinition"/> или null, если предмет не найден.</returns>
        public ItemBaseDefinition GetItem(ItemBaseDefinition item)
        {
            if (item == null)
            {
                Debug.LogWarning("Попытка получить предмет по null ссылке.");
                return null;
            }

            // Теперь этот метод может просто использовать WrapperIndex для получения элемента
            return GetItemByWrapperIndex(item);
        }

        /// <summary>
        /// Возвращает клонированный <see cref="ItemBaseDefinition"/>, используя его <c>WrapperIndex</c> для быстрого поиска.
        /// Этот метод предназначен для случаев, когда у <see cref="ItemBaseDefinition"/> уже установлен надежный <c>WrapperIndex</c>,
        /// что позволяет выполнить поиск за константное время (O(1)).
        /// </summary>
        /// <param name="item"><see cref="ItemBaseDefinition"/>, содержащий <c>WrapperIndex</c> для поиска.</param>
        /// <returns>Клонированный <see cref="ItemBaseDefinition"/> или null, если предмет не найден по <c>WrapperIndex</c>.</returns>
        public ItemBaseDefinition GetItemByWrapperIndex(ItemBaseDefinition item)
        {
            if (item == null)
            {
                Debug.LogWarning("Попытка получить предмет по null ссылке.");
                return null;
            }
            // Теперь просто вызываем GetItem(int id)
            return GetItem(item.WrapperIndex);
        }

        /// <summary>
        /// Возвращает <see cref="ItemBaseDefinition"/> из внутреннего списка <c>_baseDefinitions</c> по указанному индексу.
        /// Этот метод предоставляет прямой доступ к оригинальному объекту <see cref="ItemBaseDefinition"/> из списка,
        /// что может быть полезно для чтения данных или для операций, не требующих клонирования.
        /// Возвращает null, если индекс находится вне диапазона.
        /// </summary>
        /// <param name="index">Индекс <see cref="ItemBaseDefinition"/> в списке <c>_baseDefinitions</c>.</param>
        /// <returns>Оригинальный <see cref="ItemBaseDefinition"/> по указанному индексу или null, если индекс невалиден.</returns>
        public ItemBaseDefinition GetOriginalItemByIndex(int index)
        {
            if (index >= 0 && index < _baseDefinitions.Count)
            {
                return _baseDefinitions[index];
            }
            Debug.LogWarning($"[ItemsDataStorageDefinition] Индекс {index} находится вне диапазона для _baseDefinitions.");
            return null;
        }

        /// <summary>
        /// Возвращает клонированный <see cref="ItemBaseDefinition"/> из внутреннего списка <c>_baseDefinitions</c> по указанному индексу.
        /// Этот метод обеспечивает безопасное получение независимой копии предмета,
        /// что предотвращает случайные изменения оригинальных данных в ScriptableObject.
        /// Возвращает null, если индекс находится вне диапазона.
        /// </summary>
        /// <param name="index">Индекс <see cref="ItemBaseDefinition"/> в списке <c>_baseDefinitions</c>.</param>
        /// <returns>Клонированный <see cref="ItemBaseDefinition"/> по указанному индексу или null, если индекс невалиден.</returns>
        public ItemBaseDefinition GetClonedItemByIndex(int index)
        {
            if (index >= 0 && index < _baseDefinitions.Count)
            {
                ItemBaseDefinition originalItem = _baseDefinitions[index];
                if (originalItem != null)
                {
                    return originalItem.Clone();
                }
            }
            Debug.LogWarning($"[ItemsDataStorageDefinition] Индекс {index} находится вне диапазона или предмет по этому индексу null.");
            return null;
        }
    }
}