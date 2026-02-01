using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.Toolbox
{
    public static partial class GameObjectExt
    {
        /// <summary>
        /// Рекурсивно устанавливает указанный слой для игрового объекта и всех его дочерних объектов.
        /// </summary>
        /// <param name="inst">Корневой игровой объект, для которого нужно установить слой.</param>
        /// <param name="layer">Номер слоя для установки.</param>
        public static void SetLayerRecursively(this GameObject inst, int layer)
        {
            inst.layer = layer;
            foreach (Transform child in inst.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Находит первый родительский игровой объект (включая текущий), на котором есть компонент указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип компонента или интерфейса для поиска.</typeparam>
        /// <param name="inst">Игровой объект, с которого начинается поиск вверх по иерархии.</param>
        /// <returns>Найденный `GameObject` или `null`, если ничего не найдено.</returns>
        public static GameObject FindTypeAboveObject<T>(this GameObject inst) where T : class
        {
            if (inst == null)
            {
                return null;
            }

            return FindTypeAboveObjectRecursive<T>(inst);
        }

        /// <summary>
        /// Рекурсивно поднимается по иерархии от `inst`, чтобы найти `GameObject` с компонентом типа `T`.
        /// </summary>
        /// <remarks>
        /// Это рекурсивный вспомогательный метод для `FindTypeAboveObject`. Рекомендуется использовать `FindTypeAboveObject`.
        /// </remarks>
        /// <typeparam name="T">Тип компонента или интерфейса для поиска.</typeparam>
        /// <param name="inst">Текущий проверяемый игровой объект.</param>
        /// <returns>Найденный `GameObject` или `null`, если достигнут корень иерархии без результата.</returns>
        public static GameObject FindTypeAboveObjectRecursive<T>(this GameObject inst) where T : class
        {
            if (inst == null)
            {
                return null;
            }

            if (inst != null)
            {
                if (inst.GetComponent<T>() != null)
                {
                    return inst;
                }

                if (inst.transform.parent != null)
                {
                    return FindTypeAboveObjectRecursive<T>(inst.transform.parent.gameObject);
                }
            }

            return null;
        }

        /// <summary>
        /// Рекурсивно находит дочерний объект с указанным именем.
        /// </summary>
        /// <param name="inst">Игровой объект, с которого начинается поиск.</param>
        /// <param name="name">Имя дочернего объекта для поиска.</param>
        /// <returns>Transform найденного дочернего объекта или `null`, если не найден.</returns>
        public static Transform FindChildRecursiveByName(this GameObject inst, string name)
        {
            if (inst == null)
                return null;

            if (inst.transform.childCount == 0)
                return null;

            var child = inst.transform.Find(name);
            if (child != null)
            {
                return child;
            }

            foreach (Transform t in inst.transform)
            {
                child = FindChildRecursiveByName(t.gameObject, name);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// Показывает игровой объект, устанавливая его активным. Является null-safe.
        /// </summary>
        /// <param name="inst">Игровой объект для активации.</param>
        public static void Show(this GameObject inst)
        {
            if (inst != null)
            {
                inst.SetActive(true);
            }
        }

        /// <summary>
        /// Скрывает игровой объект, устанавливая его неактивным. Является null-safe.
        /// </summary>
        /// <param name="inst">Игровой объект для деактивации.</param>
        public static void Hide(this GameObject inst)
        {
            if (inst != null)
            {
                inst.SetActive(false);
            }
        }

        /// <summary>
        /// Проверяет, что объект не равен null.
        /// </summary>
        /// <param name="inst">Проверяемый объект.</param>
        /// <returns>Возвращает `true`, если объект не `null`, иначе `false`.</returns>
        public static bool IsReady(this Object inst)
        {
            return inst != null;
        }

        /// <summary>
        /// Отвязывает и уничтожает всех непосредственных дочерних объектов у игрового объекта.
        /// </summary>
        /// <param name="inst">Родительский игровой объект, дочерние элементы которого нужно уничтожить.</param>
        public static void DestroyChildren(this GameObject inst)
        {
            if (inst == null)
                return;

            List<Transform> transforms = new List<Transform>();
            foreach (Transform t in inst.transform)
            {
                transforms.Add(t);
            }

            foreach (Transform t in transforms)
            {
                t.parent = null;
                Object.Destroy(t.gameObject);
            }

            transforms.Clear();
        }

        /// <summary>
        /// Рекурсивно изменяет слой всех дочерних объектов на указанный слой по имени.
        /// </summary>
        /// <param name="inst">Корневой игровой объект.</param>
        /// <param name="name">Имя слоя для установки.</param>
        public static void ChangeLayersRecursively(this GameObject inst, string name)
        {
            if (inst == null)
                return;

            foreach (Transform child in inst.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer(name);
                ChangeLayersRecursively(child.gameObject, name);
            }
        }

        /// <summary>
        /// Проверяет, видим ли хотя бы один рендерер на игровом объекте или в его дочерних объектах.
        /// </summary>
        /// <param name="inst">Игровой объект для проверки.</param>
        /// <returns>Возвращает `true`, если найден хотя бы один активный рендерер, иначе `false`.</returns>
        public static bool IsRenderersVisible(GameObject inst)
        {
            if (inst == null)
                return false;

            if (inst.GetComponent<Renderer>() != null)
            {
                if (inst.GetComponent<Renderer>().enabled)
                {
                    return true;
                }
            }

            Renderer[] rendererComponents = inst.GetComponentsInChildren<Renderer>();

            // Enable rendering:
            foreach (Renderer component in rendererComponents)
            {
                if (component.enabled)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Включает все рендереры на игровом объекте и во всех его дочерних объектах.
        /// </summary>
        /// <param name="inst">Игровой объект, рендереры которого нужно включить.</param>
        public static void ShowRenderers(GameObject inst)
        {
            if (inst == null)
                return;

            if (inst.GetComponent<Renderer>() != null)
            {
                inst.GetComponent<Renderer>().enabled = true;
            }

            Renderer[] rendererComponents = inst.GetComponentsInChildren<Renderer>();

            // Enable rendering:
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = true;
            }
        }

        /// <summary>
        /// Отключает все рендереры на игровом объекте и во всех его дочерних объектах.
        /// </summary>
        /// <param name="inst">Игровой объект, рендереры которого нужно отключить.</param>
        public static void HideRenderers(GameObject inst)
        {
            if (inst == null)
                return;

            if (inst.GetComponent<Renderer>() != null)
            {
                inst.GetComponent<Renderer>().enabled = false;
            }

            Renderer[] rendererComponents = inst.GetComponentsInChildren<Renderer>();

            // Enable rendering:
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = false;
            }
        }

        /// <summary>
        /// [DEBUG] Выводит в лог информацию о всех корневых объектах сцены.
        /// </summary>
        /// <remarks>
        /// Этот метод предназначен для отладки и может быть удален в релизных сборках.
        /// </remarks>
        public static void DumpRootTransforms()
        {
            Object[] objs = Object.FindObjectsByType(typeof(GameObject), FindObjectsSortMode.None);
            foreach (Object obj in objs)
            {
                GameObject go = obj as GameObject;
                if (go.transform.parent == null)
                {
                    DumpGoToLog(go);
                }
            }
        }

        /// <summary>
        /// [DEBUG] Выводит в лог дамп иерархии и свойств для указанного игрового объекта.
        /// </summary>
        /// <param name="go">Игровой объект для вывода в лог.</param>
        /// <remarks>
        /// Этот метод предназначен для отладки и может быть удален в релизных сборках.
        /// </remarks>
        public static void DumpGoToLog(GameObject go)
        {
            Debug.Log($"DUMP: go: {go.name} :::: {DumpGo(go)}");
        }

        /// <summary>
        /// [DEBUG] Формирует строку с подробной информацией об игровом объекте и его дочерних объектах.
        /// </summary>
        /// <param name="go">Игровой объект для анализа.</param>
        /// <returns>Строка с отформатированной информацией об иерархии и свойствах.</returns>
        /// <remarks>
        /// Этот метод предназначен для отладки и может быть удален в релизных сборках.
        /// </remarks>
        public static string DumpGo(GameObject go)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(go.name);
            DumpGameObject(go, sb, "", false);
            return sb.ToString();

            void DumpGameObject(GameObject gameObject, StringBuilder sb, string indent, bool includeAllComponents)
            {
                bool rendererEnabled = false;
                if (gameObject.GetComponent<Renderer>() != null)
                {
                    rendererEnabled = gameObject.GetComponent<Renderer>().enabled;
                }
                int markerId = -1;
                bool hasLoadedObj = false;

                sb.Append($"\r\n{indent} - name: {gameObject.name}\n" +
                    $" - a: {gameObject.activeSelf} - r: {rendererEnabled} - mid: {markerId} - loadedObj: {hasLoadedObj}\n" +
                    $" - scale: x: {gameObject.transform.localScale.x} y: {gameObject.transform.localScale.y} z: {gameObject.transform.localScale.z}\n" +
                    $" - pos: x: {gameObject.transform.position.x} y: {gameObject.transform.position.y} z: {gameObject.transform.position.z}\n");

                if (includeAllComponents)
                {
                    foreach (Component component in gameObject.GetComponents<Component>())
                    {
                        DumpComponent(component, sb, indent + "  ");
                    }
                }

                void DumpComponent(Component component, StringBuilder sb, string indent)
                {
                    sb.Append(string.Format("{0}{1}", indent, (component == null ? "(null)" : component.GetType().Name)));
                }

                foreach (Transform child in gameObject.transform)
                {
                    DumpGameObject(child.gameObject, sb, indent + "  ", includeAllComponents);
                }
            }
        }

        /// <summary>
        /// Возвращает список непосредственных дочерних GameObject'ов у данного объекта (не рекурсивно).
        /// </summary>
        /// <param name="parent">Родительский GameObject.</param>
        /// <returns>Список дочерних GameObject'ов.</returns>
        public static List<GameObject> GetDirectChildren(this GameObject parent)
        {
            List<GameObject> children = new List<GameObject>();

            // Проходим по всем непосредственным детям через Transform
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }

            return children;
        }

        /// <summary>
        /// Находит все компоненты указанного типа T в дочерних объектах, исключая сам родительский объект.
        /// </summary>
        /// <typeparam name="T">Тип компонента для поиска.</typeparam>
        /// <param name="gameObject">Родительский объект, с которого начинается поиск.</param>
        /// <param name="includeInactive">Включать ли неактивные дочерние объекты в поиск? По умолчанию false.</param>
        /// <returns>Список найденных компонентов.</returns>
        public static List<T> FindComponentsInAllChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
        {
            // GetComponentsInChildren(true) находит компоненты и на самом объекте, и на всех дочерних.
            var allComponents = gameObject.GetComponentsInChildren<T>(includeInactive);

            // Используем Linq, чтобы отфильтровать и вернуть только те компоненты,
            // которые не принадлежат самому родительскому объекту.
            return allComponents.Where(c => c.gameObject != gameObject).ToList();
        }


    }
}
