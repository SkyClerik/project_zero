//using UnityEngine;
//using UnityEngine.UIElements;
//using SkyClerik.Inventory;
//using UnityEngine.DataEditor;

//namespace SkyClerik.EquipmentSystem
//{
//    /// <summary>
//    /// Визуальное представление одного слота экипировки на UI.
//    /// </summary>
//    public class EquipmentSlotVisual : VisualElement
//    {
//        public EquipmentSlot EquipmentSlot { get; private set; }
//        public ItemVisual CurrentItemVisual { get; private set; }

//        private ItemsPage _itemsPage; // Ссылка на главную ItemsPage для взаимодействия

//        public EquipmentSlotVisual(EquipmentSlot equipmentSlot, ItemsPage itemsPage)
//        {
//            EquipmentSlot = equipmentSlot;
//            _itemsPage = itemsPage;

//            // Настроить визуальный стиль слота
//            // Можно добавить фон, рамку, иконку, обозначающую тип слота и т.д.
//            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Пример фона
//            style.borderLeftColor = style.borderTopColor = style.borderRightColor = style.borderBottomColor = Color.gray;
//            style.borderLeftWidth = style.borderTopWidth = style.borderRightWidth = style.borderBottomWidth = 1;

//            // Добавить обработчики событий для перетаскивания
//            this.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
//            this.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
//            this.RegisterCallback<PointerUpEvent>(OnPointerUp);
//        }

//        /// <summary>
//        /// Устанавливает предмет в слот, создавая или удаляя его визуальное представление.
//        /// </summary>
//        /// <param name="item">Предмет для установки (null, чтобы очистить слот).</param>
//        public void SetItem(ItemBaseDefinition item)
//        {
//            // Удаляем старое визуальное представление, если оно есть
//            if (CurrentItemVisual != null)
//            {
//                CurrentItemVisual.RemoveFromHierarchy();
//                CurrentItemVisual = null;
//            }

//            // Создаем новое визуальное представление, если item не null
//            if (item != null)
//            {
//                // ItemsPage.CurrentDraggedItem будет установлен, когда ItemVisual создается
//                // Здесь ownerInventory_ это null, так как слот экипировки не является ItemContainer
//                CurrentItemVisual = new ItemVisual(
//                    _itemsPage,
//                    ownerInventory: null, 
//                    itemDefinition: item,
//                    gridPosition: Vector2Int.zero, 
//                    gridSize: new Vector2Int(item.Dimensions.Width, item.Dimensions.Height));

//                // Устанавливаем размер ItemVisual так, чтобы он вписывался в слот
//                CurrentItemVisual.style.width = EquipmentSlot.UiSize.x;
//                CurrentItemVisual.style.height = EquipmentSlot.UiSize.y;
//                CurrentItemVisual.SetPosition(Vector2.zero); // Внутри слота всегда в начале

//                // Добавляем ItemVisual в иерархию слота
//                Add(CurrentItemVisual);
//            }
//        }

//        private void OnPointerEnter(PointerEnterEvent evt)
//        {
//            // Обработчик наведения курсора
//            // Например, можно изменить стиль слота или показать тултип
//            // _itemsPage.StartTooltipDelay(CurrentItemVisual); // если нужно показывать тултип для экипированного предмета
//        }

//        private void OnPointerLeave(PointerLeaveEvent evt)
//        {
//            // Обработчик убирания курсора
//            // _itemsPage.StopTooltipDelayAndHideTooltip();
//        }

//        private void OnPointerUp(PointerUpEvent evt)
//        {
//            // Обработчик отпускания кнопки мыши
//            // Это событие будет перехватываться ItemsPage для обработки Drop
//            if (ItemsPage.CurrentDraggedItem != null) // Исправлено
//            {
//                // ItemsPage будет обрабатывать TransferItemBetweenContainers
//            }
//            else if (CurrentItemVisual != null)
//            {
//                // Если нет перетаскиваемого предмета, но есть предмет в слоте,
//                // это может быть начало перетаскивания (PickUp)
//                ItemsPage.CurrentDraggedItem = CurrentItemVisual; // Исправлено
//                // Устанавливаем владельца для CurrentDraggedItem
//                ItemsPage.CurrentDraggedItem.SetOwnerInventory(_itemsPage.GetEquipmentPageElement()); // Исправлено
                
//                // Временно, чтобы убрать ItemVisual из слота сразу при поднятии
//                Remove(CurrentItemVisual);
//            }
//        }
//    }
//}