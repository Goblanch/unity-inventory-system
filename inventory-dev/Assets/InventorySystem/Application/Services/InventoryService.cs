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
        private readonly IEffectRegistry _effects;

        public InventoryService(IInventory inventory, IEffectRegistry effects = null)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _effects = effects;
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

        public bool TryClear(int slotIndex, out string reason) =>
            _inventory.TryClear(slotIndex, out reason);

        public bool TryUse(int slotIndex, ITurnContext ctx, out UseResult result, out string reason)
        {
            result = default;
            reason = null;

            if (_effects == null)
            {
                reason = "No hay EffectRegistry configurado";
                return false;
            }

            var slots = _inventory.Slots;
            if ((uint)slotIndex >= (uint)slots.Count)
            {
                reason = "slotIndex fuera de rango";
                return false;
            }

            var slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                reason = "Slot vacío";
                return false;
            }

            var stack = slot.Stack;
            if (!_effects.TryResolve(stack.DefinitionId, out var effect))
            {
                reason = $"No hay efecto para '{stack.DefinitionId}'";
                return false;
            }

            var res = effect.Apply(ctx, stack.DefinitionId, null);
            result = res;
            if (res.Success && res.ConsumeOne)
            {
                if (stack.Count == 1)
                {
                    _inventory.TryClear(slotIndex, out _);
                }
                else
                {
                    if (_inventory.TrySplit(slotIndex, 1, out var tmpSlot, out _)) _inventory.TryClear(tmpSlot, out _);
                }
            }

            return res.Success;
        }

    }
}


