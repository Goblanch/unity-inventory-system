namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Determines wether a slot (by profile ID) can accept an item definition,
    /// and computes the effective max-per-stack for that item in that slot.
    /// The effective maximum is typically the min between item-level and profile-level constraints.
    /// </summary>
    public interface ISlotFilterPolicy
    {
        /// <summary>
        /// Validates compatibility and calculates the effective max-per-stack for the given item in a slot profile.
        /// </summary>
        /// <param name="slotProfileId">Profile identifier assigned to the destination slot.</param>
        /// <param name="definitionId">Item definition ID requested to be added/merged.</param>
        /// <param name="effectiveMaxStack">Outputs the maximum number of units allowed in a single stack for this (profile, item) pairing.</param>
        /// <param name="reason">Outputs an explanation when the item is no accepted.</param>
        /// <returns>True if the item can be placed in a slot with this profile; otherwise false.</returns>
        bool CanAccept(string slotProfileId, string definitionId, out int effectiveMaxStack, out string reason);
    }
}