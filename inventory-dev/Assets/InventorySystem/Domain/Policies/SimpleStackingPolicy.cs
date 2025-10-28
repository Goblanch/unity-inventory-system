using System;
using System.Collections.Generic;
using GB.Inventory.Domain.Abstractions;

namespace GB.Inventory.Domain.Policies
{
    public sealed class SimpleStackingPolicy : IStackingPolicy
    {
        private readonly int _defaultMax;
        private readonly Dictionary<string, int> _perDefMax;

        public SimpleStackingPolicy(int defaultMax = 99, Dictionary<string, int> perDefinitionMax = null)
        {
            _defaultMax = Math.Max(1, defaultMax);
            _perDefMax = perDefinitionMax ?? new Dictionary<string, int>();
        }

        public int GetMaxStack(string definitionId)
        {
            if (definitionId != null && _perDefMax.TryGetValue(definitionId, out var max))
                return Math.Max(1, max);

            return _defaultMax;
        }

        public bool CanMerge(string definitionId, int existingCount, int incomingCount, out int canMergeCount, out string reason)
        {
            reason = null;
            if (incomingCount <= 0)
            {
                canMergeCount = 0;
                reason = "Cantidad a añadir debe ser > 0";
                return false;
            }

            int max = GetMaxStack(definitionId);
            if (existingCount >= max)
            {
                canMergeCount = 0;
                reason = "El stack ya está en el máximo";
                return false;
            }

            canMergeCount = Math.Min(incomingCount, max - existingCount);
            return canMergeCount > 0;
        }

        public int GetMaxPerStack(string definitionId, string typeId)
        {
            throw new NotImplementedException();
        }
    }
}