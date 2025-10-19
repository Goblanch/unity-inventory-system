using System.Collections.Generic;
using UnityEngine;

namespace GB.Inventory.Infrastructure.Definitions
{
    [CreateAssetMenu(fileName = "SlotProfileDatabase", menuName = "GB/Inventory/DB - Slot Profiles")]
    public sealed class SlotProfileDatabase : ScriptableObject
    {
        [SerializeField] private SlotProfileDefinition[] profiles;

        private readonly Dictionary<string, SlotProfileDefinition> _map = new();

        void OnEnable()
        {
            _map.Clear();
            if (profiles == null) return;
            foreach (var p in profiles)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.ProfileId)) continue;
                _map[p.ProfileId] = p;
            }
        }

        public bool TryGet(string profileId, out SlotProfileDefinition def) => _map.TryGetValue(profileId, out def);
    }
}