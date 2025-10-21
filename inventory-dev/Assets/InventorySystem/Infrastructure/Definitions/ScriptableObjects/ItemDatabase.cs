using System.Collections.Generic;
using UnityEngine;

namespace GB.Inventory.Infrastructure.Definitions
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "GB/Inventory/DB - Items")]
    public sealed class ItemDatabase : ScriptableObject
    {
        [SerializeField] private ItemDefinition[] items;

        private readonly Dictionary<string, ItemDefinition> _map = new();

        void OnEnable()
        {
            RebuildIndex();
        }

        void OnValidate()
        {
            RebuildIndex();
        }

        public void RebuildIndex()
        {
            _map.Clear();
            if (items == null) return;
            foreach (var it in items)
            {
                if (it == null || string.IsNullOrWhiteSpace(it.DefinitionId)) continue;
                _map[it.DefinitionId] = it;
            }
        }

        public bool TryGet(string definitionId, out ItemDefinition def) => _map.TryGetValue(definitionId, out def);
    }
}