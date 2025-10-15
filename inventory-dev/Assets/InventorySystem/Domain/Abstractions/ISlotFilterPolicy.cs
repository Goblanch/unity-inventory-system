namespace GB.Inventory.Domain.Abstractions
{
    public interface ISlotFilterPolicy
    {
        /// <summary>
        /// ¿Puede este slot aceptar (crear/merge) items de esta definición?
        /// Debe validar tipo/tags y devolver además el maxStack efectivo del slot
        /// </summary>
        /// <param name="slotProfileId"></param>
        /// <param name="definitionId"></param>
        /// <param name="effectiveMaxStack"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool CanAccept(string slotProfileId, string definitionId, out int effectiveMaxStack, out string reason);
    }
}