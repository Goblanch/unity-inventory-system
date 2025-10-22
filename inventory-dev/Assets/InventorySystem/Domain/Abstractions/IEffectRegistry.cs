namespace GB.Inventory.Domain.Abstractions
{
    public interface IEffectRegistry
    {
        bool TryResolve(string effectKey, out IItemEffect effect);

        bool TryGetPayload(string definitionId, out object payload);
        string[] GetAllowedPhases(string definitionId);
    }
}