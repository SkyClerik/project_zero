# План рефакторинга системы инвентаря

## Цель

Разделить **логическую модель** инвентаря (данные о сетке и предметах) и его **визуальное представление** (UI элементы). Это позволит работать с логикой инвентаря независимо от UI, повысит стабильность, гибкость и чистоту кода, а также решит текущие проблемы с позиционированием.

## Важное замечание по процессу

При рефакторинге существующих файлов, старый код не будет удаляться полностью. Вместо этого, старые методы, которые подвергаются значительным изменениям, будут переименованы с постфиксом `_old` или закомментированы. Это позволит обращаться к исходной логике в процессе работы и обеспечит дополнительную безопасность.

## Ключевые файлы, затронутые рефакторингом

- `D:\Developing\ProjectManager\Data\Projects\Project zero\Project zero\Assets\SkyClerik\ItemsSystem\Pages\GridPageElementBase.cs`
- `D:\Developing\ProjectManager\Data\Projects\Project zero\Project zero\Assets\SkyClerik\ItemsSystem\Scripts\ItemContainerBase.cs`
- `D:\Developing\ProjectManager\Data\Projects\Project zero\Project zero\Assets\SkyClerik\ItemsSystem\Pages\ItemsPage.cs`
- `D:\Developing\ProjectManager\Data\Projects\Project zero\Project zero\Assets\SkyClerik\ItemsSystem\Pages\ItemVisual.cs`
- (Новый файл) `D:\Developing\ProjectManager\Data\Projects\Project zero\Project zero\Assets\SkyClerik\ItemsSystem\Scripts\Core\InventoryGridModel.cs`

---

## План действий

### Шаг 1: Создание `InventoryGridModel` (Логическая Модель)

Будет создан новый C# класс `InventoryGridModel`, который не будет связан с UI (не `MonoBehaviour` и не `VisualElement`). Это будет "мозг" сетки.

- **Расположение:** `Assets/SkyClerik/ItemsSystem/Scripts/Core/InventoryGridModel.cs`
- **Поля:**
  - `private readonly bool[,] _occupancyMatrix;` - Матрица занятости ячеек.
  - `private readonly Dictionary<ItemBaseDefinition, Vector2Int> _itemPositions;` - Словарь для хранения позиций каждого экземпляра предмета.
- **Публичные методы:**
  - `InventoryGridModel(int width, int height)` - Конструктор.
  - `bool IsAreaFree(Vector2Int position, Vector2Int size)` - Проверка, свободна ли область.
  - `bool TryFindPlacement(ItemBaseDefinition item, out Vector2Int position)` - Поиск свободного места для предмета.
  - `void PlaceItem(ItemBaseDefinition item, Vector2Int position)` - Размещение предмета в модели.
  - `void RemoveItem(ItemBaseDefinition item)` - Удаление предмета из модели.

### Шаг 2: Рефакторинг `ItemContainerBase.cs`

Класс `ItemContainerBase` станет владельцем `InventoryGridModel`.

- В `ItemContainerBase` будет добавлено приватное поле `private InventoryGridModel _gridModel;` и публичное свойство для доступа к нему.
- Методы `AddItem`, `RemoveItem`, `Initialize` и т.д. будут обновлены для синхронной работы с `_gridModel`. Например, при добавлении предмета в список `_items`, он также будет размещаться в `_gridModel`.

### Шаг 3: Рефакторинг `GridPageElementBase.cs` (Визуальное Представление)

Этот класс будет значительно "облегчён" и станет чисто визуальным контроллером.

- Будут **удалены** поля: `_gridOccupancy`, `_placedItemsGridData`, `_visualToGridDataMap`.
- `GridPageElementBase` больше не будет хранить состояние сетки.
- Методы, требующие логических проверок (`ShowPlacementTarget`, `TryAddItemInternal`), будут обращаться к `_itemContainer.GridModel` для получения данных (например, `_itemContainer.GridModel.IsAreaFree(...)`).
- `LoadInventory` будет читать позиции предметов из `_itemContainer.GridModel` для первоначальной отрисовки.

---

### Последовательность работ

1.  Создать и реализовать класс `InventoryGridModel.cs`.
2.  Интегрировать `InventoryGridModel` в `ItemContainerBase.cs`.
3.  Переписать `GridPageElementBase.cs`, удалив из него логику данных и заставив его использовать `_itemContainer.GridModel`.
4.  Адаптировать `ItemVisual.cs` и `ItemsPage.cs` под новую архитектуру, если потребуется.
