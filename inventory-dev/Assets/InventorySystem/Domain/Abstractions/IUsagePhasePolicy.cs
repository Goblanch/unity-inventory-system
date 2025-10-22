namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Política que valida si un item puede usarse en la fase actual.
    /// Si no hay fases configuradas para el item, debe permitir por defecto.
    /// </summary>
    public interface IUsagePhasePolicy
    {
        bool CanUse(string definitionId, string[] allowedPhasesForItem, ITurnContext ctx, out string reason);
    }
}