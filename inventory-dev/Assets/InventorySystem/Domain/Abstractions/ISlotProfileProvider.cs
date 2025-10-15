namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Proveedor de perfiles para slots (por id). Más adelante será un SO
    /// </summary>
    public interface ISlotProfileProvider
    {
        SlotProfile Get(string slotProfileId);
    }
}