using UnityEngine;
using UnityEngine.DataEditor;
//using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class KeyItemsStorageWrapper : MonoBehaviour
    {
        [SerializeField]
        private KeyItemDefinition[] _wrapperItems = new KeyItemDefinition[100];

        public KeyItemDefinition GetWrapperItem(int wrapperIndex)
        {
            _wrapperItems[wrapperIndex].WrapperIndex += wrapperIndex;
            return _wrapperItems[wrapperIndex];
        }

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