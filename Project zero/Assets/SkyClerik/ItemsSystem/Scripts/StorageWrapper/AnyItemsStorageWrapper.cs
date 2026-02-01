using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.DataEditor;
//using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class AnyItemsStorageWrapper : MonoBehaviour
    {
        [SerializeField]
        private List<WrapperBase> wrappers = new List<WrapperBase>();

        //public KeyItemDefinition GetWrapperItem(int listIndex, int wrapperIndex)
        //{
        //_wrapperItems[wrapperIndex].WrapperIndex += wrapperIndex;
        //return _wrapperItems[wrapperIndex];
        //}

        //private void Awake()
        //{
        //    ServiceProvider.Register(this);
        //}

        //private void OnDestroy()
        //{
        //    ServiceProvider.Unregister(this);
        //}
    }
}