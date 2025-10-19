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
        public IReadOnlyList<IStack> SlotsView => _inventory.Slots.Select(s => s.IsEmpty ? null : s.Stack).ToList(); // ! KHE

        public bool TryAdd(string definitionId, int count, out int slotIndex, out string reason) =>
            _inventory.TryAdd(definitionId, count, out slotIndex, out reason);

        public bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason) =>
            _inventory.TrySplit(slotIndex, count, out newSlotIndex, out reason);

        public bool TryMove(int srcSlot, int dstSlot, out string reason) =>
            _inventory.TryMove(srcSlot, dstSlot, out reason);

        public bool TrySetSlotProfile(int slotIndex, string slotProfileId, out string reason) =>
            _inventory.TrySetSlotProfile(slotIndex, slotProfileId, out reason);

        public string GetSlotProfileId(int slotIndex) => _inventory.GetSlotProfileId(slotIndex);

        public bool SetCapacity(int newCapacity, out string reason) => _inventory.TrySetCapacity(newCapacity, out reason);
        public bool IncreaseCapacity(int delta, out string reason) => _inventory.IncreaseCapacity(delta, out reason);

    }
}


