using GB.Inventory.Domain.Abstractions;
using UnityEditor.Build;

namespace GB.Inventory.Domain.Policies
{
    /// <summary>
    /// Política simple: usa SlotProfile + ItemMeta, y combina con stackingPolicy para devolver el maxStack efectivo
    /// Reglas:
    ///  - AllowedTypes: si se define, el TypeId del item debe estar incluido.
    ///  - RequiredTags: si se define, todos deben estar en los tags del item.
    ///  - BannedTags: ninguno debe estar en los tags del item
    ///  - StackableOverride: si existe y es false -> maxStack = 1; si true -> respeta a MaxStackOverride o stackingPolicy.
    ///  - MaxStack efectivo = min(MaxStackOverride > 0 ? override : stackingPolicy.GetMaxStack)
    /// </summary>
    public sealed class SimpleSlotFilterPolicy : ISlotFilterPolicy
    {
        private readonly ISlotProfileProvider _profiles;
        private readonly IItemMetadataProvider _meta;
        private readonly IStackingPolicy _stacking;

        public SimpleSlotFilterPolicy(ISlotProfileProvider profiles, IItemMetadataProvider meta, IStackingPolicy stacking)
        {
            _profiles = profiles;
            _meta = meta;
            _stacking = stacking;
        }

        public bool CanAccept(string slotProfileId, string definitionId, out int effectiveMaxStack, out string reason)
        {
            effectiveMaxStack = 0;
            reason = null;

            var profile = _profiles.Get(slotProfileId);
            var meta = _meta.Get(definitionId);

            // AllowedTypes
            if (profile.AllowedTypes != null && profile.AllowedTypes.Length > 0)
            {
                bool okType = false;
                for (int i = 0; i < profile.AllowedTypes.Length; i++)
                {
                    if (profile.AllowedTypes[i] == meta.TypeId)
                    {
                        okType = true;
                        break;
                    }
                }
                if (!okType)
                {
                    reason = $"Tipo '{meta.TypeId}' no permitido por el slot '{slotProfileId}'.";
                    return false;
                }
            }

            // RequiredTags
            if (profile.RequiredTags != null && profile.RequiredTags.Length > 0)
            {
                for (int i = 0; i < profile.RequiredTags.Length; i++)
                {
                    string req = profile.RequiredTags[i];
                    bool found = false;
                    if (meta.Tags != null)
                    {
                        for (int t = 0; t < meta.Tags.Length; t++)
                        {
                            if (meta.Tags[t] == req)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        reason = $"Falta tag requerida '{req}' para el slot '{slotProfileId}'";
                        return false;
                    }
                }
            }

            // BannedTags
            if (profile.BannedTags != null && profile.BannedTags.Length > 0 && meta.Tags != null)
            {
                for (int i = 0; i < profile.BannedTags.Length; i++)
                {
                    string banned = profile.BannedTags[i];
                    for (int t = 0; t < meta.Tags.Length; t++)
                    {
                        if (meta.Tags[t] == banned)
                        {
                            reason = $"Tag prohibido '{banned}' para el slot '{slotProfileId}'";
                            return false;
                        }
                    }
                }
            }

            // Stackable/MaxStack
            var baseMax = _stacking.GetMaxStack(definitionId);
            int overrideMax = profile.MaxStackOverride > 0 ? profile.MaxStackOverride : int.MaxValue;

            if (profile.HasStackableOverride)
            {
                if (!profile.StackableOverride)
                {
                    effectiveMaxStack = 1;
                    return true;
                }
            }

            effectiveMaxStack = System.Math.Min(baseMax, overrideMax);
            if (effectiveMaxStack < 1) effectiveMaxStack = 1;
            return true;
        }
    }
}