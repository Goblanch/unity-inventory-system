using System.Collections.Generic;

namespace GB.Inventory.Domain.Abstractions
{
    public interface IInventory
    {
        int Capacity { get; }
        IReadOnlyList<IInventorySlot> Slots { get; }

        bool TryAdd(Item item, out int slotIndex);
        bool TryRemoveAt(int slotIndex, out Item removed);

        bool TrySetCapacity(int newCapacity, out string reason);
        bool IncreaseCapacity(int delta, out string reason);
    }

    public interface IInventorySlot
    {
        int Index { get; }
        Item Item { get; }
        bool IsEmpty { get; }
    }

}


