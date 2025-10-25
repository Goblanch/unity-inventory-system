using System.Collections.Generic;
using UnityEngine;

namespace GB.Inventory.Samples.UI
{
    [CreateAssetMenu(fileName = "ItemIconProvider", menuName = "GB/Inventory Sample/Item Icon Database")]
    public class ItemIconDatabase : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string definitionId;
            public Sprite icon;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();
        private Dictionary<string, Sprite> _index;

        private void OnEnable()
        {
            RebuildIndex();
        }

        public void RebuildIndex()
        {
            _index = new Dictionary<string, Sprite>();
            if (entries == null) return;
            foreach (var e in entries)
            {
                if (string.IsNullOrWhiteSpace(e.definitionId)) continue;
                _index[e.definitionId] = e.icon;
            }
        }
        
        public Sprite GetIcon(string definitionId)
        {
            if (_index == null) RebuildIndex();
            return (definitionId != null && _index.TryGetValue(definitionId, out var s)) ? s : null;
        }
}
}


