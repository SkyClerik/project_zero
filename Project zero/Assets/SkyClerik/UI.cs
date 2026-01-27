using Gameplay.Inventory;
using UnityEngine;

namespace SkyClerik
{
    public class UI : MonoBehaviour
    {
        [SerializeField]
        private ItemsPage _itemsPage;

        public ItemsPage ItemsPage => _itemsPage;
    }
}