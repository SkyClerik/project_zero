namespace UnityEngine.Toolbox
{
    public static partial class Vector3Ext
    {
        /// <summary>
        /// Возвращает новый `Vector3` с измененной координатой X.
        /// </summary>
        /// <param name="inst">Исходный `Vector3`.</param>
        /// <param name="x">Новое значение для координаты X.</param>
        /// <returns>Новый `Vector3` с обновленной координатой X.</returns>
        public static Vector3 WithX(this Vector3 inst, float x)
        {
            inst.x = x;
            return inst;
        }

        /// <summary>
        /// Возвращает новый `Vector3` с измененной координатой Y.
        /// </summary>
        /// <param name="inst">Исходный `Vector3`.</param>
        /// <param name="y">Новое значение для координаты Y.</param>
        /// <returns>Новый `Vector3` с обновленной координатой Y.</returns>
        public static Vector3 WithY(this Vector3 inst, float y)
        {
            inst.y = y;
            return inst;
        }

        /// <summary>
        /// Возвращает новый `Vector3` с измененной координатой Z.
        /// </summary>
        /// <param name="inst">Исходный `Vector3`.</param>
        /// <param name="z">Новое значение для координаты Z.</param>
        /// <returns>Новый `Vector3` с обновленной координатой Z.</returns>
        public static Vector3 WithZ(this Vector3 inst, float z)
        {
            inst.z = z;
            return inst;
        }
    }
}
