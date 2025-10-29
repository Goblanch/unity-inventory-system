using System;
using GB.Inventory.Domain.Abstractions;

namespace GB.Inventory.Domain.Policies
{
    /// <summary>
    /// Stacking policy that reads per-item metadata to determine the base max-per-stack.
    /// If the item defines an override (e.g., sword=1, stone=50), it is used; otherwise a default max is returned.
    /// </summary>
    public sealed class PerItemStackingPolicy : IStackingPolicy
    {
        private readonly IItemMetadataProvider _meta;
        private readonly int _defaultMax;

        /// <summary>
        /// Creates the policy.
        /// </summary>
        /// <param name="metadataProvider"></param>
        /// <param name="defaultMax"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public PerItemStackingPolicy(IItemMetadataProvider metadataProvider, int defaultMax)
        {
            _meta = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            if (defaultMax < 1) throw new ArgumentOutOfRangeException(nameof(defaultMax));
            _defaultMax = defaultMax;
        }

        ///<inheritdoc/>
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