using System;

namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Implementación por defecto:
    /// - Si allowedPhasesForItem es null o vacío --> PERMITIR siempre
    /// - Si ctx.Phase es null / vacío --> PERMITIR (sistema sin fases)
    /// - Si hay fases condifuradas --> ctx.Phase debe estar incluida (case-sensitive por simplicidad)
    /// </summary>
    public sealed class DefaultUsagePhasePolicy : IUsagePhasePolicy
    {
        public bool CanUse(string definitionId, string[] allowedPhasesForItem, ITurnContext ctx, out string reason)
        {
            reason = null;

            if (allowedPhasesForItem == null || allowedPhasesForItem.Length == 0)
                return true;

            var phase = ctx?.Phase;
            if (string.IsNullOrWhiteSpace(phase))
                return true;

            for (int i = 0; i < allowedPhasesForItem.Length; i++)
            {
                if (allowedPhasesForItem[i] == phase)
                {
                    return true;
                }
            }

            reason = $"El item {definitionId} no puede usarse en la fase {phase}";
            return false;
        }
    }
}