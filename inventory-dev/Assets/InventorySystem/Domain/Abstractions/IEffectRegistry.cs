namespace GB.Inventory.Domain.Abstractions
{
    public interface IEffectRegistry
    {
        bool TryResolve(string effectKey, out IItemEffect effect);
    }
}