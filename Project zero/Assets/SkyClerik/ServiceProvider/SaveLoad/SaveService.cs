using UnityEngine;
using SkyClerik.Inventory;
using System.IO;
using Newtonsoft.Json;

namespace SkyClerik.Utils
{
    public class SaveService
    {
        /// <summary>
        /// Сохраняет все игровые данные (состояние и инвентари) для указанного слота.
        /// </summary>
        public void SaveAll(GlobalGameState globalState, ItemContainer[] containersToSave)
        {
            int slotIndex = globalState.CurrentSaveSlotIndex;
            string slotFolderPath = GetSaveSlotFolderPath(slotIndex);

            // Сохраняем глобальное состояние
            SaveGlobalState(globalState, slotFolderPath);

            // Сохраняем все переданные контейнеры
            foreach (var container in containersToSave)
            {
                SaveItemContainer(container, slotFolderPath);
            }

            Debug.Log($"Полное сохранение для слота {slotIndex} завершено.");
        }

        /// <summary>
        /// Сохраняет объект GlobalGameState в указанную папку слота.
        /// </summary>
        public void SaveGlobalState(GlobalGameState globalState, string slotFolderPath)
        {
            string filePath = Path.Combine(slotFolderPath, "globalGameState.json");
            string json = JsonConvert.SerializeObject(globalState, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            });

            File.WriteAllText(filePath, json);
            Debug.Log($"Глобальное состояние игры сохранено в: {filePath}");
        }

        /// <summary>
        /// Сохраняет один ItemContainer в указанную папку слота.
        /// </summary>
        public void SaveItemContainer(ItemContainer itemContainer, string slotFolderPath)
        {
            if (itemContainer == null)
            {
                Debug.LogWarning("Попытка сохранить пустой (null) ItemContainer.");
                return;
            }

            if (itemContainer.ItemDataStorageSO == null || string.IsNullOrEmpty(itemContainer.ItemDataStorageSO.ContainerGuid))
            {
                Debug.LogError($"GUID контейнера '{itemContainer.name}' не может быть пустым для сохранения!");
                return;
            }

            string fileName = $"{itemContainer.ItemDataStorageSO.ContainerGuid}.json";
            string filePath = Path.Combine(slotFolderPath, fileName);

            string json = Inventory.JsonScriptableObjectSerializer.SerializeScriptableObject(itemContainer.ItemDataStorageSO);
            File.WriteAllText(filePath, json);
            Debug.Log($"Контейнер '{itemContainer.ItemDataStorageSO.ContainerGuid}' сохранен в: {filePath}");
            Debug.Log($"[SaveService] Сериализован контейнер '{itemContainer.ItemDataStorageSO.name}' с {itemContainer.ItemDataStorageSO.Items.Count} предметами.");

            int itemIndex = 0;
            foreach (var item in itemContainer.ItemDataStorageSO.Items)
            {
                if (item != null)
                {
                    Debug.Log($"[SaveService]   Предмет {itemIndex}: Name='{item.DefinitionName}', WrapperIndex={item.WrapperIndex}, Stack={item.Stack}, GridPosition={item.GridPosition}, RuntimeID={item.GetInstanceID()}, Type={item.GetType().Name}");
                }
                else
                {
                    Debug.Log($"[SaveService]   Предмет {itemIndex}: NULL (Возможно, потеряна ссылка)");
                }
                itemIndex++;
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
