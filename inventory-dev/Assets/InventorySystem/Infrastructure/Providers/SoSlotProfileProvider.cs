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
            _db = db;
        }

        public SlotProfile Get(string slotProfileId)
        {
            if (_db != null && _db.TryGet(slotProfileId, out var def) && def != null)
            {
                return new SlotProfile
                {
                    Id = def.ProfileId,
                    AllowedTypes = def.AllowedTypes,
                    RequiredTags = def.RequiredTags,
                    BannedTags = def.BannedTags,
                    HasStackableOverride = def.HasStackableOverride,
                    StackableOverride = def.StackableOverride,
                    MaxStackOverride = def.MaxStackOverride
                };
            }
            
            // Fallback "Default": sin restricciones.
            return new SlotProfile
            {
                Id = slotProfileId ?? "Default",
                AllowedTypes = System.Array.Empty<string>(),
                RequiredTags = System.Array.Empty<string>(),
                BannedTags = System.Array.Empty<string>(),
                HasStackableOverride = false,
                StackableOverride = true,
                MaxStackOverride = 0
            };
        }
    }
}


