using UnityEngine;
using SkyClerik.Inventory;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Toolbox;

namespace SkyClerik.Utils
{
    /// <summary>
    /// Сервис для загрузки игровых данных, таких как глобальное состояние игры и содержимое контейнеров предметов.
    /// Использует Newtonsoft.Json для десериализации данных.
    /// </summary>
    public class LoadService
    {
        /// <summary>
        /// Загружает все игровые данные (состояние и инвентари) для указанного <see cref="GlobalGameProperty"/>.
        /// </summary>
        /// <param name="globalGameProperty">Глобальные свойства игры для загрузки.</param>
        /// <param name="slotFolderPath">Путь к папке слота сохранения/загрузки.</param>
        public void LoadAll(GlobalGameProperty globalGameProperty, string slotFolderPath)
        {
            var itemsPage = ServiceProvider.Get<InventoryContainer>();
            if (itemsPage == null)
                return;

            // Загружаем данные
            foreach (var containerAndPage in itemsPage.ContainersAndPages)
                LoadItemContainer(containerAndPage.Container, slotFolderPath);

            // Обновляем визуальные элементы после загрузки всех контейнеров
            foreach (var containerAndPage in itemsPage.ContainersAndPages)
                containerAndPage.Page.RefreshVisuals();

            //Debug.Log($"Полная загрузка для слота {globalGameProperty.CurrentSaveSlotIndex} завершена.");
        }

        /// <summary>
        /// Загружает объект <see cref="GlobalGameProperty"/> из указанной папки слота.
        /// </summary>
        /// <param name="globalGameProperty">Объект глобальных свойств игры, в который будут загружены данные.</param>
        /// <param name="slotFolderPath">Путь к папке слота сохранения/загрузки.</param>
        public void LoadGlobalState(GlobalGameProperty globalGameProperty, string slotFolderPath)
        {
            string filePath = Path.Combine(slotFolderPath, "globalGameState.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    JsonConvert.PopulateObject(json, globalGameProperty, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    //Debug.Log($"Глобальное состояние игры загружено из: {filePath}");
                }
                catch (JsonSerializationException ex)
                {
                    //Debug.LogError($"Не удалось загрузить глобальное состояние игры из-за ошибки сериализации: {ex.Message}. Используется текущее состояние.", null);
                }
            }
            else
            {
                //Debug.LogWarning($"Файл сохранения глобального состояния не найден по пути: {filePath}. Используется текущее состояние.", null);
            }
        }

        /// <summary>
        /// Загружает данные в <see cref="ItemContainer"/> из JSON файла для указанного слота.
        /// </summary>
        /// <param name="targetContainer">Контейнер предметов, в который будут загружены данные.</param>
        /// <param name="slotFolderPath">Путь к папке слота сохранения/загрузки.</param>
        public void LoadItemContainer(ItemContainer targetContainer, string slotFolderPath)
        {
            if (targetContainer == null)
            {
                //Debug.LogWarning("Попытка загрузить в пустой (null) ItemContainer.");
                return;
            }

            if (targetContainer.ContainerDefinition == null || string.IsNullOrEmpty(targetContainer.ContainerDefinition.ContainerGuid))
            {
                //Debug.LogError($"GUID целевого контейнера '{targetContainer.name}' не может быть пустым для загрузки!");
                return;
            }

            string containerGuid = targetContainer.ContainerDefinition.ContainerGuid;
            string fileName = $"{containerGuid}.json";
            string filePath = Path.Combine(slotFolderPath, fileName);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                // Десериализуем JSON в новый временный ItemContainerDefinition
                ItemContainerDefinition loadedContainerDefinition = Inventory.JsonScriptableObjectSerializer.DeserializeScriptableObject<ItemContainerDefinition>(json);

                if (loadedContainerDefinition != null)
                {
                    //Debug.Log($"Контейнер '{containerGuid}' загружен из: {filePath}. Десериализовано {loadedContainerDefinition.Items.Count} предметов.");

                    int itemIndex = 0;
                    foreach (var item in loadedContainerDefinition.Items)
                    {
                        if (item != null)
                        {
                            //Debug.Log($"[LoadService]   Загруженный предмет {itemIndex}: Name='{item.DefinitionName}', WrapperIndex={item.ID}, Stack={item.Stack}, GridPosition={item.GridPosition}, RuntimeID={item.GetInstanceID()}, Type={item.GetType().Name}");
                        }
                        else
                        {
                            //Debug.Log($"[LoadService]   Загруженный предмет {itemIndex}: NULL (Возможно, потеряна ссылка)");
                        }
                        itemIndex++;
                    }

                    // Копируем данные из загруженного определения в существующее
                    // Это важно для ScriptableObject, чтобы сохранить ссылки в Unity
                    targetContainer.ContainerDefinition.SetDataFromOtherContainer(loadedContainerDefinition);

                    // После загрузки данных в ItemDataStorageSO, настраиваем логическую сетку контейнера
                    targetContainer.SetupLoadedItemsGrid();


                }
                else
                {
                    //Debug.LogError($"Не удалось десериализовать ItemContainerDefinition из файла: {filePath}");
                }
            }
            else
            {
                //Debug.LogWarning($"Файл сохранения для контейнера '{containerGuid}' не найден по пути: {filePath}");
            }
        }

        /// <summary>
        /// Получает или создает путь к папке для указанного слота сохранения.
        /// </summary>
        /// <param name="slotIndex">Индекс слота сохранения.</param>
        /// <returns>Полный путь к папке слота сохранения.</returns>
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
