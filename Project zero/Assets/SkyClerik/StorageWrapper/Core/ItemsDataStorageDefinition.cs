using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    [CreateAssetMenu(fileName = "Global Items Storage Definition", menuName = "SkyClerik/Inventory/Global Items Storage Definition")]
    public class ItemsDataStorageDefinition : ScriptableObject
    {
        [SerializeField]
        private List<ItemBaseDefinition> _baseDefinitions = new List<ItemBaseDefinition>();

        // Awake больше не нужен, так как словарь _itemsById удален.
        // public void Awake() {}

        /// <summary>
        /// Возвращает клонированный ItemBaseDefinition по его индексу (id) в списке.
        /// </summary>
        /// <param name="id">Индекс (id) искомого предмета.</param>
        /// <returns>Клонированный ItemBaseDefinition или null, если предмет не найден или индекс вне диапазона.</returns>
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
        /// Возвращает клонированный ItemBaseDefinition по ссылке на оригинальный ItemBaseDefinition.
        /// Этот метод выполняет линейный поиск по ссылке объекта в базовом списке `_baseDefinitions`.
        /// Если предмет был ранее клонирован и его `WrapperIndex` известен, рекомендуется использовать `GetItemByWrapperIndex` для более быстрого поиска.
        /// </summary>
        /// <param name="item">Оригинальный ItemBaseDefinition для поиска.</param>
        /// <returns>Клонированный ItemBaseDefinition или null, если предмет не найден.</returns>
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
        /// Возвращает клонированный ItemBaseDefinition, используя его WrapperIndex для быстрого поиска.
        /// Этот метод предназначен для случаев, когда у ItemBaseDefinition уже установлен надежный WrapperIndex,
        /// что позволяет выполнить поиск за константное время (O(1)).
        /// </summary>
        /// <param name="item">ItemBaseDefinition, содержащий WrapperIndex для поиска.</param>
        /// <returns>Клонированный ItemBaseDefinition или null, если предмет не найден по WrapperIndex.</returns>
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
        /// Возвращает ItemBaseDefinition из внутреннего списка _baseDefinitions по указанному индексу.
        /// Этот метод предоставляет прямой доступ к оригинальному объекту ItemBaseDefinition из списка,
        /// что может быть полезно для чтения данных или для операций, не требующих клонирования.
        /// Возвращает null, если индекс находится вне диапазона.
        /// </summary>
        /// <param name="index">Индекс ItemBaseDefinition в списке _baseDefinitions.</param>
        /// <returns>Оригинальный ItemBaseDefinition по указанному индексу или null, если индекс невалиден.</returns>
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
        /// Возвращает клонированный ItemBaseDefinition из внутреннего списка _baseDefinitions по указанному индексу.
        /// Этот метод обеспечивает безопасное получение независимой копии предмета,
        /// что предотвращает случайные изменения оригинальных данных в ScriptableObject.
        /// Возвращает null, если индекс находится вне диапазона.
        /// </summary>
        /// <param name="index">Индекс ItemBaseDefinition в списке _baseDefinitions.</param>
        /// <returns>Клонированный ItemBaseDefinition по указанному индексу или null, если индекс невалиден.</returns>
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