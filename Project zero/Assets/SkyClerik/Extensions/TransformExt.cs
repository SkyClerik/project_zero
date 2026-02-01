using System;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.Toolbox
{
    public static partial class TransformExt
    {
        /// <summary>
        /// Устанавливает позицию X Transform'а в мировых координатах.
        /// </summary>
        /// <param name="transform">Целевой Transform.</param>
        /// <param name="x">Новое значение позиции X.</param>
        public static void SetX(this Transform transform, float x)
        {
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        /// <summary>
        /// Устанавливает позицию Y Transform'а в мировых координатах.
        /// </summary>
        /// <param name="transform">Целевой Transform.</param>
        /// <param name="y">Новое значение позиции Y.</param>
        public static void SetY(this Transform transform, float y)
        {
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        /// <summary>
        /// Устанавливает позицию Z Transform'а в мировых координатах.
        /// </summary>
        /// <param name="transform">Целевой Transform.</param>
        /// <param name="z">Новое значение позиции Z.</param>
        public static void SetZ(this Transform transform, float z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        /// <summary>
        /// Устанавливает позицию X Transform'а в локальных координатах.
        /// </summary>
        /// <param name="transform">Целевой Transform.</param>
        /// <param name="x">Новое значение локальной позиции X.</param>
        public static void SetLocalX(this Transform transform, float x)
        {
            transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
        }

        /// <summary>
        /// Устанавливает позицию Y Transform'а в локальных координатах.
        /// </summary>
        /// <param name="transform">Целевой Transform.</param>
        /// <param name="y">Новое значение локальной позиции Y.</param>
        public static void SetLocalY(this Transform transform, float y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        }

        /// <summary>
        /// Устанавливает позицию Z Transform'а в локальных координатах.
        /// </summary>
        /// <param name="transform">Целевой Transform.</param>
        /// <param name="z">Новое значение локальной позиции Z.</param>
        public static void SetLocalZ(this Transform transform, float z)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
        }

        /// <summary>
        /// Сбрасывает локальную позицию, локальное вращение и локальный масштаб Transform'а к значениям по умолчанию
        /// (позиция: Vector3.zero, вращение: Quaternion.identity, масштаб: Vector3.one).
        /// </summary>
        /// <param name="tr">Целевой Transform.</param>
        public static void LocalReset(this Transform tr)
        {
            tr.localPosition = Vector3.zero;
            tr.localRotation = Quaternion.identity;
            tr.localScale = Vector3.one;
        }

        /// <summary>
        /// Копирует позицию и вращение из одного Transform'а в другой.
        /// </summary>
        /// <param name="tr">Целевой Transform, в который будут скопированы значения.</param>
        /// <param name="from">Исходный Transform, откуда берутся значения.</param>
        /// <param name="doCloneScale">Если `true`, копируется также локальный масштаб.</param>
        /// <param name="doCloneParent">Если `true`, устанавливается тот же родительский Transform.</param>
        public static void CloneTransform(this Transform tr, Transform from, bool doCloneScale = false, bool doCloneParent = false)
        {
            tr.position = from.position;
            tr.rotation = from.rotation;
            if (doCloneScale)
                tr.localScale = from.localScale;
            if (doCloneParent)
                tr.parent = from.parent;
        }

        /// <summary>
        /// Рекурсивно ищет дочерний объект с указанным именем в иерархии Transform'а.
        /// </summary>
        /// <param name="obj">Transform, в иерархии которого производится поиск.</param>
        /// <param name="name">Имя дочернего объекта для поиска.</param>
        /// <param name="excludeCurrentTransform">Если `true`, поиск начинается с дочерних объектов `obj`, не включая сам `obj`.</param>
        /// <returns>Найденный Transform или `null`, если объект с таким именем не найден.</returns>
        public static Transform FindChildRecursive(this Transform obj, string name, bool excludeCurrentTransform = false)
        {
            if (excludeCurrentTransform)
                return obj.GetComponentsInChildren<Transform>().FirstOrDefault(tr => tr.name == name && obj != tr);
            else
                return obj.GetComponentsInChildren<Transform>().FirstOrDefault(tr => tr.name == name);
        }

        /// <summary>
        /// Рекурсивно ищет все дочерние объекты, соответствующие любому из переданных имен, в иерархии Transform'а.
        /// </summary>
        /// <param name="obj">Transform, в иерархии которого производится поиск.</param>
        /// <param name="excludeCurrentTransform">Если `true`, поиск начинается с дочерних объектов `obj`, не включая сам `obj`.</param>
        /// <param name="names">Массив имен дочерних объектов для поиска.</param>
        /// <returns>Массив найденных Transform'ов.</returns>
        public static Transform[] FindChildsRecursive(this Transform obj, bool excludeCurrentTransform = false, params string[] names)
        {
            if (excludeCurrentTransform)
                return obj.GetComponentsInChildren<Transform>().Where(tr => names.Contains(tr.name) && obj != tr).ToArray();
            else
                return obj.GetComponentsInChildren<Transform>().Where(tr => names.Contains(tr.name)).ToArray();
        }

        /// <summary>
        /// Удаляет все компоненты (кроме Transform) со всех дочерних объектов рекурсивно.
        /// </summary>
        /// <remarks>
        /// **Внимание:** Этот метод использует `GameObject.DestroyImmediate`, что рекомендуется использовать только в режиме редактора.
        /// Использование в рантайме может привести к проблемам и утечкам памяти.
        /// </remarks>
        /// <param name="obj">Корневой Transform, начиная с которого происходит удаление.</param>
        /// <param name="excludeTypes">Массив типов компонентов, которые не следует удалять.</param>
        public static void RemoveChildComponentsRecursive(this Transform obj, params Type[] excludeTypes)
        {
            var allComponents = obj.GetComponentsInChildren<Component>().Where(c => c.GetType() != typeof(Transform));
            IEnumerable<Component> components = null;
            if (excludeTypes != null)
                components = allComponents.Where(c => !excludeTypes.Contains(c.GetType())); // Исправлено: Use !excludeTypes.Contains
            else
                components = allComponents;

            int count = components.Count();
            for (int i = 0; i < count; i++)
                GameObject.DestroyImmediate(components.ElementAt(i));
        }
    }
}
