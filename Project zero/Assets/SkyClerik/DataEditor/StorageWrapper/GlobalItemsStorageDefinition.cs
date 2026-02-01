using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
using Newtonsoft.Json;

namespace SkyClerik.Inventory
{
    [CreateAssetMenu(fileName = "Global Items Storage Definition", menuName = "SkyClerik/Inventory/Global Items Storage Definition")]
    public class GlobalItemsStorageDefinition : ScriptableObject
    {
        [SerializeField]
        private List<ItemBaseDefinition> _baseDefinitions = new List<ItemBaseDefinition>();
        private Dictionary<int, ItemBaseDefinition> _itemsById = new Dictionary<int, ItemBaseDefinition>();

        private void Awake()
        {
            _itemsById.Clear();

            for (int i = 0; i < _baseDefinitions.Count; i++)
            {
                var item = _baseDefinitions[i];
                if (item == null)
                {
                    Debug.LogWarning($"AnyItemsStorageWrapper: Элемент в _baseDefinitions под индексом {i} является null. Пропускаем его.");
                    continue;
                }

                if (!_itemsById.ContainsKey(i))
                    _itemsById.Add(i, item);
                else
                    Debug.LogError($"AnyItemsStorageWrapper: Обнаружен дубликат ID (индекса) '{i}' при инициализации. Этого не должно быть.");
            }
        }

        /// <summary>
        /// Клонирует ItemBaseDefinition с использованием NewtonSoft.Json для глубокого копирования.
        /// </summary>
        /// <param name="original">Оригинальный ItemBaseDefinition для клонирования.</param>
        /// <returns>Клонированный ItemBaseDefinition или null, если оригинал null.</returns>
        private ItemBaseDefinition CloneItem(ItemBaseDefinition original)
        {
            if (original == null)
            {
                Debug.LogWarning("Попытка клонировать null ItemBaseDefinition.");
                return null;
            }

            try
            {
                string json = JsonConvert.SerializeObject(original);
                return JsonConvert.DeserializeObject(json, original.GetType()) as ItemBaseDefinition;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка при клонировании ItemBaseDefinition '{original.name}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Возвращает клонированный ItemBaseDefinition по его индексу (id) в списке.
        /// </summary>
        /// <param name="id">Индекс (id) искомого предмета.</param>
        /// <returns>Клонированный ItemBaseDefinition или null, если предмет не найден.</returns>
        public ItemBaseDefinition GetItem(int id)
        {
            if (_itemsById.TryGetValue(id, out ItemBaseDefinition originalItem))
            {
                var clonedItem = CloneItem(originalItem);
                if (clonedItem != null)
                {
                    clonedItem.WrapperIndex = id;
                }
                return clonedItem;
            }

            Debug.LogWarning($"Искомый предмет с ID '{id}' не найден в словаре _itemsById.");
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

            int index = _baseDefinitions.IndexOf(item);
            if (index == -1)
            {
                Debug.LogWarning($"Искомый предмет '{item.name}' не найден в базовых определениях.");
                return null;
            }

            var clonedItem = CloneItem(item);
            if (clonedItem != null)
            {
                clonedItem.WrapperIndex = index;
            }
            return clonedItem;
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

            if (item.WrapperIndex < 0)
            {
                Debug.LogWarning($"AnyItemsStorageWrapper: Невалидный WrapperIndex ({item.WrapperIndex}) у переданного ItemBaseDefinition для GetItemByWrapperIndex.");
                return null;
            }

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
            Debug.LogWarning($"GlobalItemsStorageDefinition: Индекс {index} находится вне диапазона для _baseDefinitions.");
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
                    var clonedItem = CloneItem(originalItem);
                    if (clonedItem != null)
                    {
                        clonedItem.WrapperIndex = index; // Устанавливаем WrapperIndex для клонированного элемента
                    }
                    return clonedItem;
                }
            }
            Debug.LogWarning($"GlobalItemsStorageDefinition: Индекс {index} находится вне диапазона или предмет по этому индексу null.");
            return null;
        }
    }
}