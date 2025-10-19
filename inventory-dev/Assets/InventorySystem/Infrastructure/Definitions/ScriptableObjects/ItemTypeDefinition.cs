using UnityEngine;

namespace GB.Inventory.Infrastructure.Definitions
{
    [CreateAssetMenu(fileName = "ItemTypeDefinition", menuName = "GB/Inventory/Item Type Definition")]
    public sealed  class ItemTypeDefinition : ScriptableObject
    {
        [SerializeField] private string typeId;
        [SerializeField] private Sprite defaultIcon;

        public string TypeId => typeId;
        public Sprite DefaultIcon => defaultIcon;
    }
}