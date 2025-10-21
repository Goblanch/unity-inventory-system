namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Implementación de un efecto item. No depende de Unity. Se ejecuta con contexto de turno.
    /// </summary>
    public interface IItemEffect
    {
        UseResult Apply(ITurnContext ctx, string definitionId, object payload);
    }
}