using UnityEngine;
using SkyClerik.Inventory;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox; // Добавлено для доступа к ServiceProvider

namespace SkyClerik.Utils
{
    public class LoadService
    {
        /// <summary>
        /// Загружает все игровые данные (состояние и инвентари) для указанного слота.
        /// </summary>
        public void LoadAll(GlobalGameState globalState, ItemContainer[] containersToLoad)
        {
            int slotIndex = globalState.CurrentSaveSlotIndex;
            string slotFolderPath = GetSaveSlotFolderPath(slotIndex);

            // Загружаем глобальное состояние
            LoadGlobalState(globalState, slotFolderPath);

            // Загружаем все переданные контейнеры
            foreach (var container in containersToLoad)
            {
                LoadItemContainer(container, slotFolderPath);
            }
            
            Debug.Log($"Полная загрузка для слота {slotIndex} завершена.");
        }

        /// <summary>
        /// Загружает объект GlobalGameState из указанной папки слота.
        /// </summary>
        public void LoadGlobalState(GlobalGameState globalState, string slotFolderPath)
        {
            string filePath = Path.Combine(slotFolderPath, "globalGameState.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    JsonConvert.PopulateObject(json, globalState, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    Debug.Log($"Глобальное состояние игры загружено из: {filePath}");
                }
                catch (JsonSerializationException ex)
                {
                    Debug.LogError($"Не удалось загрузить глобальное состояние игры из-за ошибки сериализации: {ex.Message}. Используется текущее состояние.", null);
                }
            }
            else
            {
                Debug.LogWarning($"Файл сохранения глобального состояния не найден по пути: {filePath}. Используется текущее состояние.", null);
            }
        }

        /// <summary>
        /// Загружает данные в ItemContainer из JSON файла для указанного слота.
        /// </summary>
        public void LoadItemContainer(ItemContainer targetContainer, string slotFolderPath)
        {
            if (targetContainer == null)
            {
                Debug.LogWarning("Попытка загрузить в пустой (null) ItemContainer.");
                return;
            }

            if (targetContainer.ItemDataStorageSO == null || string.IsNullOrEmpty(targetContainer.ItemDataStorageSO.ContainerGuid))
            {
                Debug.LogError($"GUID целевого контейнера '{targetContainer.name}' не может быть пустым для загрузки!");
                return;
            }

            string containerGuid = targetContainer.ItemDataStorageSO.ContainerGuid;
            string fileName = $"{containerGuid}.json";
            string filePath = Path.Combine(slotFolderPath, fileName);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                // Десериализуем JSON в новый временный ItemContainerDefinition
                ItemContainerDefinition loadedContainerDefinition = Inventory.JsonScriptableObjectSerializer.DeserializeScriptableObject<ItemContainerDefinition>(json);

                if (loadedContainerDefinition != null)
                {
                    Debug.Log($"Контейнер '{containerGuid}' загружен из: {filePath}. Десериализовано {loadedContainerDefinition.Items.Count} предметов.");

                    int itemIndex = 0;
                    foreach (var item in loadedContainerDefinition.Items)
                    {
                        if (item != null)
                        {
                            Debug.Log($"[LoadService]   Загруженный предмет {itemIndex}: Name='{item.DefinitionName}', WrapperIndex={item.WrapperIndex}, Stack={item.Stack}, GridPosition={item.GridPosition}, RuntimeID={item.GetInstanceID()}, Type={item.GetType().Name}");
                        }
                        else
                        {
                            Debug.Log($"[LoadService]   Загруженный предмет {itemIndex}: NULL (Возможно, потеряна ссылка)");
                        }
                        itemIndex++;
                    }

                    // Копируем данные из загруженного определения в существующее
                    // Это важно для ScriptableObject, чтобы сохранить ссылки в Unity
                    targetContainer.ItemDataStorageSO.SetDataFromOtherContainer(loadedContainerDefinition);
                    
                    // После загрузки данных в ItemDataStorageSO, настраиваем логическую сетку контейнера
                    targetContainer.SetupLoadedItemsGrid();

                    // Обновляем визуальные элементы
                    ItemsPage itemsPage = ServiceProvider.Get<ItemsPage>();
                    if (itemsPage != null)
                    {
                        GridPageElementBase targetGridPage = null;
                        if (targetContainer == itemsPage.InventoryItemContainer)
                        {
                            targetGridPage = itemsPage.InventoryPage;
                        }
                        else if (targetContainer == itemsPage.CraftItemContainer)
                        {
                            targetGridPage = itemsPage.CraftPage; // Исправлено на правильное имя свойства
                        }

                        if (targetGridPage != null)
                        {
                            targetGridPage.RefreshVisuals();
                        }
                        else
                        {
                            Debug.LogWarning($"[LoadService] Не удалось найти соответствующий GridPageElementBase для контейнера '{targetContainer.name}'. Визуальные элементы не будут обновлены.");
                        }
                    }
                    else
                    {
                        Debug.LogError("[LoadService] ItemsPage не найден в ServiceProvider. Невозможно обновить визуальные элементы инвентаря.");
                    }
                }
                else
                {
                    Debug.LogError($"Не удалось десериализовать ItemContainerDefinition из файла: {filePath}");
                }
            }
            else
            {
                Debug.LogWarning($"Файл сохранения для контейнера '{containerGuid}' не найден по пути: {filePath}");
            }
        }

        /// <summary>
        /// Получает или создает путь к папке для указанного слота сохранения.
        /// </summary>
        public string GetSaveSlotFolderPath(int slotIndex)
        {
            const string SaveSlotPrefix = "Slot";
            string baseSavePath = Path.Combine(Application.persistentDataPath, "Saves");
            string slotFolderName = $"{SaveSlotPrefix}_{slotIndex}";
            string slotPath = Path.Combine(baseSavePath, slotFolderName);

            if (!Directory.Exists(slotPath))
            {
                Directory.CreateDirectory(slotPath);
            }
            return slotPath;
        }
    }
}
