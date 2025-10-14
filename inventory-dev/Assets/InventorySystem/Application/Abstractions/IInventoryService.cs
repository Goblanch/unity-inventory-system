using GB.Inventory.Domain;
using System.Collections.Generic;

namespace GB.Inventory.Application.Abstractions
{
    public interface IInventoryService
    {
        int Capacity { get; }
        IReadOnlyList<Item> SlotsView { get; }

        bool TryAdd(Item item, out int slotIndex);
        bool TryRemoveAt(int slotIndex, out Item removed);

        bool SetCapacity(int newCapacity, out string reason);
        bool IncreaseCapacity(int delta, out string reason);
    }
}