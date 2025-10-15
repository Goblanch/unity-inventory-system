using System.Collections.Generic;

namespace GB.Inventory.Domain.Abstractions
{
    public interface IInventory
    {
        int Capacity { get; }
        IReadOnlyList<ISlot> Slots { get; }

        bool TryAdd(string definitionId, int count, out int slotIndex, out string reason);
        bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason);
        bool TryMove(int srcSlot, int dstSlot, out string reason);

        bool TrySetCapacity(int newCapacity, out string reason);
        bool IncreaseCapacity(int delta, out string reason);
    }

    public interface ISlot
    {
        int Index { get; }
        IStack Stack { get; }
        bool IsEmpty { get; }
    }

}


