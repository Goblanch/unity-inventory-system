using System;
using GB.Inventory.Domain.Abstractions;

namespace GB.Inventory.Domain.Policies
{
    public sealed class PerItemStackingPolicy : IStackingPolicy
    {
        private readonly IItemMetadataProvider _meta;
        private readonly int _defaultMax;

        public PerItemStackingPolicy(IItemMetadataProvider metadataProvider, int defaultMax)
        {
            _meta = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            if (defaultMax < 1) throw new ArgumentOutOfRangeException(nameof(defaultMax));
            _defaultMax = defaultMax;
        }

        public int GetMaxPerStack(string definitionId, string typeId)
        {
            if (string.IsNullOrWhiteSpace(definitionId)) return _defaultMax;

            ItemMeta meta;
            if (_meta.TryGet(definitionId, out meta) && meta.HasStackOverride && meta.MaxStack > 0)
                return meta.MaxStack;

            return _defaultMax;
        }
    }
}