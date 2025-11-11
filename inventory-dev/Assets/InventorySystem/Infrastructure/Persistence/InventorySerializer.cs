using System;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Domain.Abstractions;

namespace GB.Inventory.Infrastructure.Persistence
{
    /// <summary>
    /// Bridges IInventoryService to plain DTOs for persistence.
    /// </summary>
    public static class InventorySerializer
    {
        public static InventorySaveDTO Capture(IInventoryService svc)
        {
            if (svc == null) throw new ArgumentNullException(nameof(svc));

            var dto = new InventorySaveDTO
            {
                version = 1,
                savedAtIsoUtc = DateTime.UtcNow.ToString("o"),
                capacity = svc.Capacity
            };

            var view = svc.SlotsView;

            for (int i = 0; i < view.Count; i++)
            {
                var slot = view[i];
                if (slot == null) continue;

                dto.slots.Add(new SlotDTO
                {
                    index = i,
                    slotProfileId = svc.GetSlotProfileId(i) ?? "Default",
                    definitionId = slot.DefinitionId,
                    count = slot.Count
                });
            }

            return dto;
        }

        public static bool Restore(IInventoryService svc, InventorySaveDTO dto, out string reason)
        {
            reason = null;
            if (svc == null) { reason = "Service is null"; return false; }
            if (dto == null) { reason = "DTO is null"; return false; }

            // 1. Capacity
            if (!svc.SetCapacity(dto.capacity, out reason))
                return false;

            // 2. Profiles (set first, so add operations respect profile limits)
            foreach (var s in dto.slots)
            {
                if (s.index < 0 || s.index >= dto.capacity) continue;
                svc.TrySetSlotProfile(s.index, string.IsNullOrWhiteSpace(s.slotProfileId) ? "Default" : s.slotProfileId, out _);
            }

            // 3. Stacks
            foreach (var s in dto.slots)
            {
                if (string.IsNullOrWhiteSpace(s.definitionId) || s.count <= 0) continue;
                svc.TryAdd(s.definitionId, s.count, out _, out _);
            }

            return true;
        }
    }
}