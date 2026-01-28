using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using SkyClerik.CraftingSystem;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class CraftPageElement : GridPageElementBase
    {
        private const string _craftPageTitleText = "Окно крафта предметов";
        private const string _craftRootID = "craft_root";
        private VisualElement _header;
        private const string _headerID = "header";
        private Label _title;
        private const string _titleID = "l_title";
        private VisualElement _body;
        private const string _bodyID = "body";
        private Button _craftButton;
        private const string _craftButtonID = "b_craft";

        public CraftPageElement(ItemsPage itemsPage, UIDocument document, ItemContainerBase itemContainer, Vector2 cellSize, Vector2Int inventoryGridSize)
            : base(itemsPage, document, itemContainer, cellSize, inventoryGridSize, _craftRootID)
        {
            _header = _root.Q(_headerID); 
            _title = _header.Q<Label>(_titleID);
            _body = _root.Q(_bodyID);
            _craftButton = _body.Q<Button>(_craftButtonID);

            _title.text = _craftPageTitleText;
            _craftButton.clicked += _craftButton_clicked;
        }

        // Специфичная логика для крафта
        private void _craftButton_clicked()
        {
            var craftSystem = ServiceProvider.Get<ICraftingSystem>();
            if (craftSystem == null)
            {
                Debug.LogError("Система крафта не найдена!");
                return;
            }

            // Собираем все предметы, которые сейчас лежат в сетке крафта
            var itemsInGridDefinitions = _placedItemsGridData.Keys.Select(visual => visual.ItemDefinition).ToList();
            
            if (craftSystem.TryFindRecipe(itemsInGridDefinitions, out var foundRecipe))
            {
                Debug.Log($"Найден рецепт! Результат: {foundRecipe.Result.Item.DefinitionName}");

                // 1. Уничтожаем визуальные элементы ингредиентов и освобождаем ячейки
                foreach (var entry in _visualToGridDataMap.ToList())
                {
                    UnregisterVisual(entry.Key);
                    OccupyGridCells(entry.Value, false);
                    entry.Key.RemoveFromHierarchy();
                }
                _placedItemsGridData.Clear();
                _itemContainer.Clear();

                // 2. Создаем результирующий предмет
                var resultItem = _itemContainer.AddItemAsClone(foundRecipe.Result.Item);
                resultItem.Stack = foundRecipe.Result.Quantity;

                if (resultItem != null)
                {
                    // 3. Пытаемся разместить результат в сетке
                    if (TryFindPlacement(resultItem, out Vector2Int gridPosition))
                    {
                        ItemGridData newGridData = new ItemGridData(resultItem, gridPosition);
                        ItemVisual resultVisual = new ItemVisual(
                            itemsPage: _itemsPage,
                            ownerInventory: this,
                            itemDefinition: resultItem,
                            gridPosition: gridPosition,
                            gridSize: newGridData.GridSize);

                        _placedItemsGridData.Add(resultVisual, newGridData);
                        
                        resultVisual.UpdatePcs();
                        
                        AddItemToInventoryGrid(resultVisual);
                        RegisterVisual(resultVisual, newGridData);
                        resultVisual.SetPosition(new Vector2(gridPosition.x * _cellSize.width, gridPosition.y * _cellSize.height));
                    }
                    else
                    {
                        Debug.LogError("В сетке крафта нет места для результата! Хотя такого не может быть при правильной логике рецептов.");
                    }
                }
            }
            else
            {
                Debug.Log("Такого рецепта не существует.");
            }
        }
        
        protected override void CalculateGridRect()
        {
            _gridRect = _inventoryGrid.worldBound;
            _gridRect.width = (_cellSize.width * _inventoryDimensions.width) + (_cellSize.width / 2);
            _gridRect.height = (_cellSize.height * _inventoryDimensions.height) + (_cellSize.height / 2);
            _gridRect.x -= (_cellSize.width / 4);
            _gridRect.y -= (_cellSize.height / 4);
        }
    }
}