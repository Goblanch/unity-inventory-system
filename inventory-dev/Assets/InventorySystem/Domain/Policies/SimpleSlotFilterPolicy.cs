using System;
using System.Linq;
using GB.Inventory.Domain.Abstractions;
using UnityEditor.Build;

namespace GB.Inventory.Domain.Policies
{
    /// <summary>
    /// Slot filter that enforces slot-profile constraints:
    /// - AllowedTypes, RequiredTags, BannedTags.
    /// - Optional stackability override (non-stackable or max stack clamp per profile)
    /// It combines the per item base max (IStackingPolicy) with profile overrides.
    /// </summary>
    public sealed class SimpleSlotFilterPolicy : ISlotFilterPolicy
    {
        private readonly ISlotProfileProvider _profiles;
        private readonly IItemMetadataProvider _items;
        private readonly IStackingPolicy _stacking;

        /// <summary>
        /// Creates the filter combining profile, item metadata and stackable policy.
        /// </summary>
        public SimpleSlotFilterPolicy(ISlotProfileProvider slotProfileProvider, IItemMetadataProvider itemMetadataProvider, IStackingPolicy stackingPolicy)
        {
            _profiles = slotProfileProvider ?? throw new ArgumentNullException(nameof(slotProfileProvider));
            _items = itemMetadataProvider ?? throw new ArgumentNullException(nameof(itemMetadataProvider));
            _stacking = stackingPolicy ?? throw new ArgumentNullException(nameof(stackingPolicy));
        }

        ///<inheritdoc/>
        public bool CanAccept(string slotProfileId, string definitionId, out int effectiveMaxStack, out string reason)
        {
            effectiveMaxStack = 0;
            reason = null;

            SlotProfile profile;
            if (!_profiles.TryGet(slotProfileId, out profile))
            {
                reason = $"Perfil '{slotProfileId}' no encontrado";
                return false;
            }

            ItemMeta meta;
            if (!_items.TryGet(definitionId, out meta))
            {
                reason = $"Item '{definitionId}' mo encontrado";
                return false;
            }
            
            // 1) Compatibilidad por tipo/tags
            if (profile.AllowedTypes != null && profile.AllowedTypes.Length > 0)
            {
                bool ok = profile.AllowedTypes.Contains(meta.TypeId);
                if (!ok)
                {
                    reason = $"Tipo '{meta.TypeId}' no permitido en perfil '{slotProfileId}'";
                    return false;
                }
            }

            if (profile.RequiredTags != null && profile.RequiredTags.Length > 0)
            {
                var hasAll = profile.RequiredTags.All(t => meta.Tags != null && meta.Tags.Contains(t));
                if (!hasAll)
                {
                    reason = $"Faltan tags requeridos en '{definitionId}'";
                    return false;
                }
            }

            if (profile.BannedTags != null && profile.BannedTags.Length > 0)
            {
                var banned = meta.Tags != null && meta.Tags.Any(t => profile.BannedTags.Contains(t));
                if (banned)
                {
                    reason = $"Tag prohibido en '{definitionId}' para perfil '{slotProfileId}'";
                    return false;
                }
            }

            // 2) Máximo base por ítem (policy per-item)
            int baseMax = _stacking.GetMaxPerStack(definitionId, meta.TypeId);

            // 3) Overrides del perfil (si los hay)
            if (profile.HasStackableOverride)
            {
                if (!profile.StackableOverride)
                {
                    effectiveMaxStack = 1; // no apilable
                    return true;
                }

                if (profile.MaxStackOverride > 0)
                {
                    effectiveMaxStack = Math.Min(baseMax, profile.MaxStackOverride);
                    return true;
                }
            }

            // Sin override de perfil → usa baseMax del ítem
            effectiveMaxStack = baseMax;
            return true;
        }
    }
}