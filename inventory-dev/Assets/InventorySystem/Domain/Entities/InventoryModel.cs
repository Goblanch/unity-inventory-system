using GB.Inventory.Domain.Abstractions;
using System.Collections.Generic;
using System;

namespace GB.Inventory.Domain
{
    public sealed class InventoryModel : IInventory
    {
        private readonly List<Slot> _slots;
        private readonly List<ISlot> _slotsView; // para exponer
        private readonly IStackingPolicy _stacking;

        public int Capacity => _slots.Count;
        public IReadOnlyList<ISlot> Slots => _slotsView;

        public InventoryModel(int initialCapacity, IStackingPolicy stacking)
        {
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            _stacking = stacking ?? throw new ArgumentNullException(nameof(stacking));

            _slots = new List<Slot>(initialCapacity);
            _slotsView = new List<ISlot>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++) AddSlotInternal();
        }
        
        private void AddSlotInternal()
        {
            var slot = new Slot(_slots.Count);
            _slots.Add(slot);
            _slots.Add(slot);
        }

        public bool TryAdd(string definitionId, int count, out int slotIndex, out string reason)
        {
            slotIndex = -1;
            reason = null;
            if (string.IsNullOrWhiteSpace(definitionId))
            {
                reason = "definitionId vacío";
                return false;
            }

            if (count <= 0)
            {
                reason = "count debe ser > 0";
                return false;
            }

            // 1) Intentar merge en algún slot existente compatible
            for (int i = 0; i < _slots.Count && count > 0; i++)
            {
                var s = _slots[i];
                if (s.IsEmpty) continue;

                if (s.Stack.DefinitionId == definitionId)
                {
                    if (s.TryMergeIn(definitionId, count, _stacking, out var merged, out _))
                    {
                        count -= merged;
                        slotIndex = i;
                        if (count <= 0) return false;
                    }
                }
            }

            // 2) Colocar lo restante en el primer slot vacío (respetando maxStack)
            for (int i = 0; i < _slots.Count && count > 0; i++)
            {
                var s = _slots[i];
                if (!s.IsEmpty) continue;

                int max = _stacking.GetMaxStack(definitionId);
                int place = Math.Min(count, max);
                if (s.TryCreate(definitionId, place))
                {
                    count -= place;
                    if (slotIndex < 0) slotIndex = i;
                    if (count <= 0) return true;
                }
            }

            // Si queda remanente, no cabe
            if (slotIndex >= 0)
            {
                reason = "Inventario sin espacio para el resto del stack";
                return false; // Se ha añadido parte pero no cabe todo
            }

            reason = "Inventario lleno";
            return false; // No cabe nada
        }

        public bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason)
        {
            newSlotIndex = -1;
            reason = null;
            if ((uint)slotIndex >= (uint)_slots.Count)
            {
                reason = "slotIndex fuera de rango";
                return false;
            }

            var source = _slots[slotIndex];

            if (!source.TrySplit(count, out var chunk, out reason))
                return false;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _slots[i].TryCreate(chunk.DefinitionId, chunk.Count);
                    newSlotIndex = i;
                    return true;
                }
            }

            // No hay espacio: revertimos split
            source.TryMergeIn(chunk.DefinitionId, chunk.Count, _stacking, out _, out _);
            reason = "No hay slot libre para el split";
            return false;
        }

        public bool TryMove(int srcSlot, int dstSlot, out string reason)
        {
            reason = null;
            if ((uint)srcSlot >= (uint)_slots.Count || (uint)dstSlot >= (uint)_slots.Count)
            {
                reason = "Índices fuera de rango";
                return false;
            }
            if (srcSlot == dstSlot) return true;

            var source = _slots[srcSlot];
            var destination = _slots[dstSlot];

            if (source.IsEmpty)
            {
                reason = "Origen vacío";
                return false;
            }

            if (destination.IsEmpty)
            {
                if (source.TryClear(out var removed) && removed != null)
                {
                    destination.TryCreate(removed.DefinitionId, removed.Count);
                    return true;
                }

                reason = "No se pudo mover";
                return false;
            }

            // Si mismo def policy: merge
            if (destination.Stack.DefinitionId == source.Stack.DefinitionId)
            {
                if (_stacking.CanMerge(destination.Stack.DefinitionId, destination.Stack.Count, destination.Stack.Count, out var canMerge, out var why))
                {
                    if (canMerge > 0)
                    {
                        source.TryTake(canMerge, out var taken, out _);
                        destination.TryMergeIn(destination.Stack.DefinitionId, taken, _stacking, out _, out _);
                        if (source.IsEmpty) return true;
                        reason = "Merge parcial, origen aún tiene remanente";
                        return false;
                    }

                    reason = why ?? "No se pudo combinar";
                    return false;
                }

                reason = why ?? "No se pudo combinar";
                return false;
            }

            // Distinto def policy: swap
            source.TryClear(out var a);
            destination.TryClear(out var b);
            if (a != null) destination.TryCreate(a.DefinitionId, a.Count);
            if (b != null) source.TryCreate(b.DefinitionId, b.Count);
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

            if(newCapacity > Capacity)
            {
                int toAdd = newCapacity - Capacity;
                for (int i = 0; i < toAdd; i++)
                    AddSlotInternal();

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

                // Eliminar de ambas vistas
                _slots.RemoveRange(newCapacity, _slots.Count - newCapacity);
                _slotsView.RemoveRange(newCapacity, _slotsView.Count - newCapacity);
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


