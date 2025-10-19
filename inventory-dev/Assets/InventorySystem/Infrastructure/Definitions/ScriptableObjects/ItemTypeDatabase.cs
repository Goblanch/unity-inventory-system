using System.Collections.Generic;
using UnityEngine;

namespace GB.Inventory.Infrastructure.Definitions
{
    [CreateAssetMenu(fileName = "ItemTypeDataBase", menuName = "GB/Inventory/DB - Item Types")]
    public sealed class ItemTypeDataBase : ScriptableObject
    {
        [SerializeField] private ItemTypeDefinition[] types;

        private readonly Dictionary<string, ItemTypeDefinition> _map = new();

        void OnEnable()
        {
            _map.Clear();
            if (types == null) return;
            foreach (var t in types)
            {
                if (t == null || string.IsNullOrWhiteSpace(t.TypeId)) continue;
                _map[t.TypeId] = t;
            }
        }

        public bool TryGet(string typeId, out ItemTypeDefinition def) => _map.TryGetValue(typeId, out def);
    }
}