namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Define las reglas para aplicar stacks (maxStack por definición y si se pueden combinar)
    /// </summary>
    public interface IStackingPolicy
    {
        int GetMaxStack(string definitionId);

        /// <summary>
        /// Determina si un stack existente puede combinarse con una entrada.
        /// Devuelve la cantidad que realmente se puede agregar sin superar los límites.
        /// </summary>
        /// <param name="definitionId"></param>
        /// <param name="existingCount"></param>
        /// <param name="canMergeCount"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool CanMerge(string definitionId, int existingCount, int incomingCount, out int canMergeCount, out string reason);
    }
}