using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;
// using Newtonsoft.Json; // Удалено, так как больше не используется для клонирования

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
        /// Клонирует ItemBaseDefinition с использованием ScriptableObject.Instantiate() для создания независимого экземпляра.
        /// Затем вручную копирует необходимые поля.
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

            // Создаем новый, независимый экземпляр ScriptableObject
            ItemBaseDefinition clonedItem = Instantiate(original);
            clonedItem.name = original.name; // Instatiate добавляет "(Clone)", убираем это.

            // Копируем все необходимые поля вручную, так как Instantiate создает "чистый" клон
            // и не копирует напрямую приватные поля или свойства с protected set.
            // Примечание: Stack и GridPosition копируются позже в ItemContainerDefinition.SetDataFromOtherContainer
            // из десериализованных данных. Здесь копируем базовые данные, не изменяющиеся экземпляром.

            // Копируем поля из BaseDefinition
            clonedItem.ID = original.ID;
            clonedItem.DefinitionName = original.DefinitionName;
            clonedItem.Description = original.Description;
            clonedItem.Icon = original.Icon;

            // Копируем поля из ItemBaseDefinition
            clonedItem.WrapperIndex = original.WrapperIndex;
            clonedItem.Price = original.Price;
            clonedItem.MaxStack = original.MaxStack;
            clonedItem.Stackable = original.Stackable;
            clonedItem.ViewStackable = original.ViewStackable;

            // Если ItemDimensions - это class, который должен быть глубоко скопирован, а не просто ссылкой:
            if (original.Dimensions != null)
            {
                clonedItem.Dimensions = new ItemDimensions
                {
                    DefaultWidth = original.Dimensions.DefaultWidth,
                    DefaultHeight = original.Dimensions.DefaultHeight,
                    DefaultAngle = original.Dimensions.DefaultAngle,
                    CurrentWidth = original.Dimensions.CurrentWidth,
                    CurrentHeight = original.Dimensions.CurrentHeight,
                    CurrentAngle = original.Dimensions.CurrentAngle
                };
            } else {
                clonedItem.Dimensions = null;
            }
            
            // Stack и GridPosition не копируются здесь, так как они будут установлены из десериализованных данных
            // в ItemContainerDefinition.SetDataFromOtherContainer.

            return clonedItem;
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