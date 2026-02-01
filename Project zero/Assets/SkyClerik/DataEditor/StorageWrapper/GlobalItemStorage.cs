using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class GlobalItemStorage : MonoBehaviour
    {
        [SerializeField]
        private GlobalItemsStorageDefinition globalItemsStorageDefinition;

        public GlobalItemsStorageDefinition GlobalItemsStorageDefinition => globalItemsStorageDefinition;

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