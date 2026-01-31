using System.IO;
using UnityEngine;

namespace SkyClerik.Inventory
{
    public static class SaveAndLoadPlayerItems
    {
        private const string SaveSlotPrefix = "Slot";

        private static string GetSaveSlotFolderPath(int slotIndex)
        {
            string baseSavePath = Path.Combine(Application.persistentDataPath, "Saves");
            string slotFolderName = $"{SaveSlotPrefix}_{slotIndex}";
            string slotPath = Path.Combine(baseSavePath, slotFolderName);

            if (!Directory.Exists(slotPath))
            {
                Directory.CreateDirectory(slotPath);
            }
            return slotPath;
        }

        /// <summary>
        /// Сохраняет ItemContainerBase в JSON файл для указанного слота.
        /// </summary>
        /// <param name="itemContainer">Контейнер предметов для сохранения.</param>
        /// <param name="slotIndex">Индекс слота сохранения.</param>
        public static void SaveContainer(ItemContainerBase itemContainer, int slotIndex)
        {
            if (string.IsNullOrEmpty(itemContainer.ContainerGuid))
            {
                Debug.LogError("GUID контейнера не может быть пустым для сохранения!");
                return;
            }

            string slotFolderPath = GetSaveSlotFolderPath(slotIndex);
            if (string.IsNullOrEmpty(slotFolderPath)) return;

            string fileName = $"{itemContainer.ContainerGuid}.json";
            string filePath = Path.Combine(slotFolderPath, fileName);

            string json = itemContainer.SaveItemsToJson();
            File.WriteAllText(filePath, json);
            Debug.Log($"Контейнер '{itemContainer.ContainerGuid}' для слота {slotIndex} сохранен в: {filePath}");
        }

        /// <summary>
        /// Загружает данные в ItemContainerBase из JSON файла для указанного слота.
        /// </summary>
        /// <param name="targetContainer">Целевой контейнер, в который будут загружены данные. Его GUID используется для поиска файла.</param>
        /// <param name="slotIndex">Индекс слота сохранения.</param>
        public static void LoadContainer(ItemContainerBase targetContainer, int slotIndex)
        {
            if (string.IsNullOrEmpty(targetContainer.ContainerGuid))
            {
                Debug.LogError("GUID целевого контейнера не может быть пустым для загрузки!");
                return;
            }

            string containerGuid = targetContainer.ContainerGuid;

            string slotFolderPath = GetSaveSlotFolderPath(slotIndex);
            if (string.IsNullOrEmpty(slotFolderPath)) return;

            string fileName = $"{containerGuid}.json";
            string filePath = Path.Combine(slotFolderPath, fileName);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                targetContainer.LoadItemsFromJson(json);
                Debug.Log($"Контейнер '{containerGuid}' для слота {slotIndex} загружен из: {filePath}");
            }
            else
            {
                Debug.LogWarning($"Файл сохранения для контейнера '{containerGuid}' в слоте {slotIndex} не найден по пути: {filePath}");
            }
        }
    }
}
