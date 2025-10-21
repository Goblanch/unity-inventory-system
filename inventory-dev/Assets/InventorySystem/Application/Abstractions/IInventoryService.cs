using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using System.Collections.Generic;

namespace GB.Inventory.Application.Abstractions
{
    public interface IInventoryService
    {
        int Capacity { get; }
        IReadOnlyList<IStack> SlotsView { get; }

        bool TryAdd(string definitionId, int count, out int slotIndex, out string reason);
        bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason);
        bool TryMove(int srcSlot, int dstSlot, out string reason);

        // Perfiles
        bool TrySetSlotProfile(int slotIndex, string slotProfileId, out string reason);
        string GetSlotProfileId(int slotIndex);

        // Capacidad
        bool SetCapacity(int newCapacity, out string reason);
        bool IncreaseCapacity(int delta, out string reason);

        // Vaciar Slot
        bool TryClear(int slotIndex, out string reason);

        // Use slot item
        bool TryUse(int slotIndex, ITurnContext ctx, out UseResult result, out string reason);
    }
}