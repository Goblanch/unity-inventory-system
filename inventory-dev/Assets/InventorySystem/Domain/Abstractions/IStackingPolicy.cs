namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Define las reglas para aplicar stacks (maxStack por definición y si se pueden combinar)
    /// </summary>
    public interface IStackingPolicy
    {   
        /// <summary>
        /// Devuelve el máximo por pila para un ítem (según sus metadatos).
        /// </summary>
        /// <param name="definitionId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        int GetMaxPerStack(string definitionId, string typeId);
    }
}