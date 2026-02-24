//using UnityEngine;
//using System;

//namespace SkyClerik
//{
//    /// <summary>
//    /// Компонент для объектов на сцене, состояние которых должно быть сохранено и загружено.
//    /// </summary>
//    public class SavableEntity : MonoBehaviour
//    {
//        // Уникальный идентификатор объекта на сцене. Должен быть уникальным и персистентным.
//        [SerializeField]
//        private string _uniqueID = Guid.NewGuid().ToString();
//        public string UniqueID => _uniqueID;

//        // Метод для генерации нового GUID в редакторе
//        [ContextMenu("Generate New Unique ID")]
//        private void GenerateNewUniqueID()
//        {
//            _uniqueID = Guid.NewGuid().ToString();
//            Debug.Log($"Сгенерирован новый UniqueID для {gameObject.name}: {_uniqueID}");
//        }

//        /// <summary>
//        /// Возвращает JSON-строку, представляющую сохраняемое состояние объекта.
//        /// </summary>
//        public virtual string SaveState()
//        {
//            // Для примера, сохраняем только активность объекта
//            return JsonUtility.ToJson(new SavableEntityState { isActive = gameObject.activeSelf });
//        }

//        /// <summary>
//        /// Применяет загруженное состояние к объекту.
//        /// </summary>
//        /// <param name="stateData">JSON-строка с данными состояния.</param>
//        public virtual void LoadState(string stateData)
//        {
//            if (string.IsNullOrEmpty(stateData))
//            {
//                Debug.LogWarning($"Попытка загрузить пустое состояние для SavableEntity: {UniqueID}");
//                return;
//            }

//            try
//            {
//                SavableEntityState loadedState = JsonUtility.FromJson<SavableEntityState>(stateData);
//                gameObject.SetActive(loadedState.isActive);
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"Ошибка при загрузке состояния для SavableEntity: {UniqueID}. {ex.Message}");
//            }
//        }

//        // Внутренний класс для сериализации/десериализации состояния SavableEntity
//        [Serializable]
//        private class SavableEntityState
//        {
//            public bool isActive;
//        }
//    }
//}
