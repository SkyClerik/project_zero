using UnityEngine;

namespace UnityEngine.Toolbox
{
    public static partial class ComponentExt
    {
        /// <summary>
        /// Получает первый найденный на GameObject компонент, реализующий указанный интерфейс.
        /// </summary>
        /// <typeparam name="I">Тип интерфейса для поиска.</typeparam>
        /// <param name="comp">Компонент, на GameObject которого будет производиться поиск.</param>
        /// <returns>Найденный компонент, приведенный к типу интерфейса I, или null, если компонент не найден.</returns>
        public static I GetInterfaceComponent<I>(this Component comp) where I : class
        {
            return comp.GetComponent(typeof(I)) as I;
        }

        /// <summary>
        /// Получает все компоненты на GameObject, реализующие указанный интерфейс.
        /// </summary>
        /// <typeparam name="I">Тип интерфейса для поиска.</typeparam>
        /// <param name="comp">Компоннент, на GameObject которого будет производиться поиск.</param>
        /// <returns>Массив всех найденных компонентов, реализующих интерфейс I. Возвращает пустой массив, если ничего не найдено.</returns>
        public static I[] GetInterfaceComponents<I>(this Component comp) where I : class
        {
            var components = comp.GetComponents(typeof(I));
            I[] iComponents = new I[components.Length];
            components.CopyTo(iComponents, 0);
            return iComponents;
        }

        /// <summary>
        /// Безопасно получает компонент типа T. В случае отсутствия компонента, выводит ошибку в консоль, чтобы предотвратить NullReferenceException в дальнейшем.
        /// </summary>
        /// <typeparam name="T">Тип компонента для поиска.</typeparam>
        /// <param name="comp">Компонент, на GameObject которого будет производиться поиск.</param>
        /// <returns>Найденный компонент типа T. Возвращает null и выводит ошибку, если компонент не найден.</returns>
        public static T GetSafeComponent<T>(this Component comp) where T : Component
        {
            T component = comp.GetComponent<T>();
            if (component == null)
                Debug.LogError("Component of type " + typeof(T) + " not found", comp);
            return component;
        }

        /// <summary>
        /// Получает компонент с игрового объекта, поддерживая поиск по типу интерфейса.
        /// </summary>
        /// <typeparam name="T">Тип компонента или интерфейса для поиска.</typeparam>
        /// <param name="inst">Игровой объект, на котором будет производиться поиск.</param>
        /// <returns>Найденный компонент, приведенный к типу T, или null, если компонент не найден.</returns>
        public static T GetComponent<T>(this GameObject inst) where T : class
        {
            return inst.GetComponent(typeof(T)) as T;
        }

        /// <summary>
        /// Пытается рекурсивно найти компонент указанного типа в текущем Transform или его дочерних объектах.
        /// </summary>
        /// <typeparam name="T">Тип компонента для поиска.</typeparam>
        /// <param name="transform">Transform, с которого начинается поиск.</param>
        /// <param name="component">Выходной параметр, в который будет записан найденный компонент.</param>
        /// <returns>Возвращает `true`, если компонент найден, и `false` в противном случае.</returns>
        public static bool TryGetComponentInChildren<T>(this Transform transform, out T component) where T : Component
        {
            component = transform.GetComponent<T>();
            if (component != null)
                return true;

            foreach (Transform child in transform)
            {
                if (child.TryGetComponentInChildren<T>(out component))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Получает первый найденный компонент типа T на GameObject, даже если он отключен (disabled).
        /// </summary>
        /// <typeparam name="T">Тип компонента для поиска.</typeparam>
        /// <param name="inst">Игровой объект, на котором будет производиться поиск.</param>
        /// <returns>Первый найденный компонент типа T (включая неактивные) или null, если ничего не найдено.</returns>
        public static T GetComponentIncludingDisabled<T>(this GameObject inst) where T : Component
        {
            T[] components = inst.GetComponents<T>();
            return components.Length > 0 ? components[0] : null;
        }
    }
}