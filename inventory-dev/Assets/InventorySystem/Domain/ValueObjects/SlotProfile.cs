using System;

namespace GB.Inventory.Domain
{
    /// <summary>
    /// Slot behaviour profile that governs acceptance rules and stack limits for items placed in a slot.
    /// Profiles are referenced by ID and can be assigned per-slot.
    /// </summary>
    [Serializable]
    public sealed class SlotProfile
    {
        /// <summary>Profile identifier (e.g., "Any", "Materials", "Consumables").</summary>
        public string Id;

        /// <summary>Allowed item types; empty means all types are allowed.</summary>
        public string[] AllowedTypes;

        /// <summary>All required tags must be present on the item; empty means no required tags.</summary>
        public string[] RequiredTags;
        
        /// <summary>Any tag listed here forbids the item from being accepted.</summary>
        public string[] BannedTags;

        /// <summary>When true, the profile overrides stackability/max stack regardless of item settings.</summary>
        public bool HasStackableOverride;

        /// <summary>If <see cref="HasStackableOverride"/> is true and this is false, the slot becomes non-stackable (max=1).</summary>
        public bool StackableOverride;
        
        /// <summary>If <see cref="HasStackableOverride"/> is true and positive, clamps the max-per-stack to this value.</summary>
        public int MaxStackOverride;
    }
}