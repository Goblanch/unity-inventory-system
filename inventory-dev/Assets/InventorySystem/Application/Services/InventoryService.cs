using GB.Inventory.Application.Abstractions;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using System.Linq;
using System.Collections.Generic;
using System;

namespace GB.Inventory.Application
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly IInventory _inventory;

        public InventoryService(IInventory inventory)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public int Capacity => _inventory.Capacity;
        public IReadOnlyList<Item> SlotsView => _inventory.Slots.Select(s => s.Item).ToList(); // ! KHE

        public bool TryAdd(Item item, out int slotIndex) => _inventory.TryAdd(item, out slotIndex);
        public bool TryRemoveAt(int slotIndex, out Item removed) => _inventory.TryRemoveAt(slotIndex, out removed);
        public bool SetCapacity(int newCapacity, out string reason) => _inventory.TrySetCapacity(newCapacity, out reason);
        public bool IncreaseCapacity(int delta, out string reason) => _inventory.IncreaseCapacity(delta, out reason);   
     
    }
}


