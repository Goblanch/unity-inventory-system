using GB.Inventory.Application.Abstractions;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using System.Linq;
using System.Collections.Generic;
using System;

namespace GB.Inventory.Application
{
    /// <summary>
    /// Concrete implementation of IInventoryService.
    /// Acts as the public entry point to interact with the inventory model.
    /// While encapsulating low-level domain logic and policies.
    /// </summary>
    public sealed class InventoryService : IInventoryService
    {
        private readonly IInventory _inventory;
        private readonly IEffectRegistry _effects;
        private readonly IUsagePhasePolicy _phasePolicy;

        public InventoryService(IInventory inventory, IEffectRegistry effects = null, IUsagePhasePolicy phasePolicy = null)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _effects = effects;
            _phasePolicy = phasePolicy;
        }

        public int Capacity => _inventory.Capacity;

        /// <summary>
        /// Exposes a read-only view of all inventory slots.
        /// Each element may represent a stack or an empty slot.
        /// </summary>
        public IReadOnlyList<IStack> SlotsView => _inventory.Slots.Select(s => s.IsEmpty ? null : s.Stack).ToList(); // ! KHE

        /// <inheritdoc/>
        public bool TryAdd(string definitionId, int count, out int slotIndex, out string reason) =>
            _inventory.TryAdd(definitionId, count, out slotIndex, out reason);

        /// <inheritdoc/>
        public bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason) =>
            _inventory.TrySplit(slotIndex, count, out newSlotIndex, out reason);

        /// <inheritdoc/>
        public bool TryMove(int srcSlot, int dstSlot, out string reason) =>
            _inventory.TryMove(srcSlot, dstSlot, out reason);

        /// <inheritdoc/>
        public bool TrySetSlotProfile(int slotIndex, string slotProfileId, out string reason) =>
            _inventory.TrySetSlotProfile(slotIndex, slotProfileId, out reason);

        /// <inheritdoc/>
        public string GetSlotProfileId(int slotIndex) => _inventory.GetSlotProfileId(slotIndex);

        /// <inheritdoc/>
        public bool SetCapacity(int newCapacity, out string reason) => _inventory.TrySetCapacity(newCapacity, out reason);

        /// <inheritdoc/>
        public bool IncreaseCapacity(int delta, out string reason) => _inventory.IncreaseCapacity(delta, out reason);

        /// <inheritdoc/>
        public bool TryClear(int slotIndex, out string reason) =>
            _inventory.TryClear(slotIndex, out reason);


        /// <inheritdoc/>
        public bool TryUse(int slotIndex, ITurnContext ctx, out UseResult result, out string reason)
        {
            result = default;
            reason = null;

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

            if (_effects == null)
            {
                reason = "No hay EffectRegistry configurado";
                return false;
            }

            var defId = slot.Stack.DefinitionId;

            if (_phasePolicy != null)
            {
                var phases = _effects.GetAllowedPhases(defId);
                if (!_phasePolicy.CanUse(defId, phases, ctx, out var whyPhase))
                {
                    reason = whyPhase;
                    result = UseResult.Fail(whyPhase);
                    return false;
                }
            }

            if (!_effects.TryResolve(defId, out var effect))
            {
                reason = $"No hay efecto para {defId}";
                result = UseResult.Fail(reason);
                return false;
            }

            _effects.TryGetPayload(defId, out var payload);

            var res = effect.Apply(ctx, defId, payload);
            result = res;

            if (res.Success && res.ConsumeOne)
            {
                var stack = slot.Stack;
                if (stack.Count == 1) _inventory.TryClear(slotIndex, out _);
                else if (_inventory.TrySplit(slotIndex, 1, out var tmp, out _)) _inventory.TryClear(tmp, out _);
            }

            return res.Success;
        }

    }
}


