namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Resolves the base max-per-stack for a given item, optionally considering its type.
    /// This value is usually further clamped by the slot profile policy.
    /// </summary>
    public interface IStackingPolicy
    {   
        /// <summary>
        /// Returns the base maximum amount allowed in a single stack for a given item.
        /// </summary>
        /// <param name="definitionId">Item definition ID.</param>
        /// <param name="typeId">Item type identifier (optional usage depends on the implementation).</param>
        /// <returns>Base max-per-stack for that item.</returns>
        int GetMaxPerStack(string definitionId, string typeId);
    }
}