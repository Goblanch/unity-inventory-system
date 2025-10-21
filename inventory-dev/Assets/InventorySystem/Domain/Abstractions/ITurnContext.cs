namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Contexto mínimo de turno para efectos
    /// </summary>
    public interface ITurnContext
    {
        string Phase { get; }
    }
}