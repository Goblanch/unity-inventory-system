using System;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Infrastructure.Definitions;

namespace GB.Inventory.Infrastructure.Providers
{
    /// <summary>
    /// Traduce ItemDefinition (SO) -> ItemMeta (Dominio)
    /// </summary>
    public sealed class SoSlotProfileProvider : ISlotProfileProvider
    {
        private readonly SlotProfileDatabase _db;

        public SoSlotProfileProvider(SlotProfileDatabase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public bool TryGet(string slotProfileId, out SlotProfile profile)
        {
            profile = default(SlotProfile);

            if (string.IsNullOrWhiteSpace(slotProfileId)) return false;

            SlotProfileDefinition def;
            if (_db.TryGet(slotProfileId, out def) && def != null)
            {
                profile = new SlotProfile
                {
                    Id = def.ProfileId,
                    AllowedTypes = def.AllowedTypes ?? Array.Empty<string>(),
                    RequiredTags = def.RequiredTags ?? Array.Empty<string>(),
                    BannedTags = def.BannedTags ?? Array.Empty<string>(),
                    HasStackableOverride = def.HasStackableOverride,
                    StackableOverride = def.StackableOverride,
                    MaxStackOverride = def.MaxStackOverride
                };
                return true;
            }
            return false;
        }

        // Compatibilidad con código existente que usa Get
        // Devuelve null si no existe (asumiendo SlotProfile es class)
        public SlotProfile Get(string slotProfileId)
        {
            SlotProfile p;
            return TryGet(slotProfileId, out p) ? p : null;
        }
    }
}


