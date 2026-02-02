using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik.Inventory
{
    public class GlobalItemStorage : MonoBehaviour
    {
        [SerializeField]
        private ItemsDataStorageDefinition globalItemsStorageDefinition;
        [SerializeField]
        private ItemPrefabsStorageDefinition itemPrefabsStorageDefinition;

        public ItemsDataStorageDefinition GlobalItemsStorageDefinition => globalItemsStorageDefinition;
        public ItemPrefabsStorageDefinition ItemPrefabsStorageDefinition => itemPrefabsStorageDefinition;

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