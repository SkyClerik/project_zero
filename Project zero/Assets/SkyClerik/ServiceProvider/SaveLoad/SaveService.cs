using Newtonsoft.Json;
using SkyClerik.Inventory;
using System.IO;
using UnityEngine;
using UnityEngine.Toolbox;
using System.Collections.Generic; // Добавлено для использования List

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
            var inventoryStorage = ServiceProvider.Get<InventoryStorage>();
            var questsContainer = ServiceProvider.Get<SkyClerik.QuestsContainer>(); // Получаем экземпляр QuestsContainer

            if (inventoryStorage == null && questsContainer == null)
                return;

            var slotFolderPath = GetSaveSlotFolderPath(slotIndex);

            if (inventoryStorage != null)
            {
                foreach (var containerAndPage in inventoryStorage.ContainersAndPages)
                {
                    SaveItemContainer(containerAndPage.Container, slotFolderPath);
                }
            }

            if (questsContainer != null)
            {
                SaveQuestsContainer(questsContainer, slotFolderPath); // Вызываем новый метод для сохранения квестов
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

            if (itemContainer.ContainerDefinition == null || string.IsNullOrEmpty(itemContainer.ContainerDefinition.ContainerGuid))
            {
                Debug.LogError($"GUID контейнера '{itemContainer.name}' не может быть пустым для сохранения!");
                return;
            }

            string fileName = $"{itemContainer.ContainerDefinition.ContainerGuid}.json";
            string filePath = Path.Combine(slotFolderPath, fileName);

            string json = Inventory.JsonScriptableObjectSerializer.SerializeScriptableObject(itemContainer.ContainerDefinition);
            File.WriteAllText(filePath, json);
            Debug.Log($"Контейнер '{itemContainer.ContainerDefinition.ContainerGuid}' сохранен в: {filePath}");
            Debug.Log($"[SaveService] Сериализован контейнер '{itemContainer.ContainerDefinition.name}' с {itemContainer.ContainerDefinition.Items.Count} предметами.");

            int itemIndex = 0;
            foreach (var item in itemContainer.ContainerDefinition.Items)
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
        /// Сохраняет данные квестов из <see cref="QuestsContainer"/> в указанную папку слота.
        /// </summary>
        /// <param name="questsContainer">Контейнер квестов для сохранения.</param>
        /// <param name="slotFolderPath">Путь к папке слота сохранения.</param>
        public void SaveQuestsContainer(SkyClerik.QuestsContainer questsContainer, string slotFolderPath)
        {
            if (questsContainer == null)
            {
                Debug.LogWarning("Попытка сохранить пустой (null) QuestsContainer.");
                return;
            }

            string fileName = "quests.json";
            string filePath = Path.Combine(slotFolderPath, fileName);

            // Создаем временный анонимный объект для сериализации обоих списков
            var questDataToSerialize = new
            {
                Relations = questsContainer.relations,
                Quests = questsContainer.quests
            };

            string json = JsonConvert.SerializeObject(questDataToSerialize, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            });

            File.WriteAllText(filePath, json);
            Debug.Log($"Данные квестов сохранены в: {filePath}");
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
