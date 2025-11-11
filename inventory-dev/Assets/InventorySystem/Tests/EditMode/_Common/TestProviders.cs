using System.Collections.Generic;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;

namespace GB.Inventory.Tests.Common
{
    /// <summary>
    /// Implementaciones simples en memoria para tests de dominio
    /// </summary>
    public class DictItemMetaProvider : IItemMetadataProvider
    {
        private readonly Dictionary<string, ItemMeta> _db;
        public DictItemMetaProvider(Dictionary<string, ItemMeta> db) { _db = db; }

        public ItemMeta Get(string definitionId)
        {
            ItemMeta m;
            return _db.TryGetValue(definitionId, out m) ? m : null;
        }

        public bool TryGet(string definitionId, out ItemMeta meta)
            => _db.TryGetValue(definitionId, out meta);
    }

    public class DictSlotProfileProvider : ISlotProfileProvider
    {
        private readonly Dictionary<string, SlotProfile> _db;
        public DictSlotProfileProvider(Dictionary<string, SlotProfile> db) { _db = db; }

        public SlotProfile Get(string slotProfileId)
        {
            SlotProfile p;
            return _db.TryGetValue(slotProfileId, out p) ? p : null;
        }

        public bool TryGet(string slotProfileId, out SlotProfile profile)
            => _db.TryGetValue(slotProfileId, out profile);
    }

    /// <summary>
    /// Política de stacking fija, usada para simplificar ciertos tests.
    /// </summary>
    public class FixedStackingPolicy : IStackingPolicy
    {
        private readonly int _max;
        public FixedStackingPolicy(int max) { _max = max; }

        public int GetMaxPerStack(string definitionId, string typeId) => _max;
    }
}