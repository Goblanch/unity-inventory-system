using GB.Inventory.Domain;
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

        bool SetCapacity(int newCapacity, out string reason);
        bool IncreaseCapacity(int delta, out string reason);
    }
}