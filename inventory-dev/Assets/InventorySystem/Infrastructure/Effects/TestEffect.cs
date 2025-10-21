using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;

namespace GB.Inventory.Infrastructure.Effects
{
    public sealed class TestEffect : IItemEffect
    {
        public UseResult Apply(ITurnContext ctx, string definitionId, object payload)
        {
            var msg = $"TestEffect OK for {definitionId} in phase {ctx?.Phase ?? "?"}";
            return UseResult.OkConsume(msg);
        }
    }
}