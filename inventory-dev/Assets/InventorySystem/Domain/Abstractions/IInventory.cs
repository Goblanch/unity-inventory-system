using System.Collections.Generic;

namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Low-level inventory contract for domain operations.
    /// Provides slot-based manipulation (add, move, split, clear), capacity changes,
    /// and slot profile management. UI/gameplay should normally go through the Application layer.
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// Current number of slots in the inventory.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Read-only view over slot objects. A slot may be empty (its <c>Stack</c> is null).
        /// The order is stable: index corresponds to the slot position.
        /// </summary>
        IReadOnlyList<ISlot> Slots { get; }

        /// <summary>
        /// Attemps to add a new item stack into the inventory.
        /// The operation respects the slot profiles, stacking limits and type filters.
        /// </summary>
        /// <param name="definitionId">The unique definition ID of the item to add.</param>
        /// <param name="count">The number of items to add.</param>
        /// <param name="slotIndex">Outputs the first slot index where items were placed.</param>
        /// <param name="reason">Outputs a message describing why the operation failed, if it fails.</param>
        /// <returns>True if all items were successfully added; false if partially added or blocked.</returns>
        bool TryAdd(string definitionId, int count, out int slotIndex, out string reason);

        /// <summary>
        /// Attempts to split a stack from one slot into another compatible empty slot.
        /// </summary>
        /// <param name="slotIndex">The index of the slot containing the source stack.</param>
        /// <param name="count">The amount to split into a new slot.</param>
        /// <param name="newSlotIndex">Outputs the index of the slot receiving the new stack.</param>
        /// <param name="reason">Outputs a message describing why the operation failed.</param>
        /// <returns>True if the split was successful; false otherwise.</returns>
        bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason);

        /// <summary>
        /// Attempts to move a stack or part of the stack from one slot to another.
        /// Merging will occur if both slots share the same item type and stacking policy allows it.
        /// </summary>
        /// <param name="srcSlot">Index of the source slot.</param>
        /// <param name="dstSlot">Index of the destination slot.</param>
        /// <param name="reason">Outputs a message describing why the operation failed.</param>
        /// <returns>True if the move was completed (even partially); false otherwise.</returns>
        bool TryMove(int srcSlot, int dstSlot, out string reason);

        /// <summary>
        /// Attempts to change the slot profile for a given slot.
        /// Profiles define with item types, tags, and stack sizes are allowed in a slot.
        /// </summary>
        /// <param name="slotIndex">Slot index to update.</param>
        /// <param name="slotProfileId">The new profile ID to apply.</param>
        /// <param name="reason">Outputs a message if the operation is invalid.</param>
        /// <returns>True if the profile was changed successfully.</returns>
        bool TrySetSlotProfile(int slotIndex, string slotProfileId, out string reason);

        /// <summary>
        /// Gets the slot profile id of a given stack.
        /// </summary>
        /// <param name="slotIndex">The index of the slot.</param>
        /// <returns>The profile ID of the slot.</returns>
        string GetSlotProfileId(int slotIndex);

        /// <summary>
        /// Attempts to resize the inventory to a specific capacity.
        /// </summary>
        /// <param name="newCapacity">The desired number of slots.</param>
        /// <param name="reason">Outputs a message if the capacity change fails.</param>
        /// <returns>True if capacity was successfully updated.</returns>
        bool TrySetCapacity(int newCapacity, out string reason);

        /// <summary>
        /// Attempts to increase the capacity by a given delta.
        /// </summary>
        /// <param name="delta">Number of slots to add.</param>
        /// <param name="reason">Outputs a message if the increase fails.</param>
        /// <returns>True if capacity was increased.</returns>
        bool IncreaseCapacity(int delta, out string reason);

        /// <summary>
        /// Attempts to clear the contents of a slot, removing any stack it contains.
        /// </summary>
        /// <param name="slotIndex">Index of the slot to clear.</param>
        /// <param name="reason">Outputs a message describing why the operation failed.</param>
        /// <returns>True if a stack was removed; false if the slot was already empty.</returns>
        bool TryClear(int slotIndex, out string reason);
    }

    /// <summary>
    /// Represents a single inventory cell. May be empty (Stack == null).
    /// Holds the slot index and the current slot profile identifier.
    /// </summary>
    public interface ISlot
    {
        int Index { get; }
        IStack Stack { get; }
        bool IsEmpty { get; }
    }

}


