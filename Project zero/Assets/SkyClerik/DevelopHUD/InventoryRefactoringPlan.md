# План рефакторинга добавления предметов в инвентарь

**Цель:** Реализовать логику размещения предметов в инвентаре, где позиция (GridPosition) записывается в ItemBaseDefinition только после успешного нахождения места в логической матрице инвентаря. UI-представление (ItemVisual) создается только при активном UI.

## 1. Откатить предыдущие ошибочные изменения

*   **ItemContainerBase.cs**: Убедиться, что метод `AddItemAsClone(ItemBaseDefinition item)` не присваивает `copy.GridPosition = item.GridPosition;`. (Уже сделано в предыдущем шаге.)
*   **GridPageElementBase.cs**:
    *   Убедиться, что `LoadInventory()` находится в своем исходном состоянии (без "мудрёной" логики).
    *   Убедиться, что `AddStoredItem(ItemVisual storedItem, Vector2Int gridPosition)` не устанавливает `storedItem.ItemDefinition.GridPosition`.

## 2. Модификация `GridPageElementBase.cs`

### 2.1. Добавление метода `List<ItemBaseDefinition> AddItemsToGrid(List<ItemBaseDefinition> itemsToAdd)`

**Местоположение:** После `public void AddItemToInventoryGrid(VisualElement item)` и перед `public void AddLoot(ItemContainerBase sourceContainer)`.

**Функционал:**
*   Принимает `List<ItemBaseDefinition> itemsToAdd` (список предметов для размещения).
*   Создает `List<ItemBaseDefinition> unplacedItems` (для предметов, которым не нашлось места).
*   Сортирует `itemsToAdd` по убыванию размера (ширина * высота) для эффективного размещения.
*   **Для каждого `item` в отсортированном списке:**
    *   Определяет `itemGridSize` из `item.Dimensions.CurrentWidth` и `item.Dimensions.CurrentHeight`.
    *   Пытается найти свободное место в логической матрице `_gridOccupancy` с помощью `TryFindPlacement(item, out foundPosition)`.
    *   **ЕСЛИ МЕСТО НАЙДЕНО (`TryFindPlacement` вернул `true`):**
        *   **Установить `item.GridPosition = foundPosition;`** (ЭТО КЛЮЧЕВОЙ МОМЕНТ: предмет получает свою позицию).
        *   Отметить ячейки в `_gridOccupancy` как занятые, используя `OccupyGridCells(new ItemGridData(item, foundPosition), true);`.
        *   **Проверить, активен ли UI:** (`if (_inventoryGrid != null && _inventoryGrid.visible`)
            *   Создать `ItemGridData` на основе `item` и `foundPosition`.
            *   Создать `ItemVisual` для этого предмета, передавая `item`, `foundPosition` и `gridSize`.
            *   Добавить `ItemVisual` в `_placedItemsGridData`.
            *   Добавить `ItemVisual` в `_inventoryGrid`.
            *   Установить позицию `ItemVisual` на экране на основе `foundPosition` и `_cellSize`.
    *   **ЕСЛИ МЕСТО НЕ НАЙДЕНО (`TryFindPlacement` вернул `false`):**
        *   Добавить `item` в `unplacedItems`.
*   Возвращает `unplacedItems`.

### 2.2. Модификация `protected IEnumerator LoadInventory()`

*   Удалить всю старую логику размещения предметов (цикл `foreach` и `TryFindPlacement` внутри него).
*   Вместо этого вызвать `AddItemsToGrid(_itemContainer.GetItems().ToList())`.
*   Обработать возвращенный `List<ItemBaseDefinition> unplacedItems` (например, вывести `Debug.LogWarning` о неразмещенных предметах).

### 2.3. Модификация `private bool TryAddItemInternal(ItemBaseDefinition itemToAdd)`

*   Убрать из этого метода всю логику поиска места и создания `ItemVisual`.
*   Сделать так, чтобы он вызывал `AddItemsToGrid(new List<ItemBaseDefinition> { itemToAdd })`.
*   Возвращать `true` или `false` в зависимости от того, был ли предмет добавлен (`AddItemsToGrid` вернул пустой список или нет).
*   Сохранить логику стакования предметов в начале метода.

## 3. Проверка `ItemsPage.cs`

*   Убедиться, что `TransferItemBetweenContainers` по-прежнему содержит строку `itemToMove.GridPosition = gridPosition;`. Это изменение было сделано ранее и оно корректно для обновления позиции предмета после перетаскивания.