using GB.Inventory.Domain.Abstractions;
using System.Collections.Generic;
using System;

namespace GB.Inventory.Domain
{
    public sealed class InventoryModel : IInventory
    {
        private readonly List<InventorySlot> _slots;

        public int Capacity => _slots.Count;
        public IReadOnlyList<IInventorySlot> Slots => _slots;

        public InventoryModel(int initialCapacity = 3)
        {
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            _slots = new List<InventorySlot>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
            {
                _slots.Add(new InventorySlot(i));
            }
        }

        public bool TryAdd(Item item, out int slotIndex)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _slots[i].Set(item);
                    slotIndex = i;
                    return true;
                }
            }

            slotIndex = -1;
            return false;
        }

        public bool TryRemoveAt(int slotIndex, out Item removed)
        {
            removed = null;
            if ((uint)slotIndex >= (uint)_slots.Count) return false;

            var slot = _slots[slotIndex];
            if (slot.IsEmpty) return false;

            removed = slot.Remove();
            return true;
        }

        public bool TrySetCapacity(int newCapacity, out string reason)
        {
            reason = null;

            if (newCapacity < 0)
            {
                reason = "La capacidad no puede ser negativa";
                return false;
            }

            if (newCapacity == Capacity) return true;

            if (newCapacity > Capacity)
            {
                int toAdd = newCapacity - Capacity;
                int start = _slots.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    _slots.Add(new InventorySlot(start + i));
                }
                return true;
            }
            else
            {
                for (int i = newCapacity; i < _slots.Count; i++)
                {
                    if (!_slots[i].IsEmpty)
                    {
                        reason = $"No se puede reducir a {newCapacity}: el slot {i} no está vacío";
                        return false;
                    }
                }

                _slots.RemoveRange(newCapacity, _slots.Count - newCapacity);
                return true;
            }
        }

        public bool IncreaseCapacity(int delta, out string reason)
        {
            reason = null;
            if (delta <= 0)
            {
                reason = "El incremento debe ser >= 1";
                return false;
            }

            return TrySetCapacity(Capacity + delta, out reason);
        }

    }
}


