using GB.Inventory.Domain.Abstractions;
using System.Collections.Generic;
using System;
using Codice.Client.BaseCommands.Merge.Xml;

namespace GB.Inventory.Domain
{
    public sealed class InventoryModel : IInventory
    {
        private readonly List<Slot> _slots;
        private readonly List<ISlot> _slotsView; // para exponer
        private readonly IStackingPolicy _stacking;
        private readonly ISlotFilterPolicy _filter;

        private readonly string _defaultProfileId;

        public int Capacity => _slots.Count;
        public IReadOnlyList<ISlot> Slots => _slotsView;

        public InventoryModel(int initialCapacity, IStackingPolicy stacking, ISlotFilterPolicy filter, string defaultSlotProfileId = "Default")
        {
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            _stacking = stacking ?? throw new ArgumentNullException(nameof(stacking));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _defaultProfileId = string.IsNullOrWhiteSpace(defaultSlotProfileId) ? "Default" : defaultSlotProfileId;

            _slots = new List<Slot>(initialCapacity);
            _slotsView = new List<ISlot>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++) AddSlotInternal(_defaultProfileId);
        }

        private void AddSlotInternal(string profileId)
        {
            var slot = new Slot(_slots.Count, profileId);
            _slots.Add(slot);
            _slotsView.Add(slot);
        }
        
        #region API PERFILES
        public bool TrySetSlotProfile(int slotIndex, string slotProfileId, out string reason)
        {
            reason = null;
            if ((uint)slotIndex >= (uint)_slots.Count)
            {
                reason = "Slot index fuera de rango";
                return false;
            }

            _slots[slotIndex].SetProfile(string.IsNullOrWhiteSpace(slotProfileId) ? "Default" : slotProfileId);
            return true;
        }

        public string GetSlotProfileId(int slotIndex)
        {
            if ((uint)slotIndex >= (uint)_slots.Count) return "Invalid";
            return _slots[slotIndex].SlotProfileId;
        }
        #endregion

        #region STACKING API
        // ! HAY QUE MODIFICAR COSAS AQUÍ
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
                    if(_filter.CanAccept(s.SlotProfileId, definitionId, out var maxEff, out _))
                    {
                        int canAdd = System.Math.Min(count, System.Math.Max(0, maxEff - s.Stack.Count));
                        if(canAdd > 0 && s.TryMergeIn(definitionId, canAdd, _stacking, out var merged, out _))
                        {
                            count -= merged;
                            slotIndex = i;
                            if (count <= 0) return true;
                        }
                    }
                }
            }

            // 2) Colocar lo restante en el primer slot vacío (respetando maxStack)
            for (int i = 0; i < _slots.Count && count > 0; i++)
            {
                var s = _slots[i];
                if (!s.IsEmpty) continue;

                if(_filter.CanAccept(s.SlotProfileId, definitionId, out var maxEff, out string why))
                {
                    int place = System.Math.Min(count, maxEff);
                    if(place > 0 && s.TryCreate(definitionId, place))
                    {
                        count -= place;
                        if (slotIndex < 0) slotIndex = i;
                        if (count <= 0) return true;
                    }
                }
            }

            // Si queda remanente, no cabe
            if (slotIndex >= 0)
            {
                reason = "Inventario sin espacio para el resto del stack";
                return false; // Se ha añadido parte pero no cabe todo
            }

            reason = "Inventario lleno o perfiles no compatibles";
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

            if (!source.TrySplit(count, out var chunk, out reason)) return false;

            // COlocar en primer slot vacío compatible
            for (int i = 0; i < _slots.Count; i++)
            {
                var destination = _slots[i];
                if (!destination.IsEmpty) continue;

                if(_filter.CanAccept(destination.SlotProfileId, chunk.DefinitionId, out var maxEff, out _))
                {
                    int place = System.Math.Min(chunk.Count, maxEff);
                    if(place == chunk.Count)
                    {
                        destination.TryCreate(chunk.DefinitionId, chunk.Count);
                        newSlotIndex = i;
                        return true;
                    }
                }
            }

            // No hay espacio: revertimos split
            source.TryMergeIn(chunk.DefinitionId, chunk.Count, _stacking, out _, out _);
            reason = "No hay slot compatible para el split";
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

            var def = source.Stack.DefinitionId;

            if(!_filter.CanAccept(destination.SlotProfileId, def, out var maxEff, out var why))
            {
                reason = why;
                return false;
            }

            if (destination.IsEmpty)
            {
                if (source.TryClear(out var removed) && removed != null)
                {
                    int place = System.Math.Min(removed.Count, maxEff);
                    destination.TryCreate(removed.DefinitionId, place);
                    int leftover = removed.Count - place;
                    if (leftover > 0)
                    {
                        // Devolver lo que sobra al origen
                        source.TryCreate(removed.DefinitionId, leftover);
                        reason = "Movimiento parcial por límite de stack del perfil destino";
                        return false;
                    }
                    return true;
                }

                reason = "No se pudo mover";
                return false;
            }

            // Merge si mismo def
            if (destination.Stack.DefinitionId == def)
            {
                int canAdd = System.Math.Min(source.Stack.Count, System.Math.Max(0, maxEff - destination.Stack.Count));
                if (canAdd > 0)
                {
                    source.TryTake(canAdd, out var taken, out _);
                    destination.TryMergeIn(def, taken, _stacking, out _, out _);
                    if (source.IsEmpty) return true;
                    reason = "Merge parcial, origen aún tiene remanente";
                    return false;
                }

                reason = "Destino no admite más unidades por su maxStack effectivo";
                return false;
            }

            // Distinta definición: swap solo si ambos perfiles aceptan
            if(!_filter.CanAccept(source.SlotProfileId, destination.Stack.DefinitionId, out _, out why))
            {
                reason = "Swap inválido: el origen no acepta el item de destino";
                return false;
            }

            source.TryClear(out var a);
            destination.TryClear(out var b);
            if (a != null) destination.TryCreate(a.DefinitionId, System.Math.Min(a.Count, maxEff));
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
                    AddSlotInternal(_defaultProfileId);

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

        #endregion

        #region CLEAR SLOT
        public bool TryClear(int slotIndex, out string reason)
        {
            reason = null;

            if ((uint)slotIndex >= (uint)_slots.Count)
            {
                reason = "SlotIndex fuera de rango";
                return false;
            }

            var slot = _slots[slotIndex];
            if (slot.IsEmpty)
            {
                reason = "El slot ya está vacío";
                return false;
            }

            slot.TryClear(out _);
            return true;
        }

        #endregion
    }
}


