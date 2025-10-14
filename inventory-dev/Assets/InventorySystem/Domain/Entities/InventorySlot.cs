using System;
using GB.Inventory.Domain.Abstractions;

namespace GB.Inventory.Domain
{
    public sealed class InventorySlot : IInventorySlot
    {
        public int Index { get; }

        public Item Item { get; private set; }

        public bool IsEmpty => Item == null;

        public InventorySlot(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            Item = null;
        }

        public void Set(Item item)
        {
            if (!IsEmpty) throw new InvalidOperationException($"Slot {Index} ya ocupado.");
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public Item Remove()
        {
            if (IsEmpty) throw new InvalidOperationException($"Slot {Index} ya vacío.");
            var removed = Item;
            Item = null;
            return removed;
        }
    }
}


