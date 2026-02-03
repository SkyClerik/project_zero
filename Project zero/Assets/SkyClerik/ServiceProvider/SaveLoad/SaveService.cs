using Newtonsoft.Json;
using SkyClerik.Inventory;
using System.IO;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Utils
{
    /// <summary>
    /// Сервис для сохранения игровых данных, таких как глобальное состояние игры и содержимое контейнеров предметов.
    /// Использует Newtonsoft.Json для сериализации данных.
    /// </summary>
    public class SaveService
    {
        /// <summary>
        /// Сохраняет все игровые данные (состояние и инвентари) для указанного слота.
        /// </summary>
        /// <param name="globalGameProperty">Глобальные свойства игры для сохранения.</param>
        /// <param name="slotIndex">Индекс слота сохранения.</param>
        public void SaveAll(GlobalGameProperty globalGameProperty, int slotIndex)
        {
            var itemsPage = ServiceProvider.Get<ItemsPage>();
            if (itemsPage == null)
                return;

            var slotFolderPath = GetSaveSlotFolderPath(slotIndex);

            foreach (var containerAndPage in itemsPage.ContainersAndPages)
            {
                SaveItemContainer(containerAndPage.Container, slotFolderPath);
            }

            SaveGlobalState(globalGameProperty, slotFolderPath);

            Debug.Log($"Полное сохранение для слота {globalGameProperty.CurrentSaveSlotIndex} завершено.");
        }

        /// <summary>
        /// Сохраняет объект <see cref="GlobalGameProperty"/> в указанную папку слота.
        /// </summary>
        /// <param name="globalProperty">Объект глобальных свойств игры для сохранения.</param>
        /// <param name="slotFolderPath">Путь к папке слота сохранения.</param>
        public void SaveGlobalState(GlobalGameProperty globalProperty, string slotFolderPath)
        {
            string filePath = Path.Combine(slotFolderPath, "globalGameState.json");
            string json = JsonConvert.SerializeObject(globalProperty, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            });

            File.WriteAllText(filePath, json);
            Debug.Log($"Глобальное состояние игры сохранено в: {filePath}");
        }

        /// <summary>
        /// Сохраняет один <see cref="ItemContainer"/> в указанную папку слота.
        /// </summary>
        /// <param name="itemContainer">Контейнер предметов для сохранения.</param>
        /// <param name="slotFolderPath">Путь к папке слота сохранения.</param>
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
                    Debug.Log($"[SaveService]   Предмет {itemIndex}: Name='{item.DefinitionName}', WrapperIndex={item.ID}, Stack={item.Stack}, GridPosition={item.GridPosition}, RuntimeID={item.GetInstanceID()}, Type={item.GetType().Name}");
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
        private string GetSaveSlotFolderPath(int slotIndex)
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
