using UnityEngine;
using UnityEngine.DataEditor;
using UnityEngine.Toolbox;

namespace SkyClerik.Utils
{
    public class ItemDataStorageWrapper : MonoBehaviour
    {
        [SerializeField]
        private ItemBaseDefinition[] _wrapperItems = new ItemBaseDefinition[1000];

        public ItemBaseDefinition GetWrapperItem(int wrapperIndex)
        {
            _wrapperItems[wrapperIndex].WrapperIndex = wrapperIndex;
            return _wrapperItems[wrapperIndex];
        }

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);
        }
    }
}