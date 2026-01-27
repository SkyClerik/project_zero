using System.Collections.Generic;

namespace UnityEngine.Toolbox
{
    public static partial class ListExt
    {
        /// <summary>
        /// Меняет местами два элемента в списке по их индексам.
        /// </summary>
        /// <typeparam name="T">Тип элементов в списке.</typeparam>
        /// <param name="list">Список, в котором происходит обмен элементами.</param>
        /// <param name="indexA">Индекс первого элемента.</param>
        /// <param name="indexB">Индекс второго элемента.</param>
        public static void Swap<T>(this List<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}
